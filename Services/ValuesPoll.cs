using FsrNet.Controllers;
using Microsoft.AspNetCore.SignalR;

namespace FsrNet.Services;

public class ValuesPoll : IHostedService, IDisposable
{
    private Timer timer;
    private readonly SerialConnection serial;
    private readonly IHubContext<ProfileHub> hub;
    private readonly ILogger<ValuesPoll> logger;

    public ValuesPoll(SerialConnection serial, IHubContext<ProfileHub> hub, ILogger<ValuesPoll> logger)
    {
        this.serial = serial;
        this.hub = hub;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        timer = new Timer(UpdateValues, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
        return Task.CompletedTask;
    }

    private async void UpdateValues(object state)
    {
        if (!serial.Connected || serial.Busy) return;

        logger.LogDebug("Fetching values...");
        int[] values = await serial.GetValues();
        logger.LogDebug(String.Join(',', values));
        await hub.Clients.All.SendAsync("values", values);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        timer?.Dispose();
    }
}
