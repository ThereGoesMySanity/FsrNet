namespace FsrNet.Options;

public class SerialConnectionOptions
{
    public required string SerialPort { get; set; }
    public bool ImagesEnabled { get; set; } = false;
    public int BaudRate { get; set; } = 115200;
    public int Timeout { get; set; } = 1000;
}