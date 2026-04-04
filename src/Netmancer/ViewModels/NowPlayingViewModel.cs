using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Netmancer.Messages;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class NowPlayingViewModel : ObservableObject,
    IRecipient<MediaCommandRequestedMessage>,
    IRecipient<MediaOpenedMessage>
{
    private readonly IAudioPlayerService _audioService;
    private bool _isDragging;
    private bool _playWhenReady;

    public NowPlayingViewModel(IAudioPlayerService audioPlayerService)
    {
        _audioService = audioPlayerService;
        _audioService.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(IAudioPlayerService.CurrentTrack):
                    OnPropertyChanged(nameof(TrackTitle));
                    OnPropertyChanged(nameof(ArtistName));
                    OnPropertyChanged(nameof(AlbumArtUri));
                    OnPropertyChanged(nameof(IsVisible));
                    break;
                case nameof(IAudioPlayerService.IsPlaying):
                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayPauseIcon));
                    break;
                case nameof(IAudioPlayerService.CanGoNext):
                    OnPropertyChanged(nameof(CanGoNext));
                    NextCommand.NotifyCanExecuteChanged();
                    break;
                case nameof(IAudioPlayerService.CanGoPrevious):
                    OnPropertyChanged(nameof(CanGoPrevious));
                    PreviousCommand.NotifyCanExecuteChanged();
                    break;
            }
        };

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public string TrackTitle => _audioService.CurrentTrack?.Title ?? string.Empty;
    public string ArtistName => _audioService.CurrentTrack?.Artist ?? string.Empty;
    public string? AlbumArtUri => _audioService.CurrentTrack?.AlbumArtUri;
    public bool IsPlaying => _audioService.IsPlaying;
    public bool IsVisible => _audioService.HasTrack;
    public string PlayPauseIcon => _audioService.IsPlaying ? "⏸" : "▶";
    public bool CanGoNext => _audioService.CanGoNext;
    public bool CanGoPrevious => _audioService.CanGoPrevious;

    [ObservableProperty]
    public partial double Volume { get; set; } = 1.0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PositionDisplay))]
    public partial double PositionSeconds { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationDisplay))]
    public partial double DurationSeconds { get; set; }

    public string PositionDisplay => FormatTime(PositionSeconds);
    public string DurationDisplay => FormatTime(DurationSeconds);

    // ── Message handlers ───────────────────────────────────────────────

    public void Receive(MediaCommandRequestedMessage message)
    {
        switch (message.Command)
        {
            case MediaCommand.Play:
                if (_audioService.SourceUrl is not null)
                {
                    _playWhenReady = true;
                    WeakReferenceMessenger.Default.Send(
                        new SetMediaSourceMessage(_audioService.SourceUrl));
                }
                break;

            case MediaCommand.Pause:
                WeakReferenceMessenger.Default.Send(new PausePlaybackMessage());
                break;

            case MediaCommand.Resume:
                WeakReferenceMessenger.Default.Send(new StartPlaybackMessage());
                break;
        }
    }

    public void Receive(MediaOpenedMessage message)
    {
        if (!_playWhenReady) return;
        _playWhenReady = false;
        WeakReferenceMessenger.Default.Send(new StartPlaybackMessage());
    }

    // ── Called by the View on OnAppearing to sync initial state ─────────

    /// <summary>
    /// Synchronises the MediaElement with the current service state when the
    /// page becomes visible. Call from <c>OnAppearing</c>.
    /// </summary>
    public void Activate()
    {
        _isDragging = false;

        if (_audioService.SourceUrl is null) return;

        _playWhenReady = _audioService.IsPlaying;
        WeakReferenceMessenger.Default.Send(
            new SetMediaSourceMessage(_audioService.SourceUrl));
    }

    /// <summary>
    /// Called by the view's timer to push MediaElement position/duration
    /// into the ViewModel. Respects the drag-in-progress flag.
    /// </summary>
    public void UpdatePosition(double position, double duration)
    {
        if (_isDragging) return;
        PositionSeconds = position;
        DurationSeconds = duration;
    }

    // ── Partial property-changed hooks ─────────────────────────────────

    partial void OnVolumeChanged(double value)
        => WeakReferenceMessenger.Default.Send(new SetVolumeMessage(value));

    // ── Commands ───────────────────────────────────────────────────────

    [RelayCommand]
    private void DragStarted() => _isDragging = true;

    [RelayCommand]
    private void DragCompleted()
    {
        _isDragging = false;
        WeakReferenceMessenger.Default.Send(
            new SeekToPositionMessage(PositionSeconds));
    }

    [RelayCommand]
    private void PlayPause() => _audioService.PlayPause();

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void Previous()
    {
        _audioService.Previous();
        PositionSeconds = 0;
        DurationSeconds = 0;
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next()
    {
        _audioService.Next();
        PositionSeconds = 0;
        DurationSeconds = 0;
    }

    [RelayCommand]
    private async Task GoBack() => await Shell.Current.GoToAsync("..");

    // ── Helpers ─────────────────────────────────────────────────────────

    private static string FormatTime(double totalSeconds)
    {
        if (totalSeconds <= 0) return "0:00";
        var ts = TimeSpan.FromSeconds(totalSeconds);
        return ts.Hours > 0
            ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes}:{ts.Seconds:D2}";
    }
}
