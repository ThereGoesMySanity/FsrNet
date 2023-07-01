using System.Runtime.Serialization;

namespace FsrNet.Models;

public class Defaults
{
    public string[] Profiles { get; set; }
    public string[] Images { get; set; }
    public string CurrentProfile { get; set; }
    public Profile Data { get; set; }
}