using Avalonia.Threading;

namespace LokqlDx.Views;

/// <summary>
///     Allows us to dispatch code on the ui thread
/// </summary>
public static class DispatcherHelper
{
    public static async Task<T> SafeInvoke<T>(Func<Task<T>> func) => await Dispatcher.UIThread.InvokeAsync(func);
    
    public static T SafeInvoke<T>(Func<T> func)
    {
        // If already on UI thread, execute directly to avoid deadlock
        if (Dispatcher.UIThread.CheckAccess())
            return func();
        return Dispatcher.UIThread.Invoke(func);
    }
    
    public static void SafeInvoke(Action func)
    {
        // If already on UI thread, execute directly to avoid deadlock
        if (Dispatcher.UIThread.CheckAccess())
            func();
        else
            Dispatcher.UIThread.Invoke(func);
    }
}
