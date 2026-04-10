using NetmancerOld.ViewModels;

namespace NetmancerOld.Views;

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

