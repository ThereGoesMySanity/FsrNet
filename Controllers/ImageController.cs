using FsrNet.Options;
using FsrNet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace FsrNet.Controllers;

[ApiController]
[Route("/api/images")]
public class ImageController : ControllerBase
{
    private readonly IHubContext<ProfileHub> hub;
    private readonly ImageStore store;
    private readonly ProfileStore profiles;

    private bool enabled;

    public ImageController(IHubContext<ProfileHub> hub, ImageStore store, ProfileStore profiles, IOptionsMonitor<SerialConnectionOptions> options)
    {
        this.hub = hub;
        this.store = store;
        this.profiles = profiles;
        enabled = options.CurrentValue.ImagesEnabled;
        options.OnChange(o => enabled = o.ImagesEnabled);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Post()
    {
        if (!enabled) return BadRequest();

        var file = Request.Form.Files[0];
        if (!file.ContentType.StartsWith("image/")) return BadRequest("Not an image");
        var image = await store.ConvertAndSave(file.FileName, file.OpenReadStream());
        await hub.Clients.All.SendAsync("images", store.GetImageNames());
        if (image != null) await profiles.SetImage(image);

        return Redirect("/image-select#");
    }
}