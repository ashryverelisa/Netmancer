using CommunityToolkit.Mvvm.ComponentModel;
using LibVLCSharp.Shared;
using Netmancer.Models;
using Avalonia.Threading;

namespace Netmancer.Services;

/// <summary>
/// Singleton service that manages audio playback using LibVLCSharp.
/// Replaces the MAUI MediaElement-based approach with direct playback.
/// </summary>
public partial class AudioPlayerService : ObservableObject, IAudioPlayerService, IDisposable
{
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayer;
    private List<ContentItem> _playlist = [];
    private int _currentIndex = -1;

    public AudioPlayerService()
    {
        Core.Initialize();
        _libVlc = new LibVLC("--no-video");
        _mediaPlayer = new MediaPlayer(_libVlc);

        _mediaPlayer.Playing += (_, _) =>
            Dispatcher.UIThread.Post(() => IsPlaying = true);
        _mediaPlayer.Paused += (_, _) =>
            Dispatcher.UIThread.Post(() => IsPlaying = false);
        _mediaPlayer.Stopped += (_, _) =>
            Dispatcher.UIThread.Post(() => IsPlaying = false);
        _mediaPlayer.EndReached += (_, _) =>
            Dispatcher.UIThread.Post(OnTrackEnded);
    }

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

    public TimeSpan Position =>
        _mediaPlayer.IsPlaying || _mediaPlayer.Time >= 0
            ? TimeSpan.FromMilliseconds(Math.Max(0, _mediaPlayer.Time))
            : TimeSpan.Zero;

    public TimeSpan Duration =>
        _mediaPlayer.Length > 0
            ? TimeSpan.FromMilliseconds(_mediaPlayer.Length)
            : TimeSpan.Zero;

    public double Volume
    {
        get => _mediaPlayer.Volume / 100.0;
        set => _mediaPlayer.Volume = (int)Math.Clamp(value * 100, 0, 100);
    }

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
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));

        if (item.ResourceUrl is not null)
        {
            using var media = new Media(_libVlc, new Uri(item.ResourceUrl));
            _mediaPlayer.Play(media);
        }
    }

    public void PlayPause()
    {
        if (CurrentTrack is null) return;

        if (IsPlaying)
            _mediaPlayer.Pause();
        else
            _mediaPlayer.Play();
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

    public void SeekTo(TimeSpan position)
    {
        if (_mediaPlayer.Length > 0)
            _mediaPlayer.Time = (long)position.TotalMilliseconds;
    }

    private void OnTrackEnded()
    {
        if (CanGoNext)
            Next();
        else
            IsPlaying = false;
    }

    public void Dispose()
    {
        _mediaPlayer.Dispose();
        _libVlc.Dispose();
        GC.SuppressFinalize(this);
    }
}

