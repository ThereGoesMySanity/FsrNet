using FsrNet.Options;
using Microsoft.Extensions.Options;
using System.IO.Ports;

namespace FsrNet.Services;

public class SerialConnection : IDisposable
{
    private SerialPort? _socket;
    private FileSystemWatcher? _watcher;
    private SerialConnectionOptions options;
    private SemaphoreSlim serial;

    public bool Connected => _socket?.IsOpen ?? false;
    public bool ImagesEnabled => options.ImagesEnabled;

    public event Action? OnConnected;

    public SerialConnection(IOptionsMonitor<SerialConnectionOptions> options)
    {
        serial = new SemaphoreSlim(1);
        this.options = options.CurrentValue;
        initSocket(options.CurrentValue);
        options.OnChange(initSocket);
    }
    private void initSocket(SerialConnectionOptions options)
    {
        if (_watcher != null && this.options.SerialPort == options.SerialPort) return;
        this.options = options;

        if (Connected) 
        {
            _socket?.Close();
        }
        _socket = null;

        if (options.SerialPort == null) return;

        if (!File.Exists(options.SerialPort))
        {
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(options.SerialPort)!);
            _watcher.NotifyFilter = NotifyFilters.FileName;
            _watcher.EnableRaisingEvents = true;
            _watcher.Created += socketCreated;
        }
        else
        {
            _socket = new SerialPort(options.SerialPort, options.BaudRate)
            {
                ReadTimeout = options.Timeout,
                WriteTimeout = options.Timeout
            };
            _socket.Open();
            OnConnected?.Invoke();
        }
    }
    private void socketCreated(object source, FileSystemEventArgs e)
    {
        if (e.FullPath == options.SerialPort)
        {
            _watcher?.Dispose();
            _watcher = null;
            initSocket(options);
        }
    }

    public async Task<int[]?> TryGetValues() => await Get("v");
    public async Task<int[]> GetValues() => (await Get("v"))!;
    public async Task<int[]> GetThresholds() => (await Get("t"))!;

    private async Task<int[]?> Get(string type, bool blocking = true)
    {
        if (!Connected) return null;
        if (!blocking && serial.CurrentCount == 0) return null;

        await serial.WaitAsync();

        //clear data from buffer before write
        _socket!.ReadExisting();
        _socket.WriteLine(type);

        var vals = _socket.ReadLine()
                .Split(" ")
                .Skip(1)
                .Select(int.Parse)
                .ToArray();

        serial.Release();
        return vals;
    }

    public async Task WriteThreshold(int index, int threshold)
    {
        if (!Connected) return;
        await serial.WaitAsync();
        _socket!.WriteLine($"{index} {threshold}");
        serial.Release();
    }

    public async Task WriteGif(Stream gif)
    {
        if (!Connected || !options.ImagesEnabled) return;

        await serial.WaitAsync();

        _socket!.WriteLine($"g {gif.Length}");
        await gif.CopyToAsync(_socket.BaseStream);
        _socket.ReadLine();

        serial.Release();
    }

    public void Dispose()
    {
        _socket?.Dispose();
        _watcher?.Dispose();
    }
}