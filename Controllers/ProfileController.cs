using FsrNet.Models;
using FsrNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace FsrNet.Controllers;

[ApiController]
[Route("/api/profiles")]
public class ProfileController : ControllerBase
{
    private readonly ProfileStore store;

    public ProfileController(ProfileStore store)
    {
        this.store = store;
    }

    [HttpGet]
    public Defaults Get()
    {
        return store.GetDefaults();
    }
}