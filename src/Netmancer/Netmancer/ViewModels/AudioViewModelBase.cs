using Netmancer.Services;

namespace Netmancer.ViewModels;

/// <summary>
/// Base class for ViewModels that expose common audio-service state
/// and automatically forward <see cref="IAudioPlayerService"/>
/// property-change notifications to their own properties.
/// </summary>
public abstract class AudioViewModelBase : ViewModelBase
{
    public string TrackTitle => AudioService.CurrentTrack?.Title ?? string.Empty;
    public string ArtistName => AudioService.CurrentTrack?.Artist ?? string.Empty;
    public string? AlbumArtUri => AudioService.CurrentTrack?.AlbumArtUri;
    public bool IsPlaying => AudioService.IsPlaying;
    public bool IsVisible => AudioService.HasTrack;
    protected IAudioPlayerService AudioService { get; }

    protected AudioViewModelBase(IAudioPlayerService audioService)
    {
        AudioService = audioService;
        audioService.PropertyChanged += OnAudioServicePropertyChanged;
    }

    /// <summary>
    /// Extra service→VM property mappings provided by the subclass.
    /// Entries whose key already exists in the base map are merged
    /// (additional VM property names appended).
    /// </summary>
    protected virtual IReadOnlyDictionary<string, string[]> AdditionalServicePropertyMap { get; } =
        new Dictionary<string, string[]>();

    /// <summary>
    /// Optional hook called after property-change notifications have been
    /// forwarded. Override to handle extra logic (e.g. NotifyCanExecuteChanged).
    /// </summary>
    protected virtual void OnAfterAudioServicePropertyChanged(string propertyName) { }

    private IReadOnlyDictionary<string, string[]> ServicePropertyMap =>
        field ??= BuildPropertyMap();

    private Dictionary<string, string[]> BuildPropertyMap()
    {
        var map = new Dictionary<string, string[]>
        {
            [nameof(IAudioPlayerService.CurrentTrack)] =
                [nameof(TrackTitle), nameof(ArtistName), nameof(AlbumArtUri), nameof(IsVisible)],
            [nameof(IAudioPlayerService.IsPlaying)] =
                [nameof(IsPlaying)],
        };

        foreach (var (key, extraProps) in AdditionalServicePropertyMap)
        {
            if (map.TryGetValue(key, out var existing))
                map[key] = [..existing, ..extraProps];
            else
                map[key] = extraProps;
        }

        return map;
    }

    private void OnAudioServicePropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null
            || !ServicePropertyMap.TryGetValue(e.PropertyName, out var vmProperties))
            return;

        foreach (var property in vmProperties)
            OnPropertyChanged(property);

        OnAfterAudioServicePropertyChanged(e.PropertyName);
    }
}

