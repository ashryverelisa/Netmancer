using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class MediaServersView : ContentPage
{
    private readonly MediaServersViewModel _mediaServersViewModel;
    
    public MediaServersView(MediaServersViewModel mediaServersViewModel)
    {
        _mediaServersViewModel = mediaServersViewModel;
        InitializeComponent();
        BindingContext = _mediaServersViewModel;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_mediaServersViewModel.SearchCommand.CanExecute(null))
            await _mediaServersViewModel.SearchCommand.ExecuteAsync(null);
    }
}