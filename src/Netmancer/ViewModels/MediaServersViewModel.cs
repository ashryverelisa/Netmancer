using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Rssdp;

namespace Netmancer.ViewModels;

public partial class MediaServersViewModel : ObservableObject
{
    public ObservableCollection<string> Devices { get; } = [];

    [RelayCommand]
    private async Task Search()
    {
        Devices.Clear();

        var deviceLocator = new SsdpDeviceLocator();

        deviceLocator.NotificationFilter = "upnp:rootdevice";
        deviceLocator.DeviceAvailable += async (_, e) =>
        {
            var deviceInfo = await e.DiscoveredDevice.GetDeviceInfo();
            var friendlyName = deviceInfo.FriendlyName;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(friendlyName) && !Devices.Contains(friendlyName))
                    Devices.Add(friendlyName);
            });
        };

        await deviceLocator.SearchAsync();
    }
}