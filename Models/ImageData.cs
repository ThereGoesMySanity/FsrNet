namespace FsrNet.Models;
public class ImageData
{
    public required string CurImage { get; set; }
    public required IEnumerable<string> Images { get; set; }
}