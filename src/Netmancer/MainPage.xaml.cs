using Netmancer.ViewModels;

namespace Netmancer;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;
    }
}