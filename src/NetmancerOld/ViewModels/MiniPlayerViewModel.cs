using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetmancerOld.Services;

namespace NetmancerOld.ViewModels;

public partial class MiniPlayerViewModel : ObservableObject
{
    private static readonly Dictionary<string, string[]> _servicePropertyMap = new()
    {
        [nameof(IAudioPlayerService.CurrentTrack)] =
            [nameof(TrackTitle), nameof(ArtistName), nameof(AlbumArtUri), nameof(IsVisible), nameof(HasAlbumArt)],
        [nameof(IAudioPlayerService.IsPlaying)] =
            [nameof(IsPlaying), nameof(PlayPauseIcon)],
    };

    private readonly IAudioPlayerService _audioService;

    public MiniPlayerViewModel(IAudioPlayerService audioPlayerService)
    {
        _audioService = audioPlayerService;
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
    private async Task OpenNowPlaying() => await Shell.Current.GoToAsync("NowPlaying");

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

