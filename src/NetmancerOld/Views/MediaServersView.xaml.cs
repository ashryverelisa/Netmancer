using NetmancerOld.ViewModels;

namespace NetmancerOld.Views;

public partial class MediaServersView : ContentPage
{
    private readonly MediaServersViewModel _mediaServersViewModel;

    public MediaServersView(MediaServersViewModel mediaServersViewModel, MiniPlayerViewModel miniPlayerViewModel)
    {
        InitializeComponent();
        _mediaServersViewModel = mediaServersViewModel;
        BindingContext = _mediaServersViewModel;
        MiniPlayer.BindingContext = miniPlayerViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_mediaServersViewModel.Devices.Count == 0 &&
            _mediaServersViewModel.SearchCommand.CanExecute(null))
            await _mediaServersViewModel.SearchCommand.ExecuteAsync(null);
    }
}