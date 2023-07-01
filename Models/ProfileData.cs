using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace FsrNet.Models;

public class ProfileData
{
    public string CurrentProfileName { get; set; } = "";
    [JsonIgnore]
    [IgnoreDataMember]
    public Profile CurrentProfile => Profiles.GetValueOrDefault(CurrentProfileName, new Profile());
    public Dictionary<string, Profile> Profiles { get; set; } = new Dictionary<string, Profile> { { "", new Profile() } };
}