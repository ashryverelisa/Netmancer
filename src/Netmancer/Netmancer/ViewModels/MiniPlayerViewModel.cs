using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Netmancer.Messages;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class MiniPlayerViewModel(IAudioPlayerService audioPlayerService) : AudioViewModelBase(audioPlayerService)
{
    protected override IReadOnlyDictionary<string, string[]> ServicePropertyMap { get; } =
        new Dictionary<string, string[]>
        {
            [nameof(IAudioPlayerService.CurrentTrack)] =
                [nameof(TrackTitle), nameof(ArtistName), nameof(AlbumArtUri), nameof(IsVisible), nameof(HasAlbumArt)],
            [nameof(IAudioPlayerService.IsPlaying)] =
                [nameof(IsPlaying)],
        };

    public string TrackTitle => AudioService.CurrentTrack?.Title ?? string.Empty;
    public string ArtistName => AudioService.CurrentTrack?.Artist ?? string.Empty;
    public string? AlbumArtUri => AudioService.CurrentTrack?.AlbumArtUri;
    public bool HasAlbumArt => !string.IsNullOrEmpty(AlbumArtUri);
    public bool IsPlaying => AudioService.IsPlaying;
    public bool IsVisible => AudioService.HasTrack;

    [RelayCommand]
    private void PlayPause() => AudioService.PlayPause();

    [RelayCommand]
    private void OpenNowPlaying()
    {
        WeakReferenceMessenger.Default.Send(new NavigateToNowPlayingMessage());
    }
}