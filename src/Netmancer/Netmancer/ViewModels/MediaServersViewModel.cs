using System.Collections.ObjectModel;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Models;
using Netmancer.Services;
using Rssdp;

namespace Netmancer.ViewModels;

public partial class MediaServersViewModel(
    INavigationService navigationService,
    IServiceProvider serviceProvider) : ViewModelBase
{
    public ObservableCollection<MediaDevice> Devices { get; } = [];

    [ObservableProperty]
    public partial bool IsSearching { get; set; }

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial MediaDevice? SelectedDevice { get; set; }

    partial void OnSelectedDeviceChanged(MediaDevice? value)
    {
        if (value is null) return;
        DeviceTapped(value);
        SelectedDevice = null;
    }

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
                ErrorMessage = "No network connection found. Please connect to a network and try again.";
                return;
            }

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
                catch
                {
                    // Device info retrieval failed — skip this device
                }
            }
        }
        finally
        {
            IsSearching = false;
        }
    }

    private void DeviceTapped(MediaDevice device)
    {
        var vm = (BrowseFoldersViewModel)serviceProvider.GetService(typeof(BrowseFoldersViewModel))!;
        vm.Initialize(device.FriendlyName, device.DescriptionLocation.ToString(), "0");
        navigationService.NavigateTo(vm);
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
        catch
        {
            // NetworkInterface enumeration can fail on some platforms
        }

        return addresses;
    }
}

