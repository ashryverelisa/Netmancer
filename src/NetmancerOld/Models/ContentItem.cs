namespace NetmancerOld.Models;

public class ContentItem
{
    public string Id { get; init; } = string.Empty;
    public string ParentId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool IsContainer { get; init; }

    /// <summary>
    /// The streaming / download URL from the &lt;res&gt; element (files only).
    /// </summary>
    public string? ResourceUrl { get; init; }

    /// <summary>
    /// Artist name from the &lt;upnp:artist&gt; or &lt;dc:creator&gt; element.
    /// </summary>
    public string? Artist { get; init; }

    /// <summary>
    /// Album art / cover URL from the &lt;upnp:albumArtURI&gt; element (audio items only).
    /// </summary>
    public string? AlbumArtUri { get; init; }

    /// <summary>
    /// Whether album art is available for display.
    /// </summary>
    public bool HasAlbumArt => !string.IsNullOrEmpty(AlbumArtUri);

    /// <summary>
    /// UPnP class, e.g. "object.item.audioItem.musicTrack", "object.item.videoItem", etc.
    /// </summary>
    public string MediaClass { get; init; } = string.Empty;

    /// <summary>
    /// A user-friendly icon string derived from the media class.
    /// </summary>
    public string Icon =>
        IsContainer ? "📁" :
        MediaClass.Contains("audio", StringComparison.OrdinalIgnoreCase) ? "🎵" :
        MediaClass.Contains("video", StringComparison.OrdinalIgnoreCase) ? "🎬" :
        MediaClass.Contains("image", StringComparison.OrdinalIgnoreCase) ? "🖼️" :
        "📄";

    /// <summary>
    /// Short description shown below the title for files.
    /// </summary>
    public string Subtitle =>
        IsContainer ? "" :
        MediaClass.Contains("audio", StringComparison.OrdinalIgnoreCase)
            ? (!string.IsNullOrEmpty(Artist) ? Artist : "Audio") :
        MediaClass.Contains("video", StringComparison.OrdinalIgnoreCase) ? "Video" :
        MediaClass.Contains("image", StringComparison.OrdinalIgnoreCase) ? "Image" :
        "File";
}
