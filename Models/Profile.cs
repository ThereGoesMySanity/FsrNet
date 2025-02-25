namespace FsrNet.Models;

public class Profile
{
    public string Image { get; set; } = "default.gif";
    public int[] Thresholds { get; set; } = [.. Enumerable.Repeat(1000, 8)];
}