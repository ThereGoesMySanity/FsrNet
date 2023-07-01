using FsrNet.Controllers;
using FsrNet.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace FsrNet.Services;

public class ValuesPoll : BackgroundService, IDisposable
{
    private readonly SerialConnection serial;
    private readonly IHubContext<ProfileHub> hub;
    private readonly ILogger<ValuesPoll> logger;

    private ValuesPollOptions options;

    public ValuesPoll(SerialConnection serial, IHubContext<ProfileHub> hub, IOptionsMonitor<ValuesPollOptions> options, ILogger<ValuesPoll> logger)
    {
        this.serial = serial;
        this.hub = hub;
        this.logger = logger;
        this.options = options.CurrentValue;
        options.OnChange(o => this.options = o);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!serial.Connected) await Task.Delay(TimeSpan.FromSeconds(1));

        while (!cancellationToken.IsCancellationRequested)
        {
            await UpdateValues();
            await Task.Delay(TimeSpan.FromMilliseconds(options.PollingDelay));
        }
    }

    private async Task UpdateValues()
    {
        if (!serial.Connected || serial.Busy) return;

        logger.LogDebug("Fetching values...");
        int[] values = await serial.GetValues();
        logger.LogDebug(String.Join(',', values));
        await hub.Clients.All.SendAsync("values", values);
    }
}
