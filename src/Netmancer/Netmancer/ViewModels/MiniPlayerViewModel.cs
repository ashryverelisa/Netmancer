using CommunityToolkit.Mvvm.Input;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class MiniPlayerViewModel : ViewModelBase
{
    private static readonly Dictionary<string, string[]> _servicePropertyMap = new()
    {
        [nameof(IAudioPlayerService.CurrentTrack)] =
            [nameof(TrackTitle), nameof(ArtistName), nameof(AlbumArtUri), nameof(IsVisible), nameof(HasAlbumArt)],
        [nameof(IAudioPlayerService.IsPlaying)] =
            [nameof(IsPlaying), nameof(PlayPauseIcon)],
    };

    private readonly IAudioPlayerService _audioService;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;

    public MiniPlayerViewModel(
        IAudioPlayerService audioPlayerService,
        INavigationService navigationService,
        IServiceProvider serviceProvider)
    {
        _audioService = audioPlayerService;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
        _audioService.PropertyChanged += OnAudioServicePropertyChanged;
    }

    public string TrackTitle => _audioService.CurrentTrack?.Title ?? string.Empty;
    public string ArtistName => _audioService.CurrentTrack?.Artist ?? string.Empty;
    public string? AlbumArtUri => _audioService.CurrentTrack?.AlbumArtUri;
    public bool HasAlbumArt => !string.IsNullOrEmpty(AlbumArtUri);
    public bool IsPlaying => _audioService.IsPlaying;
    public bool IsVisible => _audioService.HasTrack;
    public string PlayPauseIcon => IsPlaying ? "⏸" : "▶";

    [RelayCommand]
    private void PlayPause() => _audioService.PlayPause();

    [RelayCommand]
    private void OpenNowPlaying()
    {
        var nowPlaying = (NowPlayingViewModel)_serviceProvider.GetService(typeof(NowPlayingViewModel))!;
        _navigationService.NavigateTo(nowPlaying);
    }

    private void OnAudioServicePropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null
            || !_servicePropertyMap.TryGetValue(e.PropertyName, out var vmProperties))
            return;

        foreach (var property in vmProperties)
            OnPropertyChanged(property);
    }
}

