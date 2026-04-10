using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using NetmancerOld.Messages;

namespace NetmancerOld.Behaviors;

/// <summary>
/// Bridges imperative playback commands from the ViewModel to the
/// <see cref="MediaElement"/>. Attach this behavior so the element responds to
/// <see cref="StartPlaybackMessage"/>, <see cref="PausePlaybackMessage"/>,
/// and <see cref="SeekToPositionMessage"/> without any code-behind.
/// </summary>
public class MediaElementPlaybackBehavior : Behavior<MediaElement>,
    IRecipient<StartPlaybackMessage>,
    IRecipient<PausePlaybackMessage>,
    IRecipient<SeekToPositionMessage>
{
    private MediaElement? _mediaElement;

    protected override void OnAttachedTo(MediaElement bindable)
    {
        base.OnAttachedTo(bindable);
        _mediaElement = bindable;
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    protected override void OnDetachingFrom(MediaElement bindable)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        _mediaElement = null;
        base.OnDetachingFrom(bindable);
    }

    public void Receive(StartPlaybackMessage message) =>
        MainThread.BeginInvokeOnMainThread(() => _mediaElement?.Play());

    public void Receive(PausePlaybackMessage message) =>
        MainThread.BeginInvokeOnMainThread(() => _mediaElement?.Pause());

    public void Receive(SeekToPositionMessage message) =>
        MainThread.BeginInvokeOnMainThread(() =>
            _mediaElement?.SeekTo(TimeSpan.FromSeconds(message.PositionSeconds)));
}

