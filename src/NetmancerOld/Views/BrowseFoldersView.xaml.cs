using NetmancerOld.ViewModels;

namespace NetmancerOld.Views;

public partial class BrowseFoldersView : ContentPage
{
    private readonly BrowseFoldersViewModel _viewModel;

    public BrowseFoldersView(BrowseFoldersViewModel viewModel, MiniPlayerViewModel miniPlayerViewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        MiniPlayer.BindingContext = miniPlayerViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.LoadFoldersCommand.CanExecute(null))
            await _viewModel.LoadFoldersCommand.ExecuteAsync(null);
    }
}
