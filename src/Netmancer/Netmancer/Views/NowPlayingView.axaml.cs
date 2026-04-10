using Avalonia.Controls;
using Netmancer.ViewModels;

namespace Netmancer.Views;

public partial class NowPlayingView : UserControl
{
    public NowPlayingView()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataContext is NowPlayingViewModel vm)
            vm.Activate();
    }

    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        if (DataContext is NowPlayingViewModel vm)
            vm.Deactivate();
        base.OnDetachedFromVisualTree(e);
    }
}

