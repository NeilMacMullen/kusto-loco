using Avalonia.Threading;

namespace LokqlDx.Views;

/// <summary>
/// Allows us to dispatch code on the ui thread
/// </summary>
public static class DispatcherHelper
{
    public static async Task<T> SafeInvoke<T>(Func<Task<T>> func) => await Dispatcher.UIThread.Invoke(func);
    public static T SafeInvoke<T>(Func<T> func) =>  Dispatcher.UIThread.Invoke(func);
    public static void SafeInvoke(Action func) => Dispatcher.UIThread.Invoke(func);

}
