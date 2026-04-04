# Netmancer

Netmancer is a cross-platform UPnP/DLNA media browser and audio player built with .NET MAUI. It discovers media servers on the local network, lets you browse their content directories, and plays audio tracks with a full now-playing experience.

## Features

- **UPnP/SSDP device discovery** -- automatically finds media servers on the local network.
- **Content Directory browsing** -- navigates folders and files exposed by UPnP/DLNA servers via SOAP-based ContentDirectory:1 requests.
- **Audio playback** -- streams audio tracks directly from the media server with play, pause, and stop controls.
- **Now Playing screen** -- displays album art, track title, artist, and a seek slider.
- **Cross-platform** -- targets Android, iOS, macOS (Catalyst), and Windows from a single codebase.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- .NET MAUI workload installed (`dotnet workload install maui`)
- Platform-specific prerequisites (Android SDK, Xcode, Windows App SDK, etc.) as required by .NET MAUI

## Key Dependencies

| Package | Purpose |
|---|---|
| [Microsoft.Maui.Controls](https://www.nuget.org/packages/Microsoft.Maui.Controls) | .NET MAUI UI framework |
| [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm) | MVVM source generators and base classes |
| [CommunityToolkit.Maui](https://www.nuget.org/packages/CommunityToolkit.Maui) | MAUI community extensions |
| [CommunityToolkit.Maui.MediaElement](https://www.nuget.org/packages/CommunityToolkit.Maui.MediaElement) | Cross-platform media playback control |
| [Rssdp](https://www.nuget.org/packages/Rssdp) | SSDP device discovery |

## Building and Running

Restore dependencies and build the solution:

```bash
dotnet restore src/Netmancer.sln
dotnet build src/Netmancer.sln
```

Run on a specific target framework:

```bash
# Windows
dotnet run --project src/Netmancer/Netmancer.csproj --framework net10.0-windows10.0.19041.0

# Android (emulator or connected device)
dotnet run --project src/Netmancer/Netmancer.csproj --framework net10.0-android
```

## Running Tests

```bash
dotnet test src/Netmancer.UnitTests/Netmancer.UnitTests.csproj
```

The test project uses xUnit and NSubstitute, and shares service and model source files from the main project via linked compilation.

## Architecture

Netmancer follows the **MVVM** (Model-View-ViewModel) pattern:

1. **Models** define the data structures for discovered devices and media content items.
2. **Services** encapsulate UPnP communication (SSDP discovery, SOAP ContentDirectory browsing) and audio playback state management.
3. **ViewModels** expose observable properties and commands that the views bind to, using `CommunityToolkit.Mvvm` source generators.
4. **Views** are XAML pages that present the UI and bind to their respective view models.

Navigation uses .NET MAUI Shell routing. The app starts at the Media Servers view, navigates to the Browse Folders view when a server is selected, and opens the Now Playing view when an audio track is tapped.
