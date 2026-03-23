using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class MediaServersView : ContentPage
{
    private readonly MediaServersViewModel _mediaServersViewModel;

    public MediaServersView(MediaServersViewModel mediaServersViewModel)
    {
        InitializeComponent();
        _mediaServersViewModel = mediaServersViewModel;
        BindingContext = _mediaServersViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_mediaServersViewModel.Devices.Count == 0 &&
            _mediaServersViewModel.SearchCommand.CanExecute(null))
            await _mediaServersViewModel.SearchCommand.ExecuteAsync(null);
    }
}