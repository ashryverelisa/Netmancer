using Avalonia.Controls;
using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class MediaServersView : UserControl
{
    public MediaServersView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is MediaServersViewModel vm &&
            vm.Devices.Count == 0 &&
            vm.SearchCommand.CanExecute(null))
        {
            await vm.SearchCommand.ExecuteAsync(null);
        }
    }
}

