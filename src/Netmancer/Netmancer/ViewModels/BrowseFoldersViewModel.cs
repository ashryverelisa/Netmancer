using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Netmancer.Models;
using Netmancer.Services;

namespace Netmancer.ViewModels;

public partial class BrowseFoldersViewModel(
    IUpnpContentDirectoryService contentDirectoryService,
    IAudioPlayerService audioPlayerService,
    INavigationService navigationService,
    IServiceProvider serviceProvider)
    : ViewModelBase
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
    [NotifyPropertyChangedFor(nameof(HasError))]
    public partial bool IsLoading { get; set; }

    public bool HasError => !IsLoading && ErrorMessage is not null;

    public ObservableCollection<ContentItem> Items { get; } = [];

    [ObservableProperty]
    public partial ContentItem? SelectedItem { get; set; }

    /// <summary>
    /// Replaces MAUI QueryProperty — call after DI construction.
    /// </summary>
    public void Initialize(string deviceName, string descriptionUrl, string objectId)
    {
        DeviceName = deviceName;
        DescriptionUrl = descriptionUrl;
        ObjectId = objectId;
    }

    partial void OnSelectedItemChanged(ContentItem? value)
    {
        if (value is null) return;
        ItemTappedCommand.Execute(value);
        SelectedItem = null;
    }

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
    private void ItemTapped(ContentItem item)
    {
        if (item.IsContainer)
        {
            var vm = (BrowseFoldersViewModel)serviceProvider.GetService(typeof(BrowseFoldersViewModel))!;
            vm.Initialize(DeviceName, DescriptionUrl, item.Id);
            navigationService.NavigateTo(vm);
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

                var nowPlaying = (NowPlayingViewModel)serviceProvider.GetService(typeof(NowPlayingViewModel))!;
                navigationService.NavigateTo(nowPlaying);
            }
            else
            {
                try
                {
                    Process.Start(new ProcessStartInfo(item.ResourceUrl) { UseShellExecute = true });
                }
                catch
                {
                    // No handler available for this media type
                }
            }
        }
    }

    [RelayCommand]
    private void GoBack() => navigationService.GoBack();
}

