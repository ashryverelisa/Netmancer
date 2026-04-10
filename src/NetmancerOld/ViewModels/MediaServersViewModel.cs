using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NetmancerOld.Models;
using Rssdp;
#if ANDROID
using Android.Content;
using Android.Net.Wifi;
#endif

namespace NetmancerOld.ViewModels;

public partial class MediaServersViewModel : ObservableObject
{
    public ObservableCollection<MediaDevice> Devices { get; } = [];

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [RelayCommand]
    private async Task Search()
    {
        IsSearching = true;
        ErrorMessage = null;
        Devices.Clear();

        try
        {
            var localAddresses = GetLocalIpv4Addresses();

            if (localAddresses.Count == 0)
            {
                ErrorMessage = "No network connection found. Please connect to a Wi-Fi network and try again.";
                return;
            }

#if ANDROID
            // Android filters out multicast packets by default to save battery.
            // We must acquire a Wi-Fi MulticastLock for SSDP discovery to work.
            var wifiManager = (WifiManager?)Android.App.Application.Context.GetSystemService(Context.WifiService);
            var multicastLock = wifiManager?.CreateMulticastLock("ssdp_discovery");
            multicastLock?.Acquire();
#endif
            try
            {
                using var deviceLocator = new AggregateSsdpDeviceLocator(
                    localIpAddresses: localAddresses,
                    logger: null);

                var foundDevices = await deviceLocator.SearchAsync("upnp:rootdevice", TimeSpan.FromSeconds(5));

                foreach (var device in foundDevices)
                {
                    try
                    {
                        var deviceInfo = await device.GetDeviceInfo();
                        var friendlyName = deviceInfo.FriendlyName;

                        var host = device.DescriptionLocation?.Host ?? string.Empty;
                        var isGateway = host.EndsWith(".1");

                        if (!string.IsNullOrEmpty(friendlyName) &&
                            device.DescriptionLocation is not null &&
                            !isGateway &&
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
            finally
            {
#if ANDROID
                if (multicastLock is { IsHeld: true })
                    multicastLock.Release();
#endif
            }
        }
        finally
        {
            IsSearching = false;
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

    private static List<string> GetLocalIpv4Addresses()
    {
        var addresses = new List<string>();

        try
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (networkInterface.NetworkInterfaceType is
                    NetworkInterfaceType.Loopback or
                    NetworkInterfaceType.Tunnel)
                    continue;

                var properties = networkInterface.GetIPProperties();

                foreach (var unicast in properties.UnicastAddresses)
                {
                    if (unicast.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(unicast.Address))
                    {
                        addresses.Add(unicast.Address.ToString());
                    }
                }
            }
        }
        catch (Exception)
        {
            // NetworkInterface enumeration can fail on some platforms; fall through to empty list.
        }

        return addresses;
    }
}