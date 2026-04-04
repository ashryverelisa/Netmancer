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

    /// <summary>
    /// Start playing the given audio item.
    /// </summary>
    void Play(ContentItem item);

    /// <summary>
    /// Toggle between play and pause.
    /// </summary>
    void PlayPause();

    /// <summary>
    /// Stop playback and clear the current track.
    /// </summary>
    void Stop();

    /// <summary>
    /// The MediaElement source URI — the view binds to this to drive the actual player.
    /// </summary>
    string? SourceUrl { get; }

    /// <summary>
    /// Raised when the view should execute a media command (Play, Pause, Stop).
    /// </summary>
    event Action<MediaCommand>? MediaCommandRequested;
}
