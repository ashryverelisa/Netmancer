using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class BrowseFoldersView : ContentPage
{
    private readonly BrowseFoldersViewModel _viewModel;

    public BrowseFoldersView(BrowseFoldersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.LoadFoldersCommand.CanExecute(null))
            await _viewModel.LoadFoldersCommand.ExecuteAsync(null);
    }
}

