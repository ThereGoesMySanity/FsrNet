using FsrNet.Controllers;
using FsrNet.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace FsrNet.Services;

public class ProfileStore : IDisposable
{
    private readonly SerialConnection serial;
    private readonly ImageStore images;
    private readonly IWebHostEnvironment env;
    private ProfileData _profileData;
    public IHubContext<ProfileHub> Hub;

    public ProfileStore(SerialConnection serial, ImageStore images, IHubContext<ProfileHub> hub, IWebHostEnvironment env)
    {
        this.serial = serial;
        this.images = images;
        Hub = hub;
        this.env = env;
        _profileData = Load();
        if (serial.Connected) SerialInit();
        serial.OnConnected += SerialInit;
    }

    public async void SerialInit()
    {
        await SetImage(_profileData.CurrentProfile.Image);
        var thresholds = _profileData.CurrentProfile.Thresholds;
        for (int i = 0; i < thresholds.Length; i++)
        {
            await SetThreshold(i, thresholds);
        }
    }

    public Defaults GetDefaults()
    {
        return new Defaults
        {
            Profiles = _profileData.Profiles.Keys.ToArray(),
            Images = images.GetImageNames(),
            CurrentProfile = _profileData.CurrentProfileName,
            Data = _profileData.CurrentProfile,
        };
    }

    public async Task SetThreshold(int index, int[] values)
    {
        await serial.WriteThreshold(index, values[index]);
        _profileData.CurrentProfile.Thresholds = values;
        await Broadcast("thresholds", values);
    }

    public async Task SetImage(string image)
    {
        var file = images.GetImageInfo(image);
        if(file.Exists)
        {
            using var stream = file.CreateReadStream();
            await serial.WriteGif(stream);
            _profileData.CurrentProfile.Image = image;
            await Broadcast("image", image);
        }
    }
    public async Task RemoveImage(string image)
    {
        images.RemoveImage(image);
        if (image == _profileData.CurrentProfile.Image)
        {
            await SetImage(images.GetImageNames().First());
        }
        await Broadcast("images", images.GetImageNames());
    }

    public async Task SetCurrentProfile(string name)
    {
        _profileData.CurrentProfileName = name;
        await BroadcastAll();
        await Broadcast("get_cur_profile", name);
    }
    public async Task AddProfile(string name, Profile profile)
    {
        _profileData.Profiles.Add(name, profile);
        await SetCurrentProfile(name);
        await Broadcast("get_profiles", _profileData.Profiles.Keys.ToArray());
    }
    public async Task RemoveProfile(string name)
    {
        _profileData.Profiles.Remove(name);
        if (name == _profileData.CurrentProfileName)
        {
            await SetCurrentProfile(_profileData.Profiles.First().Key);
        }
        await Broadcast("get_profiles", _profileData.Profiles.Keys.ToArray());
    }
    public async Task Broadcast(string message, object value)
    {
        await Hub.Clients.All.SendAsync(message, value);
    }
    public async Task BroadcastAll()
    {
        foreach (var prop in typeof(Profile).GetProperties())
        {
            await Broadcast(prop.Name.ToLower(), prop.GetValue(_profileData.CurrentProfile));
        }
    }

    public ProfileData Load()
    {
        var file = env.WebRootFileProvider.GetFileInfo("profiles.json");
        ProfileData data;
        if (!file.Exists)
        {
            data = new ProfileData();
            Save(data);
        }
        else
        {
            data = JsonConvert.DeserializeObject<ProfileData>(System.IO.File.ReadAllText(file.PhysicalPath));
            foreach (Profile p in data.Profiles.Values)
                if (p.Image == null || !images.GetImageInfo(p.Image).Exists)
                    data.CurrentProfile.Image = "default.gif";
        }
        return data;
    }
    public void Save(ProfileData value)
    {
        System.IO.File.WriteAllText(Path.Join(env.WebRootPath, "profiles.json"), JsonConvert.SerializeObject(value));
    }

    public void Dispose()
    {
        Save(_profileData);
    }

}