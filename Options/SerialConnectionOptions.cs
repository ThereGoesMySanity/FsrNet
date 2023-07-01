namespace FsrNet.Options;

public class SerialConnectionOptions
{
    public required string SerialPort { get; set; }
    public int BaudRate { get; set; } = 115200;
    public int Timeout { get; set; } = 1000;
}