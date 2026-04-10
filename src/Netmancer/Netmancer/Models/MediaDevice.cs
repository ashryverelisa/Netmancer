namespace Netmancer.Models;

public class MediaDevice
{
    public string FriendlyName { get; init; } = string.Empty;
    public Uri DescriptionLocation { get; init; } = null!;

    /// <summary>
    /// The host address (IP or hostname) extracted from the description URL.
    /// </summary>
    public string Address => DescriptionLocation?.Host ?? string.Empty;
}

