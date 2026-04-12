using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Netmancer.Messages;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class MiniPlayerViewModel : ViewModelBase
{
    private static readonly Dictionary<string, string[]> _servicePropertyMap = new()
    {
        [nameof(IAudioPlayerService.CurrentTrack)] =
            [nameof(TrackTitle), nameof(ArtistName), nameof(AlbumArtUri), nameof(IsVisible), nameof(HasAlbumArt)],
        [nameof(IAudioPlayerService.IsPlaying)] =
            [nameof(IsPlaying)],
    };

    private readonly IAudioPlayerService _audioService;

    public MiniPlayerViewModel(
        IAudioPlayerService audioPlayerService)
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

    [RelayCommand]
    private void PlayPause() => _audioService.PlayPause();

    [RelayCommand]
    private void OpenNowPlaying()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToNowPlayingMessage());
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