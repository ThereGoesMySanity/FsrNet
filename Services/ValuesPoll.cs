using FsrNet.Controllers;
using FsrNet.Options;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace FsrNet.Services;

public class ValuesPoll : BackgroundService, IDisposable
{
    private readonly SerialConnection serial;
    private readonly IHubContext<ProfileHub> hub;
    private readonly HubDataStore hubData;
    private readonly ILogger<ValuesPoll> logger;

    private ValuesPollOptions options;

    public ValuesPoll(SerialConnection serial, IHubContext<ProfileHub> hub, HubDataStore hubData, IOptionsMonitor<ValuesPollOptions> options, ILogger<ValuesPoll> logger)
    {
        this.serial = serial;
        this.hub = hub;
        this.hubData = hubData;
        this.logger = logger;
        this.options = options.CurrentValue;
        options.OnChange(o => this.options = o);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (hubData.ConnectedCount > 0) await UpdateValues(cancellationToken);

            var delay = serial.Connected? TimeSpan.FromMilliseconds(options.PollingDelay) : TimeSpan.FromSeconds(1);
            await Task.Delay(delay, cancellationToken);
        }
    }

    private async Task UpdateValues(CancellationToken cancellationToken)
    {
        if (!serial.Connected) return;

        logger.LogDebug("Fetching values...");
        int[]? values = await serial.TryGetValues(cancellationToken);
        if (values is null) return;

        logger.LogDebug("{}", string.Join(',', values));
        await hub.Clients.All.SendAsync("values", values, cancellationToken);
    }
}
