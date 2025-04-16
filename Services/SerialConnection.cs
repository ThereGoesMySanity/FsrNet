using FsrNet.Options;
using Microsoft.Extensions.Options;
using System.IO.Ports;

namespace FsrNet.Services;

public class SerialConnection : IDisposable
{
    private SerialPort? _socket;
    private SerialConnectionOptions options;
    private SemaphoreSlim serial;
    private bool connected => _socket?.IsOpen ?? false;

    public bool Connected
    {
        get 
        {
            if (!connected) InitSocket(options);
            return connected;
        }
    } 
    public bool ImagesEnabled => options.ImagesEnabled;

    public event Action? OnConnected;

    public SerialConnection(IOptionsMonitor<SerialConnectionOptions> options)
    {
        serial = new SemaphoreSlim(1);
        this.options = options.CurrentValue;
        InitSocket(options.CurrentValue);
        options.OnChange(InitSocket);
    }
    private void InitSocket(SerialConnectionOptions options)
    {
        this.options = options;

        if (connected) 
        {
            _socket?.Close();
        }
        _socket = null;

        if (options.SerialPort == null) return;

        if (File.Exists(options.SerialPort))
        {
            try
            {
                _socket = new SerialPort(options.SerialPort, options.BaudRate)
                {
                    ReadTimeout = options.Timeout,
                    WriteTimeout = options.Timeout
                };
                _socket.Open();
                OnConnected?.Invoke();
            }
            catch (Exception)
            {
                _socket?.Close();
            }
        }
    }

    public async Task<int[]?> TryGetValues(CancellationToken cancellationToken) => await Get("v", cancellationToken);
    public async Task<int[]?> TryGetThresholds(CancellationToken cancellationToken) => await Get("t", cancellationToken);

    private async Task<int[]?> Get(string type, CancellationToken cancellationToken, bool blocking = true)
    {
        if (!Connected) return null;
        if (!blocking && serial.CurrentCount == 0) return null;

        await serial.WaitAsync(cancellationToken);

        try
        {
            while (_socket!.BytesToRead > 0)
            {
                _socket.ReadLine();
            }
            _socket.WriteLine(type);

            var vals = _socket.ReadLine()
                    .Split(" ")
                    .Skip(1)
                    .Select(int.Parse)
                    .ToArray();

            return vals;
        }
        catch (Exception)
        {
            _socket!.Close();
        }
        finally
        {
            serial.Release();
        }
        return null;
    }

    public async Task WriteThreshold(int index, int threshold)
    {
        if (!Connected) return;

        await serial.WaitAsync();
        try
        {
            _socket!.WriteLine($"{index} {threshold}");
        }
        catch (Exception) { _socket!.Close(); }
        finally
        {
            serial.Release();
        }
    }

    public async Task WriteGif(Stream gif)
    {
        if (!Connected) return;
        if (!options.ImagesEnabled) return;

        await serial.WaitAsync();

        try
        {
            _socket!.WriteLine($"g {gif.Length}");
            await gif.CopyToAsync(_socket.BaseStream);
            _socket.ReadLine();
        } 
        catch (Exception) { _socket!.Close(); }
        finally
        {
            serial.Release();
        }
    }

    public void Dispose()
    {
        _socket?.Dispose();
    }
}