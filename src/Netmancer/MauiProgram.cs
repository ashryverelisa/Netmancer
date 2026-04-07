using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Netmancer.Services;
using Netmancer.ViewModels;
using Netmancer.Views;

namespace Netmancer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMediaElement(isAndroidForegroundServiceEnabled: false)
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        AddViewsAndViewModels(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void AddViewsAndViewModels(IServiceCollection services)
    {
        // Services
        services.AddHttpClient<IUpnpContentDirectoryService, UpnpContentDirectoryService>(client =>
        {
            // Many UPnP devices mishandle HTTP/1.1 keep-alive, causing
            // "response ended prematurely" errors.  Close after each request.
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Avoid pooling connections to flaky UPnP devices
            MaxConnectionsPerServer = 4,
        });
        services.AddSingleton<IAudioPlayerService, AudioPlayerService>();

        // ViewModels
        services.AddSingleton<MediaServersViewModel>();
        services.AddTransient<BrowseFoldersViewModel>();
        services.AddSingleton<NowPlayingViewModel>();
        services.AddSingleton<MiniPlayerViewModel>();

        // Views / Shell
        services.AddSingleton<AppShell>();
        services.AddSingleton<MediaServersView>();
        services.AddTransient<BrowseFoldersView>();
        services.AddTransient<NowPlayingView>();
    }
}