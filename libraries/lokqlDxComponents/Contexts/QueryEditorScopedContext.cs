using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Contexts;

public sealed class QueryEditorScopedContext(ILogger<QueryEditorScopedContext> logger)
    : IDisposable, IQueryEditorContext
{
    public Guid Id { get; } = Guid.NewGuid();
    public IMessenger Messenger { get; set; } = null!;
    public IQueryEngineContext QueryEngineContext { get; set; } = null!;
    public IServiceScope ServiceScope { get; set; } = null!;

    public IDisposable? BeginLoggingScope() => logger.BeginScope(new Dictionary<string, object> { ["ContextId"] = Id });

    public void Dispose()
    {
        Messenger.Cleanup();
        Messenger.Reset();
        ServiceScope.Dispose();
    }
}
