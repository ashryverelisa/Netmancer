using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Netmancer.Messages;
using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class NowPlayingView : ContentPage
{
    private readonly NowPlayingViewModel _viewModel;
    private IDispatcherTimer? _positionTimer;

    public NowPlayingView(NowPlayingViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        Player.MediaOpened += (_, _) =>
            WeakReferenceMessenger.Default.Send(new MediaOpenedMessage());
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Subscribe to ViewModel → View messages
        var messenger = WeakReferenceMessenger.Default;
        messenger.Register<SetMediaSourceMessage>(this, (_, m) =>
            MainThread.BeginInvokeOnMainThread(() =>
                Player.Source = MediaSource.FromUri(m.Url)));

        messenger.Register<StartPlaybackMessage>(this, (_, _) =>
            MainThread.BeginInvokeOnMainThread(() => Player.Play()));

        messenger.Register<PausePlaybackMessage>(this, (_, _) =>
            MainThread.BeginInvokeOnMainThread(() => Player.Pause()));

        messenger.Register<SeekToPositionMessage>(this, (_, m) =>
            MainThread.BeginInvokeOnMainThread(() =>
                Player.SeekTo(TimeSpan.FromSeconds(m.PositionSeconds))));

        messenger.Register<SetVolumeMessage>(this, (_, m) =>
            MainThread.BeginInvokeOnMainThread(() =>
                Player.Volume = m.Volume));

        // Start a timer to push position/duration into the ViewModel
        _positionTimer = Dispatcher.CreateTimer();
        _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
        _positionTimer.Tick += (_, _) =>
            _viewModel.UpdatePosition(
                Player.Position.TotalSeconds,
                Player.Duration.TotalSeconds);
        _positionTimer.Start();

        // Sync current playback state (e.g. navigating back to an active track)
        _viewModel.Activate();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _positionTimer?.Stop();
        _positionTimer = null;
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
