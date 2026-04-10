using System.ComponentModel;
using Netmancer.Models;

namespace Netmancer.Services;

/// <summary>
/// Manages audio playback state and controls.
/// In the Avalonia version, this service owns actual playback (via LibVLCSharp)
/// rather than delegating to a view-based MediaElement.
/// </summary>
public interface IAudioPlayerService : INotifyPropertyChanged
{
    ContentItem? CurrentTrack { get; }
    bool IsPlaying { get; }
    bool HasTrack { get; }
    bool CanGoNext { get; }
    bool CanGoPrevious { get; }
    string? SourceUrl { get; }

    /// <summary>
    /// Current playback position.
    /// </summary>
    TimeSpan Position { get; }

    /// <summary>
    /// Total duration of the current track.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Playback volume (0.0 – 1.0).
    /// </summary>
    double Volume { get; set; }

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
    /// Seek to the given position.
    /// </summary>
    void SeekTo(TimeSpan position);
}

