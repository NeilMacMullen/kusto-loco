using CommunityToolkit.Mvvm.Messaging;
using Lokql.Engine;
using lokqlDxComponents.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace lokqlDxComponents.Services;

public class QueryEditorScopedContextFactory(IServiceProvider serviceProvider)
{
    public QueryEditorScopedContext Create(InteractiveTableExplorer interactiveTableExplorer)
    {
        var ctx = new QueryEngineContext(interactiveTableExplorer);
        return Create(ctx);
    }

    public QueryEditorScopedContext Create(IQueryEngineContext queryEngineContext)
    {
        var scope = serviceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<QueryEditorScopedContext>();
        ctx.Messenger = new StrongReferenceMessenger();
        ctx.QueryEngineContext = queryEngineContext;
        ctx.ServiceScope = scope;
        scope.ServiceProvider.GetRequiredService<IntellisenseClientAdapter>();
        return ctx;
    }
}
