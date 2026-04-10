using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Netmancer.Models;

/// <summary>
/// Encapsulates playback position and duration tracking, including
/// slider drag state. Owned by the NowPlaying ViewModel.
/// </summary>
public partial class PlaybackPositionModel : ObservableObject
{
    private bool _isDragging;
    private bool _isUpdatingFromService;

    /// <summary>
    /// Invoked when the user changes the position (click or drag release),
    /// passing the desired position in seconds.
    /// </summary>
    public Action<double>? SeekRequested { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PositionDisplay))]
    public partial double PositionSeconds { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DurationDisplay))]
    public partial double DurationSeconds { get; set; }

    public string PositionDisplay => FormatTime(PositionSeconds);
    public string DurationDisplay => FormatTime(DurationSeconds);

    /// <summary>
    /// Called when <see cref="PositionSeconds"/> changes.
    /// If the change was initiated by the user (click or drag on slider)
    /// rather than the polling timer, trigger a seek.
    /// </summary>
    partial void OnPositionSecondsChanged(double value)
    {
        if (!_isUpdatingFromService && !_isDragging)
            SeekRequested?.Invoke(value);
    }

    /// <summary>
    /// Updates the current position and duration from the media player.
    /// Ignored while the user is dragging the slider.
    /// </summary>
    public void Update(double position, double duration)
    {
        if (_isDragging) return;
        _isUpdatingFromService = true;
        PositionSeconds = position;
        DurationSeconds = duration;
        _isUpdatingFromService = false;
    }

    /// <summary>
    /// Resets position and duration to zero (e.g. on track change).
    /// </summary>
    public void Reset()
    {
        _isUpdatingFromService = true;
        PositionSeconds = 0;
        DurationSeconds = 0;
        _isUpdatingFromService = false;
    }

    [RelayCommand]
    private void DragStarted() => _isDragging = true;

    [RelayCommand]
    private void DragCompleted()
    {
        _isDragging = false;
        SeekRequested?.Invoke(PositionSeconds);
    }

    private static string FormatTime(double totalSeconds)
    {
        if (totalSeconds <= 0) return "0:00";
        var ts = TimeSpan.FromSeconds(totalSeconds);
        return ts.Hours > 0
            ? $"{(int)ts.TotalHours}:{ts.Minutes:D2}:{ts.Seconds:D2}"
            : $"{ts.Minutes}:{ts.Seconds:D2}";
    }
}