using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Models;
using Netmancer.Services;

namespace Netmancer.ViewModels;

[QueryProperty(nameof(DeviceName), "deviceName")]
[QueryProperty(nameof(DescriptionUrl), "descriptionUrl")]
[QueryProperty(nameof(ObjectId), "objectId")]
public partial class BrowseFoldersViewModel(
    IUpnpContentDirectoryService contentDirectoryService,
    IAudioPlayerService audioPlayerService) : ObservableObject
{
    [ObservableProperty]
    public partial string DeviceName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DescriptionUrl { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ObjectId { get; set; } = "0";

    [ObservableProperty]
    public partial string? ErrorMessage { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    public ObservableCollection<ContentItem> Items { get; } = [];

    [RelayCommand]
    private async Task LoadFolders()
    {
        Items.Clear();
        ErrorMessage = null;
        IsLoading = true;

        if (string.IsNullOrEmpty(DescriptionUrl))
            return;

        try
        {
            var uri = new Uri(DescriptionUrl);
            var results = await contentDirectoryService.BrowseAsync(uri, ObjectId);

            foreach (var item in results)
                Items.Add(item);

            if (results.Count == 0)
                ErrorMessage = "No items found.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = $"Could not reach the media server. Check that it's still online.\n({ex.InnerException?.Message ?? ex.Message})";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Something went wrong: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
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
            if (item.MediaClass.Contains("audio", StringComparison.OrdinalIgnoreCase))
            {
                var audioItems = Items
                    .Where(i => !i.IsContainer &&
                                i.MediaClass.Contains("audio", StringComparison.OrdinalIgnoreCase) &&
                                !string.IsNullOrEmpty(i.ResourceUrl))
                    .ToList();
                audioPlayerService.Play(item, audioItems);
                await Shell.Current.GoToAsync("NowPlaying");
            }
            else
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
}