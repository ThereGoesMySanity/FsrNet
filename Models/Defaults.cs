using System.Runtime.Serialization;

namespace FsrNet.Models;

public class Defaults
{
    public required string[] Profiles { get; set; }
    public string[]? Images { get; set; }
    public required string CurrentProfile { get; set; }
    public required Profile Data { get; set; }
}