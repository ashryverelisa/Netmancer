using CommunityToolkit.Maui.Views;
using Netmancer.Services;
using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class NowPlayingView : ContentPage
{
    private readonly IAudioPlayerService _audioService;
    private readonly NowPlayingViewModel _viewModel;
    private IDispatcherTimer? _positionTimer;
    private bool _isDragging;
    private bool _playWhenReady;

    public NowPlayingView(NowPlayingViewModel viewModel, IAudioPlayerService audioPlayerService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        _audioService = audioPlayerService;
        _audioService.MediaCommandRequested += OnMediaCommandRequested;

        Player.MediaOpened += OnMediaOpened;

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(NowPlayingViewModel.Volume))
                Player.Volume = _viewModel.Volume;
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Start a timer to update position/duration from the MediaElement
        _positionTimer = Dispatcher.CreateTimer();
        _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
        _positionTimer.Tick += OnPositionTimerTick;
        _positionTimer.Start();

        // If there's already a source to play, load it
        if (_audioService.SourceUrl is not null && Player.Source is null)
        {
            _playWhenReady = _audioService.IsPlaying;
            Player.Source = MediaSource.FromUri(_audioService.SourceUrl);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _positionTimer?.Stop();
        _positionTimer = null;
    }

    private void OnPositionTimerTick(object? sender, EventArgs e)
    {
        if (_isDragging) return;

        _viewModel.PositionSeconds = Player.Position.TotalSeconds;
        _viewModel.DurationSeconds = Player.Duration.TotalSeconds;
    }

    private void PositionSlider_DragStarted(object? sender, EventArgs e)
    {
        _isDragging = true;
    }

    private void PositionSlider_DragCompleted(object? sender, EventArgs e)
    {
        _isDragging = false;
        Player.SeekTo(TimeSpan.FromSeconds(_viewModel.PositionSeconds));
    }

    private void OnMediaCommandRequested(MediaCommand command)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            switch (command)
            {
                case MediaCommand.Play:
                    if (_audioService.SourceUrl is not null)
                    {
                        _playWhenReady = true;
                        Player.Source = MediaSource.FromUri(_audioService.SourceUrl);
                    }
                    break;
                case MediaCommand.Pause:
                    Player.Pause();
                    break;
                case MediaCommand.Resume:
                    Player.Play();
                    break;

            }
        });
    }

    private void OnMediaOpened(object? sender, EventArgs e)
    {
        if (_playWhenReady)
        {
            _playWhenReady = false;
            MainThread.BeginInvokeOnMainThread(() => Player.Play());
        }
    }
}

