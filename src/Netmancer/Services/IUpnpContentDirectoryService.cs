using Netmancer.Models;

namespace Netmancer.Services;

public interface IUpnpContentDirectoryService
{
    /// <summary>
    /// Browses the children of the given object ID on the device at <paramref name="descriptionLocation"/>.
    /// Pass objectId "0" for the root container.
    /// </summary>
    Task<List<ContentItem>> BrowseAsync(Uri descriptionLocation, string objectId = "0");
}
