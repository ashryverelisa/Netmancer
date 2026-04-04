using CommunityToolkit.Mvvm.ComponentModel;
using Netmancer.Models;

namespace Netmancer.Services;

/// <summary>
/// Singleton service that holds audio playback state.
/// The actual MediaElement is controlled by the view; this service exposes
/// observable properties that the view and view-model bind to.
/// </summary>
public partial class AudioPlayerService : ObservableObject, IAudioPlayerService
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTrack))]
    public partial ContentItem? CurrentTrack { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial string? SourceUrl { get; set; }

    public bool HasTrack => CurrentTrack is not null;

    /// <summary>
    /// Raised when the view should execute a media command (Play, Pause, Stop).
    /// </summary>
    public event Action<MediaCommand>? MediaCommandRequested;

    public void Play(ContentItem item)
    {
        CurrentTrack = item;
        SourceUrl = item.ResourceUrl;
        IsPlaying = true;
        MediaCommandRequested?.Invoke(MediaCommand.Play);
    }

    public void PlayPause()
    {
        if (CurrentTrack is null) return;

        if (IsPlaying)
        {
            IsPlaying = false;
            MediaCommandRequested?.Invoke(MediaCommand.Pause);
        }
        else
        {
            IsPlaying = true;
            MediaCommandRequested?.Invoke(MediaCommand.Resume);
        }
    }

    public void Stop()
    {
        IsPlaying = false;
        SourceUrl = null;
        CurrentTrack = null;
        MediaCommandRequested?.Invoke(MediaCommand.Stop);
    }
}
