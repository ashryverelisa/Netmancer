using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class MiniPlayerView : ContentView
{
    public MiniPlayerView()
    {
        InitializeComponent();
    }

    public MiniPlayerView(MiniPlayerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

