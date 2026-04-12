using Netmancer.ViewModels;

namespace Netmancer.Services;

/// <summary>
/// ViewModel-based navigation service replacing MAUI Shell navigation.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate to a new page ViewModel, pushing the current one onto the stack.
    /// </summary>
    void NavigateTo(ViewModelBase viewModel);

    /// <summary>
    /// Pop the current page and return to the previous one.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Whether there is a page to go back to.
    /// </summary>
    bool CanGoBack { get; }
}

