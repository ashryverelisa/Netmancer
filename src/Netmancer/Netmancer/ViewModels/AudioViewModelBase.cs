using Netmancer.Services;

namespace Netmancer.ViewModels;

/// <summary>
/// Base class for ViewModels that forward <see cref="IAudioPlayerService"/>
/// property-change notifications to their own properties.
/// Subclasses only need to supply the mapping via <see cref="ServicePropertyMap"/>.
/// </summary>
public abstract class AudioViewModelBase : ViewModelBase
{
    protected IAudioPlayerService AudioService { get; }

    protected AudioViewModelBase(IAudioPlayerService audioService)
    {
        AudioService = audioService;
        audioService.PropertyChanged += OnAudioServicePropertyChanged;
    }

    /// <summary>
    /// Maps an <see cref="IAudioPlayerService"/> property name to the
    /// ViewModel property names that should be re-raised when it changes.
    /// </summary>
    protected abstract IReadOnlyDictionary<string, string[]> ServicePropertyMap { get; }

    /// <summary>
    /// Optional hook called after property-change notifications have been
    /// forwarded. Override to handle extra logic (e.g. NotifyCanExecuteChanged).
    /// </summary>
    protected virtual void OnAfterAudioServicePropertyChanged(string propertyName) { }

    private void OnAudioServicePropertyChanged(object? sender,
        System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null
            || !ServicePropertyMap.TryGetValue(e.PropertyName, out var vmProperties))
            return;

        foreach (var property in vmProperties)
            OnPropertyChanged(property);

        OnAfterAudioServicePropertyChanged(e.PropertyName);
    }
}

