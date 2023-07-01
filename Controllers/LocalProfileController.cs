using FsrNet.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FsrNet.Controllers;
[ApiController]
public class LocalProfileController : ControllerBase
{
    private readonly IOptionsSnapshot<LocalProfileOptions> _options;


    public LocalProfileController(IOptionsSnapshot<LocalProfileOptions> options)
    {
        _options = options;
    }
    [HttpGet("/api/local-profiles")]
    public Dictionary<string, string> GetLocalProfiles()
    {
        string basepath = _options.Value.LocalProfilesPath;
        if (!Path.Exists(basepath)) return null;

        return Directory.EnumerateFileSystemEntries(basepath)
                .Select(d => Path.GetRelativePath(basepath, d))
                .ToDictionary(d => System.IO.File.ReadLines(Path.Combine(basepath, d, "Editable.ini"))
                            .First(l => l.StartsWith("DisplayName="))
                            .Replace("DisplayName=", "")
                            .TrimEnd());
                
    }

    [HttpGet("/api/local-profiles/{id}")]
    public Dictionary<string, string[]> GetLocalProfile(string id)
    {
        string basepath = _options.Value.LocalProfilesPath;
        if (!Path.Exists(basepath)) return null;

        var opts = new EnumerationOptions { AttributesToSkip = FileAttributes.Directory | FileAttributes.Hidden | FileAttributes.System};
        var sspath = Path.Combine(basepath, id, "Screenshots", "Simply_Love");
        if (!Path.Exists(sspath)) return null;

        return Directory.EnumerateDirectories(sspath, "*", SearchOption.AllDirectories)
                .Select(d => (Path.GetRelativePath(sspath, d), 
                    Directory.GetFileSystemEntries(d, "*", opts)
                            .Select(Path.GetFileName).ToArray()))
                .Where(t => t.Item2.Length > 0)
                .ToDictionary(t => t.Item1, t => t.Item2);
    }
    [HttpGet("/api/screenshots/{id}/{**path}")]
    public IActionResult GetScreenshot(string id, string path)
    {
        string basepath = _options.Value.LocalProfilesPath;

        var file = Path.Combine(basepath, id, "Screenshots", "Simply_Love", path);
        if (!System.IO.File.Exists(file) || Path.GetExtension(file) != ".png")
            return NotFound();
        
        return PhysicalFile(file, "image/png");
    }
}