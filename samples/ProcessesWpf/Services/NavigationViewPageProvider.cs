using Wpf.Ui.Abstractions;

namespace ProcessesWpf.Services;
/// <summary>
/// Service that provides pages for navigation.
/// </summary>
/// <remarks>
/// Creates new instance and attaches the <see cref="IServiceProvider"/>.
/// </remarks>
public class NavigationViewPageProvider(IServiceProvider serviceProvider) : INavigationViewPageProvider
{
    /// <summary>
    /// Service which provides the instances of pages.
    /// </summary>
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public T? GetPage<T>() where T : class
        => !typeof(FrameworkElement).IsAssignableFrom(typeof(T))
            ? throw new InvalidOperationException("The page should be a WPF control.")
            : (T?)_serviceProvider.GetService(typeof(T));

    /// <inheritdoc />
    public FrameworkElement? GetPage(Type pageType)
        => !typeof(FrameworkElement).IsAssignableFrom(pageType)
            ? throw new InvalidOperationException("The page should be a WPF control.")
            : _serviceProvider.GetService(pageType) as FrameworkElement;

    object? INavigationViewPageProvider.GetPage(Type pageType) => GetPage(pageType);
}
