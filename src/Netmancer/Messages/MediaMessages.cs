using Netmancer.Services;

namespace Netmancer.Messages;

/// <summary>
/// Sent by <see cref="AudioPlayerService"/> when a media command is issued.
/// Received by <see cref="ViewModels.NowPlayingViewModel"/>.
/// </summary>
public sealed record MediaCommandRequestedMessage(MediaCommand Command);

/// <summary>
/// ViewModel → View: call Player.Play().
/// Handled by <see cref="Behaviors.MediaElementPlaybackBehavior"/>.
/// </summary>
public sealed record StartPlaybackMessage;

/// <summary>
/// ViewModel → View: call Player.Pause().
/// Handled by <see cref="Behaviors.MediaElementPlaybackBehavior"/>.
/// </summary>
public sealed record PausePlaybackMessage;

/// <summary>
/// ViewModel → View: seek to the given position.
/// Handled by <see cref="Behaviors.MediaElementPlaybackBehavior"/>.
/// </summary>
public sealed record SeekToPositionMessage(double PositionSeconds);


