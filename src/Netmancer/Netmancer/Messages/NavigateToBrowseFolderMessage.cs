namespace Netmancer.Messages;

/// <summary>
/// Sent when the user wants to browse a folder on a media server.
/// </summary>
public sealed class NavigateToBrowseFolderMessage(string deviceName, string descriptionUrl, string objectId)
{
    public string DeviceName { get; } = deviceName;
    public string DescriptionUrl { get; } = descriptionUrl;
    public string ObjectId { get; } = objectId;
}

