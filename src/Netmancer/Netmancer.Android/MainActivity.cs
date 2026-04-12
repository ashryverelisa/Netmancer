using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace Netmancer.Android;

[Activity(
    Label = "Netmancer.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}