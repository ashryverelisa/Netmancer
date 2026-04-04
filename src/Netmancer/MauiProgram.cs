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
        services.AddHttpClient();
        services.AddSingleton<IUpnpContentDirectoryService, UpnpContentDirectoryService>();
        services.AddSingleton<IAudioPlayerService, AudioPlayerService>();

        // ViewModels
        services.AddSingleton<MediaServersViewModel>();
        services.AddTransient<BrowseFoldersViewModel>();
        services.AddSingleton<NowPlayingViewModel>();

        // Views / Shell
        services.AddSingleton<AppShell>();
        services.AddSingleton<MediaServersView>();
        services.AddTransient<BrowseFoldersView>();
        services.AddTransient<NowPlayingView>();
    }
}