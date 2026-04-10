using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Models;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class NowPlayingViewModel : ViewModelBase
{
    private static readonly Dictionary<string, string[]> _servicePropertyMap = new()
    {
        [nameof(IAudioPlayerService.CurrentTrack)] =
            [nameof(TrackTitle), nameof(ArtistName), nameof(AlbumArtUri), nameof(IsVisible)],
        [nameof(IAudioPlayerService.IsPlaying)] =
            [nameof(IsPlaying), nameof(PlayPauseIcon)],
        [nameof(IAudioPlayerService.CanGoNext)]     = [nameof(CanGoNext)],
        [nameof(IAudioPlayerService.CanGoPrevious)] = [nameof(CanGoPrevious)],
    };

    private readonly IAudioPlayerService _audioService;
    private readonly INavigationService _navigationService;
    private DispatcherTimer? _positionTimer;

    public NowPlayingViewModel(
        IAudioPlayerService audioPlayerService,
        INavigationService navigationService)
    {
        _audioService = audioPlayerService;
        _navigationService = navigationService;
        _audioService.PropertyChanged += OnAudioServicePropertyChanged;

        Position = new PlaybackPositionModel
        {
            SeekRequested = seconds =>
                _audioService.SeekTo(TimeSpan.FromSeconds(seconds))
        };
    }

    public string TrackTitle   => _audioService.CurrentTrack?.Title ?? string.Empty;
    public string ArtistName   => _audioService.CurrentTrack?.Artist ?? string.Empty;
    public string? AlbumArtUri => _audioService.CurrentTrack?.AlbumArtUri;
    public bool IsPlaying      => _audioService.IsPlaying;
    public bool IsVisible      => _audioService.HasTrack;
    public string PlayPauseIcon => IsPlaying ? "⏸" : "▶";
    public bool CanGoNext      => _audioService.CanGoNext;
    public bool CanGoPrevious  => _audioService.CanGoPrevious;

    public PlaybackPositionModel Position { get; }

    /// <summary>
    /// Starts a timer to poll position/duration from the audio service.
    /// Called when the view is attached.
    /// </summary>
    public void Activate()
    {
        _positionTimer?.Stop();
        _positionTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _positionTimer.Tick += (_, _) =>
            Position.Update(
                _audioService.Position.TotalSeconds,
                _audioService.Duration.TotalSeconds);
        _positionTimer.Start();
    }

    /// <summary>
    /// Stops the position polling timer.
    /// Called when the view is detached.
    /// </summary>
    public void Deactivate()
    {
        _positionTimer?.Stop();
        _positionTimer = null;
    }

    [RelayCommand]
    private void PlayPause() => _audioService.PlayPause();

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void Previous()
    {
        _audioService.Previous();
        Position.Reset();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next()
    {
        _audioService.Next();
        Position.Reset();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    private void OnAudioServicePropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null
            || !_servicePropertyMap.TryGetValue(e.PropertyName, out var vmProperties))
            return;

        foreach (var property in vmProperties)
            OnPropertyChanged(property);

        switch (e.PropertyName)
        {
            case nameof(IAudioPlayerService.CanGoNext):
                NextCommand.NotifyCanExecuteChanged();
                break;
            case nameof(IAudioPlayerService.CanGoPrevious):
                PreviousCommand.NotifyCanExecuteChanged();
                break;
        }
    }
}