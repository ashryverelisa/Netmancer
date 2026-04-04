using Netmancer.Services;

namespace Netmancer.Messages;

/// <summary>
/// Sent by <see cref="AudioPlayerService"/> when a media command is issued.
/// Received by <see cref="ViewModels.NowPlayingViewModel"/>.
/// </summary>
public sealed record MediaCommandRequestedMessage(MediaCommand Command);

/// <summary>
/// ViewModel → View: set the MediaElement source URL.
/// </summary>
public sealed record SetMediaSourceMessage(string Url);

/// <summary>
/// ViewModel → View: call Player.Play().
/// </summary>
public sealed record StartPlaybackMessage;

/// <summary>
/// ViewModel → View: call Player.Pause().
/// </summary>
public sealed record PausePlaybackMessage;

/// <summary>
/// ViewModel → View: seek to the given position.
/// </summary>
public sealed record SeekToPositionMessage(double PositionSeconds);

/// <summary>
/// ViewModel → View: set the MediaElement volume.
/// </summary>
public sealed record SetVolumeMessage(double Volume);

/// <summary>
/// View → ViewModel: the MediaElement has finished loading and is ready to play.
/// </summary>
public sealed record MediaOpenedMessage;

