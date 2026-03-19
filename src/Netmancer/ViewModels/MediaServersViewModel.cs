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

        using var deviceLocator = new SsdpDeviceLocator();
        var foundDevices = await deviceLocator.SearchAsync("upnp:rootdevice", TimeSpan.FromSeconds(5));

        foreach (var device in foundDevices)
        {
            try
            {
                var deviceInfo = await device.GetDeviceInfo();
                var friendlyName = deviceInfo.FriendlyName;

                if (!string.IsNullOrEmpty(friendlyName) && !Devices.Contains(friendlyName))
                    Devices.Add(friendlyName);
            }
            catch
            {
                // Device didn't respond to description request; skip it
            }
        }
    }
}