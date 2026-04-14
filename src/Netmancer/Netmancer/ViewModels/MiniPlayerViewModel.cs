using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Netmancer.Messages;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class MiniPlayerViewModel(IAudioPlayerService audioPlayerService) : AudioViewModelBase(audioPlayerService)
{
    protected override IReadOnlyDictionary<string, string[]> AdditionalServicePropertyMap { get; } =
        new Dictionary<string, string[]>
        {
            [nameof(IAudioPlayerService.CurrentTrack)] = [nameof(HasAlbumArt)],
        };

    public bool HasAlbumArt => !string.IsNullOrEmpty(AlbumArtUri);

    [RelayCommand]
    private void PlayPause() => AudioService.PlayPause();

    [RelayCommand]
    private void OpenNowPlaying() => WeakReferenceMessenger.Default.Send(new NavigateToNowPlayingMessage());
}