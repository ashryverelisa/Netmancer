using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using NetmancerOld.Messages;
using NetmancerOld.Models;

namespace NetmancerOld.Services;

/// <summary>
/// Singleton service that holds audio playback state.
/// The actual MediaElement is controlled by the view; this service exposes
/// observable properties that the view and view-model bind to.
/// </summary>
public partial class AudioPlayerService : ObservableObject, IAudioPlayerService
{
    private List<ContentItem> _playlist = [];
    private int _currentIndex = -1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTrack))]
    public partial ContentItem? CurrentTrack { get; set; }

    [ObservableProperty]
    public partial bool IsPlaying { get; set; }

    [ObservableProperty]
    public partial string? SourceUrl { get; set; }

    public bool HasTrack => CurrentTrack is not null;
    public bool CanGoNext => _currentIndex >= 0 && _currentIndex < _playlist.Count - 1;
    public bool CanGoPrevious => _currentIndex > 0;

    public void Play(ContentItem item, IReadOnlyList<ContentItem>? playlist = null)
    {
        if (playlist is not null)
        {
            _playlist = playlist.ToList();
            _currentIndex = _playlist.IndexOf(item);
        }
        else if (!_playlist.Contains(item))
        {
            _playlist = [item];
            _currentIndex = 0;
        }
        else
        {
            _currentIndex = _playlist.IndexOf(item);
        }

        CurrentTrack = item;
        SourceUrl = item.ResourceUrl;
        IsPlaying = true;
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        WeakReferenceMessenger.Default.Send(new MediaCommandRequestedMessage(MediaCommand.Play));
    }

    public void PlayPause()
    {
        if (CurrentTrack is null) return;

        if (IsPlaying)
        {
            IsPlaying = false;
            WeakReferenceMessenger.Default.Send(new MediaCommandRequestedMessage(MediaCommand.Pause));
        }
        else
        {
            IsPlaying = true;
            WeakReferenceMessenger.Default.Send(new MediaCommandRequestedMessage(MediaCommand.Resume));
        }
    }

    public void Next()
    {
        if (!CanGoNext) return;
        Play(_playlist[_currentIndex + 1]);
    }

    public void Previous()
    {
        if (!CanGoPrevious) return;
        Play(_playlist[_currentIndex - 1]);
    }
}
