using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Netmancer.Services;
using Netmancer.ViewModels;
using Netmancer.Views;

namespace Netmancer;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        var mainVm = Services.GetRequiredService<MainViewModel>();

        // Set the initial page
        var mediaServersVm = Services.GetRequiredService<MediaServersViewModel>();
        mainVm.NavigateTo(mediaServersVm);

        // Wire up the mini player
        mainVm.MiniPlayer = Services.GetRequiredService<MiniPlayerViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm
            };
        }
        else if (ApplicationLifetime is IActivityApplicationLifetime activityLifetime)
        {
            activityLifetime.MainViewFactory = () => new MainView { DataContext = mainVm };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = mainVm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigureServices(IServiceCollection services)
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

        // Navigation — MainViewModel implements INavigationService
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<MainViewModel>());

        // ViewModels
        services.AddSingleton<MediaServersViewModel>();
        services.AddTransient<BrowseFoldersViewModel>();
        services.AddSingleton<NowPlayingViewModel>();
        services.AddSingleton<MiniPlayerViewModel>();
    }
}