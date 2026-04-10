using Avalonia.Controls;
using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class BrowseFoldersView : UserControl
{
    public BrowseFoldersView()
    {
        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is BrowseFoldersViewModel vm &&
            vm.LoadFoldersCommand.CanExecute(null))
        {
            await vm.LoadFoldersCommand.ExecuteAsync(null);
        }
    }
}

