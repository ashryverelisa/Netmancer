using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Models;
using Netmancer.Services;

namespace Netmancer.ViewModels;

[QueryProperty(nameof(DeviceName), "deviceName")]
[QueryProperty(nameof(DescriptionUrl), "descriptionUrl")]
[QueryProperty(nameof(ObjectId), "objectId")]
public partial class BrowseFoldersViewModel(IUpnpContentDirectoryService contentDirectoryService) : ObservableObject
{
    [ObservableProperty]
    private string _deviceName = string.Empty;

    [ObservableProperty]
    private string _descriptionUrl = string.Empty;

    [ObservableProperty]
    private string _objectId = "0";

    public ObservableCollection<ContentItem> Items { get; } = [];

    [RelayCommand]
    private async Task LoadFolders()
    {
        Items.Clear();

        if (string.IsNullOrEmpty(DescriptionUrl))
            return;

        var uri = new Uri(DescriptionUrl);
        var results = await contentDirectoryService.BrowseAsync(uri, ObjectId);

        foreach (var item in results)
            Items.Add(item);
    }

    [RelayCommand]
    private async Task ItemTapped(ContentItem item)
    {
        if (item.IsContainer)
        {
            var parameters = new Dictionary<string, object>
            {
                { "deviceName", DeviceName },
                { "descriptionUrl", DescriptionUrl },
                { "objectId", item.Id }
            };

            await Shell.Current.GoToAsync("BrowseFolders", parameters);
        }
        else if (!string.IsNullOrEmpty(item.ResourceUrl))
        {
            try
            {
                await Launcher.Default.OpenAsync(new Uri(item.ResourceUrl));
            }
            catch
            {
                // No handler available for this media type
            }
        }
    }
}