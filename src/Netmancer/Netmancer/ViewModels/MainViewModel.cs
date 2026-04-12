using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Netmancer.Messages;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class MainViewModel : ViewModelBase, INavigationService
{
    private readonly Stack<ViewModelBase> _navigationStack = new();
    private readonly IServiceProvider _serviceProvider;

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        WeakReferenceMessenger.Default.Register<NavigateToNowPlayingMessage>(this, (r, _) =>
        {
            var vm = _serviceProvider.GetRequiredService<NowPlayingViewModel>();
            ((MainViewModel)r).NavigateTo(vm);
        });

        WeakReferenceMessenger.Default.Register<NavigateToBrowseFolderMessage>(this, (r, m) =>
        {
            var vm = _serviceProvider.GetRequiredService<BrowseFoldersViewModel>();
            vm.Initialize(m.DeviceName, m.DescriptionUrl, m.ObjectId);
            ((MainViewModel)r).NavigateTo(vm);
        });
    }

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