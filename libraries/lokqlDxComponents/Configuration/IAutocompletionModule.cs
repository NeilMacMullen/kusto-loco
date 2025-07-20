using Intellisense.Configuration;
using Jab;
using lokqlDxComponents.Contexts;
using lokqlDxComponents.Handlers;
using lokqlDxComponents.Services;


namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Scoped<IntellisenseClientAdapter>]
[Scoped<IIntellisenseSettingsProvider, IntellisenseSettingsProvider>]
[Scoped<IIntellisenseKqlOperatorsProvider, IntellisenseKqlOperatorsProvider>]
[Scoped<IIntellisenseKqlFunctionsProvider, IntellisenseKqlFunctionsProvider>]
[Scoped<IIntellisenseCommandsProvider, IntellisenseCommandsProvider>]
[Scoped<IIntellisenseHandler, IntellisensePathHandler>]
[Scoped<IIntellisenseHandler, IntellisenseSettingsHandler>]
[Scoped<IIntellisenseHandler, IntellisenseKqlOperatorsHandler>]
[Scoped<IIntellisenseHandler, IntellisenseKqlFunctionsHandler>]
[Scoped<IIntellisenseHandler, IntellisenseCommandsHandler>]
[Scoped<IIntellisenseHandler, IntellisenseSchemaHandler>]
[Scoped<IQueryEditorContext>(Factory = nameof(GetQueryEditorScope))]
[Scoped<QueryEditorScopedContext>]
[Scoped<SchemaIntellisenseProvider>]
[Singleton<QueryEditorScopedContextFactory>]
[Singleton<IImageProvider>(Factory = nameof(GetAssetFolderImageProvider))]
[Singleton<AssetFolderImageProvider>]
[Singleton<IAssetService, AssetService>]
[Singleton<IInternalAssetLoader, InternalAssetLoader>]
public interface IAutocompletionModule
{
    public static QueryEditorScopedContext GetQueryEditorScope(QueryEditorScopedContext queryEditorScopedContext) => queryEditorScopedContext;
    public static IImageProvider GetAssetFolderImageProvider(AssetFolderImageProvider assetFolderImageProvider) => assetFolderImageProvider;
}
