using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using NetmancerOld.Messages;
using NetmancerOld.Models;
using NetmancerOld.Services;

namespace NetmancerOld.ViewModels;

public partial class NowPlayingViewModel : ObservableObject,
    IRecipient<MediaCommandRequestedMessage>
{
    /// <summary>
    /// Declarative mapping from <see cref="IAudioPlayerService"/> property
    /// names to the ViewModel properties that must be refreshed when they change.
    /// </summary>
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
    private bool _isActive;

    public NowPlayingViewModel(IAudioPlayerService audioPlayerService)
    {
        _audioService = audioPlayerService;
        _audioService.PropertyChanged += OnAudioServicePropertyChanged;

        Position = new PlaybackPositionModel
        {
            SeekRequested = seconds =>
                WeakReferenceMessenger.Default.Send(new SeekToPositionMessage(seconds))
        };

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public string TrackTitle   => _audioService.CurrentTrack?.Title ?? string.Empty;
    public string ArtistName   => _audioService.CurrentTrack?.Artist ?? string.Empty;
    public string? AlbumArtUri => _audioService.CurrentTrack?.AlbumArtUri;
    public bool IsPlaying      => _audioService.IsPlaying;
    public bool IsVisible      => _audioService.HasTrack;
    public string PlayPauseIcon => IsPlaying ? "⏸" : "▶";
    public bool CanGoNext      => _audioService.CanGoNext;
    public bool CanGoPrevious  => _audioService.CanGoPrevious;

    [ObservableProperty]
    public partial double Volume { get; set; } = 1.0;

    /// <summary>
    /// Bound to <c>MediaElement.ShouldAutoPlay</c>.
    /// Set <c>true</c> before assigning <see cref="PlayerSource"/> so the
    /// MediaElement starts playback automatically once the source is loaded.
    /// </summary>
    [ObservableProperty]
    public partial bool AutoPlay { get; set; }

    [ObservableProperty]
    public partial MediaSource? PlayerSource { get; set; }

    public PlaybackPositionModel Position { get; }

    public void Receive(MediaCommandRequestedMessage message)
    {
        switch (message.Command)
        {
            case MediaCommand.Play:   HandlePlay();   break;
            case MediaCommand.Pause:  HandlePause();  break;
            case MediaCommand.Resume: HandleResume(); break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandlePlay()
    {
        if (_audioService.SourceUrl is null) return;

        AutoPlay = true;

        // Only push the source to the MediaElement when the page
        // is visible (handler attached).  Otherwise Activate()
        // will pick it up when the page appears.
        if (_isActive)
            LoadSource(_audioService.SourceUrl);
    }

    private void HandlePause()
    {
        AutoPlay = false;
        WeakReferenceMessenger.Default.Send(new PausePlaybackMessage());
    }

    private void HandleResume()
    {
        AutoPlay = true;
        WeakReferenceMessenger.Default.Send(new StartPlaybackMessage());
    }

    /// <summary>
    /// Synchronises the MediaElement with the current service state when the
    /// page becomes visible. Call from <c>OnAppearing</c>.
    /// </summary>
    public void Activate()
    {
        _isActive = true;

        if (_audioService.SourceUrl is null) return;

        AutoPlay = _audioService.IsPlaying;
        LoadSource(_audioService.SourceUrl);
    }

    /// <summary>
    /// Marks the page as inactive so messages don't try to push a source
    /// before the MediaElement handler is attached.
    /// Call from <c>OnDisappearing</c>.
    /// </summary>
    public void Deactivate() => _isActive = false;


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
    private async Task GoBack() => await Shell.Current.GoToAsync("..");

    private void OnAudioServicePropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null
            || !_servicePropertyMap.TryGetValue(e.PropertyName, out var vmProperties))
            return;

        foreach (var property in vmProperties)
            OnPropertyChanged(property);

        // Refresh CanExecute for navigation commands when their guards change.
        if (e.PropertyName == nameof(IAudioPlayerService.CanGoNext))
            NextCommand.NotifyCanExecuteChanged();
        else if (e.PropertyName == nameof(IAudioPlayerService.CanGoPrevious))
            PreviousCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Resets <see cref="PlayerSource"/> to <c>null</c> first so the
    /// MediaElement always sees a genuine property change, even when the
    /// URL hasn't changed (e.g. navigating back to the same track).
    /// </summary>
    private void LoadSource(string url)
    {
        PlayerSource = null;
        PlayerSource = MediaSource.FromUri(url);
    }
}
