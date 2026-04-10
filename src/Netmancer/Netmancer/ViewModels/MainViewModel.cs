using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Netmancer.Services;

namespace Netmancer.ViewModels;

/// <summary>
/// Root ViewModel that hosts page navigation and the mini player.
/// Replaces MAUI Shell navigation with a ViewModel-based stack.
/// </summary>
public partial class MainViewModel : ViewModelBase, INavigationService
{
    private readonly Stack<ViewModelBase> _navigationStack = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoBack))]
    public partial ViewModelBase? CurrentPage { get; set; }

    [ObservableProperty]
    public partial MiniPlayerViewModel? MiniPlayer { get; set; }

    public bool CanGoBack => _navigationStack.Count > 0;

    public void NavigateTo(ViewModelBase viewModel)
    {
        if (CurrentPage is not null)
            _navigationStack.Push(CurrentPage);

        CurrentPage = viewModel;
        OnPropertyChanged(nameof(CanGoBack));
    }

    public void GoBack()
    {
        if (_navigationStack.Count == 0) return;
        CurrentPage = _navigationStack.Pop();
        OnPropertyChanged(nameof(CanGoBack));
    }
}