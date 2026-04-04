using Netmancer.Views;

namespace Netmancer;

public partial class AppShell : Shell
{
    public AppShell(MediaServersView mediaServersView)
    {
        InitializeComponent();
        MediaServersShellContent.Content = mediaServersView;

        Routing.RegisterRoute("BrowseFolders", typeof(BrowseFoldersView));
        Routing.RegisterRoute("NowPlaying", typeof(NowPlayingView));
    }
}