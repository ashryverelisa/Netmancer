using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class NowPlayingViewModel : ObservableObject
{
    private readonly IAudioPlayerService _audioService;

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

    private static string FormatTime(double totalSeconds)
    {
        if (totalSeconds <= 0) return "0:00";
        var ts = TimeSpan.FromSeconds(totalSeconds);
        return ts.Hours > 0
            ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes}:{ts.Seconds:D2}";
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
}
