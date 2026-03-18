using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
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
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MediaServersViewModel>();
        services.AddTransient<MediaServersView>();
        services.AddTransient<MainPage>();
    }
}