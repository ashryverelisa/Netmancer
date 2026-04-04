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
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Start a timer to push position/duration into the ViewModel
        _positionTimer = Dispatcher.CreateTimer();
        _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
        _positionTimer.Tick += (_, _) =>
            _viewModel.Position.Update(
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
        _viewModel.Deactivate();
    }
}
