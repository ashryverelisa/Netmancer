using System.ComponentModel;
using Netmancer.Models;

namespace Netmancer.Services;

/// <summary>
/// Manages audio playback state and controls.
/// </summary>
public interface IAudioPlayerService : INotifyPropertyChanged
{
    ContentItem? CurrentTrack { get; }
    bool IsPlaying { get; }
    bool HasTrack { get; }
    bool CanGoNext { get; }
    bool CanGoPrevious { get; }

    /// <summary>
    /// Start playing the given audio item, optionally within a playlist.
    /// </summary>
    void Play(ContentItem item, IReadOnlyList<ContentItem>? playlist = null);

    /// <summary>
    /// Toggle between play and pause.
    /// </summary>
    void PlayPause();

    /// <summary>
    /// Skip to the next track in the playlist.
    /// </summary>
    void Next();

    /// <summary>
    /// Skip to the previous track in the playlist.
    /// </summary>
    void Previous();

    /// <summary>
    /// The MediaElement source URI — the view binds to this to drive the actual player.
    /// </summary>
    string? SourceUrl { get; }

    /// <summary>
    /// Raised when the view should execute a media command (Play, Pause, Stop).
    /// </summary>
    event Action<MediaCommand>? MediaCommandRequested;
}
