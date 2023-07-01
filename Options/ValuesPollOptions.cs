using System.ComponentModel.DataAnnotations;

namespace FsrNet.Options;

public class ValuesPollOptions
{
    [Range(5, int.MaxValue)]
    public int PollingDelay { get; set; } = 10;
}