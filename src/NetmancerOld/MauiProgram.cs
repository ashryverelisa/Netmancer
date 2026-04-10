using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NetmancerOld.Services;
using NetmancerOld.ViewModels;
using NetmancerOld.Views;

namespace NetmancerOld;

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
            client.DefaultRequestHeaders.ConnectionClose = true;
            client.Timeout = TimeSpan.FromSeconds(15);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            MaxConnectionsPerServer = 4,
            PooledConnectionLifetime = TimeSpan.Zero,
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