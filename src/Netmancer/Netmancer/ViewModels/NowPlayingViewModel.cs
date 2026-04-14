using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Models;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class NowPlayingViewModel : AudioViewModelBase
{
    protected override IReadOnlyDictionary<string, string[]> ServicePropertyMap { get; } =
        new Dictionary<string, string[]>
        {
            [nameof(IAudioPlayerService.CurrentTrack)] =
                [nameof(TrackTitle), nameof(ArtistName), nameof(AlbumArtUri), nameof(IsVisible)],
            [nameof(IAudioPlayerService.IsPlaying)] =
                [nameof(IsPlaying)],
            [nameof(IAudioPlayerService.CanGoNext)]     = [nameof(CanGoNext)],
            [nameof(IAudioPlayerService.CanGoPrevious)] = [nameof(CanGoPrevious)],
        };

    private readonly INavigationService _navigationService;
    private DispatcherTimer? _positionTimer;

    public NowPlayingViewModel(
        IAudioPlayerService audioPlayerService,
        INavigationService navigationService)
        : base(audioPlayerService)
    {
        _navigationService = navigationService;

        Position = new PlaybackPositionModel
        {
            SeekRequested = seconds =>
                AudioService.SeekTo(TimeSpan.FromSeconds(seconds))
        };
    }

    public string TrackTitle   => AudioService.CurrentTrack?.Title ?? string.Empty;
    public string ArtistName   => AudioService.CurrentTrack?.Artist ?? string.Empty;
    public string? AlbumArtUri => AudioService.CurrentTrack?.AlbumArtUri;
    public bool IsPlaying      => AudioService.IsPlaying;
    public bool IsVisible      => AudioService.HasTrack;
    public bool CanGoNext      => AudioService.CanGoNext;
    public bool CanGoPrevious  => AudioService.CanGoPrevious;

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
                AudioService.Position.TotalSeconds,
                AudioService.Duration.TotalSeconds);
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
    private void PlayPause() => AudioService.PlayPause();

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void Previous()
    {
        AudioService.Previous();
        Position.Reset();
    }

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next()
    {
        AudioService.Next();
        Position.Reset();
    }

    [RelayCommand]
    private void GoBack() => _navigationService.GoBack();

    protected override void OnAfterAudioServicePropertyChanged(string propertyName)
    {
        switch (propertyName)
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