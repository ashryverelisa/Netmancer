# Netmancer

Netmancer is a cross-platform UPnP/DLNA media browser and audio player built with Avalonia UI. It discovers media servers on the local network, lets you browse their content directories, and plays audio tracks with a full now-playing experience.

## Features

- **UPnP/SSDP device discovery** -- automatically finds media servers on the local network using the SSDP protocol.
- **Content directory browsing** -- navigates folders and files exposed by UPnP/DLNA servers via SOAP-based ContentDirectory:1 requests.
- **Audio playback** -- streams audio tracks directly from the media server using LibVLCSharp, with play/pause, next, previous, and seek controls.
- **Now Playing screen** -- displays album art, track title, artist, a position slider, and playback controls.
- **Mini player** -- a persistent compact player bar shown at the bottom of the screen while browsing; tap it to return to the full Now Playing view.
- **Cross-platform** -- targets Desktop (Windows, macOS, Linux) and Android from a single shared codebase.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- Platform-specific prerequisites:
  - **Desktop**: no additional setup required
  - **Android**: Android SDK (API level 23+)

## Project Structure

```
Netmancer/
  Netmancer/              Shared core library (UI, ViewModels, Services, Models)
    Models/               Data structures (MediaDevice, ContentItem, PlaybackPositionModel)
    Services/             UPnP communication, audio playback, navigation
    ViewModels/           Observable ViewModels with CommunityToolkit.Mvvm
    Views/                Avalonia AXAML views
    Converters/           Value converters for data binding
    Helpers/              Utility classes (e.g. async image loading)
    Assets/               Icons and images
  Netmancer.Desktop/      Desktop head project (Windows, macOS, Linux)
  Netmancer.Android/      Android head project
```

## Key Dependencies

| Package | Purpose |
|---|---|
| [Avalonia](https://avaloniaui.net/) (12.x) | Cross-platform UI framework |
| [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm) | MVVM source generators and base classes |
| [LibVLCSharp](https://www.nuget.org/packages/LibVLCSharp) | Audio playback via libVLC |
| [Rssdp](https://www.nuget.org/packages/Rssdp) | SSDP device discovery |
| [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection) | Dependency injection |
| [Microsoft.Extensions.Http](https://www.nuget.org/packages/Microsoft.Extensions.Http) | HttpClient factory for SOAP requests |

Package versions are managed centrally in `Directory.Packages.props`.

## Building and Running

Restore dependencies and build the solution:

```bash
dotnet restore src/Netmancer.sln
dotnet build src/Netmancer.sln
```

Run the desktop application:

```bash
dotnet run --project src/Netmancer/Netmancer.Desktop/Netmancer.Desktop.csproj
```

Run on Android (emulator or connected device):

```bash
dotnet run --project src/Netmancer/Netmancer.Android/Netmancer.Android.csproj
```

## Architecture

Netmancer follows the MVVM (Model-View-ViewModel) pattern:

1. **Models** define the data structures for discovered devices, media content items, and playback position state.
2. **Services** encapsulate UPnP communication (SSDP discovery via Rssdp, SOAP ContentDirectory browsing via HttpClient) and audio playback management (LibVLCSharp). The `INavigationService` abstraction decouples ViewModels from navigation logic.
3. **ViewModels** expose observable properties and commands that the views bind to. `AudioViewModelBase` provides a shared foundation for any ViewModel that needs to reflect audio playback state. The `CommunityToolkit.Mvvm` source generators handle `INotifyPropertyChanged` and `IRelayCommand` boilerplate.
4. **Views** are Avalonia AXAML UserControls that present the UI and bind to their respective ViewModels. A `ViewLocator` automatically resolves the correct View for each ViewModel type.

Navigation is ViewModel-based. `MainViewModel` implements `INavigationService` and maintains a page stack. ViewModels communicate navigation requests via `WeakReferenceMessenger` messages. The app starts at the Media Servers view, navigates to the Browse Folders view when a server is selected, and opens the Now Playing view when an audio track is tapped.
