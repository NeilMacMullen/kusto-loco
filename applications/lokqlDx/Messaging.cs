using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace LokqlDx;

public static class Messaging
{
    public static T Send<T>(T message) where T : class => WeakReferenceMessenger.Default.Send(message);

    public static void RegisterForValue<T, V>(object owner, Action<V> action) where T : ValueChangedMessage<V>
    {
        void InnerAction(object sender, T message)
        {
            action(message.Value);
        }

        WeakReferenceMessenger.Default.Register<T>(owner, (_, msg) => InnerAction(string.Empty, msg));
    }

    public static void RegisterForEvent<T>(object owner, Action action) where T : class
    {
        void InnerAction(object sender, T message)
        {
            action();
        }

        WeakReferenceMessenger.Default.Register<T>(owner, (_, msg) => InnerAction(string.Empty, msg));
    }
}
