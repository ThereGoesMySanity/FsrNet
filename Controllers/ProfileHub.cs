using FsrNet.Models;
using FsrNet.Services;
using Microsoft.AspNetCore.SignalR;

namespace FsrNet.Controllers;

public class ProfileHub : Hub
{
    private readonly ProfileStore store;
    private readonly HubDataStore dataStore;

    public ProfileHub(ProfileStore store, HubDataStore dataStore)
    {
        this.store = store;
        this.dataStore = dataStore;
    }
    public override Task OnConnectedAsync()
    {
        dataStore.ConnectedCount++;
        return base.OnConnectedAsync();
    }
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        dataStore.ConnectedCount--;
        return base.OnDisconnectedAsync(exception);
    }
    public async Task UpdateThreshold(int[] values, int index)
    {
        await store.SetThreshold(index, values);
    }
    public async Task UpdateImage(string image)
    {
        await store.SetImage(image);
    }
    public async Task RemoveImage(string image)
    {
        await store.RemoveImage(image);
    }
    public async Task AddProfile(string name, Profile profile)
    {
        await store.AddProfile(name, profile);
    }
    public async Task RemoveProfile(string name)
    {
        await store.RemoveProfile(name);
    }
    public async Task ChangeProfile(string name)
    {
        await store.SetCurrentProfile(name);
    }
}