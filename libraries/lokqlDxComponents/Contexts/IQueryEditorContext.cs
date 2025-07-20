using CommunityToolkit.Mvvm.Messaging;

namespace lokqlDxComponents.Contexts;

public interface IQueryEditorContext
{
    Guid Id { get; }
    IMessenger Messenger { get; }
    IQueryEngineContext QueryEngineContext { get; }
    IDisposable? BeginLoggingScope();
}
