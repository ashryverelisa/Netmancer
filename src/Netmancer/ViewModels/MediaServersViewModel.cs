using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Models;
using Rssdp;

namespace Netmancer.ViewModels;

public partial class MediaServersViewModel : ObservableObject
{
    public ObservableCollection<MediaDevice> Devices { get; } = [];

    [RelayCommand]
    private async Task Search()
    {
        Devices.Clear();

        using var deviceLocator = new AggregateSsdpDeviceLocator(
            includeIpv4: true,
            includeIpv6: false,
            adapterFilter: null,
            logger: null);

        var foundDevices = await deviceLocator.SearchAsync("upnp:rootdevice", TimeSpan.FromSeconds(5));

        foreach (var device in foundDevices)
        {
            try
            {
                var deviceInfo = await device.GetDeviceInfo();
                var friendlyName = deviceInfo.FriendlyName;

                if (!string.IsNullOrEmpty(friendlyName) &&
                    device.DescriptionLocation is not null &&
                    Devices.All(d => d.DescriptionLocation != device.DescriptionLocation))
                {
                    Devices.Add(new MediaDevice
                    {
                        FriendlyName = friendlyName,
                        DescriptionLocation = device.DescriptionLocation
                    });
                }
            }
            catch (Exception)
            {
                // Device info retrieval failed — skip this device
            }
        }
    }

    [RelayCommand]
    private async Task DeviceTapped(MediaDevice device)
    {
        var parameters = new Dictionary<string, object>
        {
            { "deviceName", device.FriendlyName },
            { "descriptionUrl", device.DescriptionLocation.ToString() },
            { "objectId", "0" }
        };

        await Shell.Current.GoToAsync("BrowseFolders", parameters);
    }
}