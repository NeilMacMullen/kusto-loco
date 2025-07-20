using System.IO.Abstractions.TestingHelpers;
using Avalonia.Platform;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Messaging;
using Intellisense;
using Intellisense.FileSystem;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using lokqlDxComponents;
using lokqlDxComponents.Contexts;
using lokqlDxComponents.Events;
using lokqlDxComponents.Services;
using lokqlDxComponents.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace lokqlDxComponentsTests.Fixtures;

public class AutocompletionTestFixture
{
    public AutocompletionTestFixture()
    {
        var provider = new CompletionManagerTestContainer();
        MockAssetLoader = new Mock<IAssetService>();
        provider.GetAssetLoader = _ => MockAssetLoader.Object;
        var window = new TestTextEditorWindow();
        window.Show();
        // we need to obtain the editor from a view because editor internals assume it is attached to the logical tree (null exceptions)
        var editor = window.TextEditor;
        var editorHelper = new EditorHelper(editor);
        var queryEngineContext = new Mock<IQueryEngineContext>();
        var scope = provider
            .GetRequiredService<QueryEditorScopedContextFactory>()
            .Create(queryEngineContext.Object);


        Messenger = scope.Messenger;



        var windowWrapper = new CompletionWindowWrapper(editor.TextArea);
        var completionManager = new CompletionManager(editor, editorHelper, () => scope.Messenger, windowWrapper);
        editor.TextArea.TextEntered += async (_, args) => await completionManager.HandleKeyDown(args);
        Editor = editor;
        EditorHelper = editorHelper;
        CompletionManager = completionManager;
        Provider = provider;
        MockQueryEngineContext = queryEngineContext;
        CompletionWindow = windowWrapper;
    }

    public Mock<IAssetService> MockAssetLoader { get; set; }

    public Mock<IQueryEngineContext> MockQueryEngineContext { get; set; }

    public IMessenger Messenger { get; set; }

    public CompletionWindowWrapper CompletionWindow { get; set; }

    public CompletionManagerTestContainer Provider { get; set; }

    public CompletionManager CompletionManager { get; set; }

    public EditorHelper EditorHelper { get; set; }

    public TextEditor Editor { get; set; }

    public void SetInternalCommands(params string[] commands)
    {
        var verbs = commands.Select(x => new VerbEntry { Name = x, SupportsFiles = true, SupportedExtensions = []}).ToArray();
        MockQueryEngineContext.Setup(x => x.GetVerbs()).Returns(verbs);
    }

    public void SetVerbs(IEnumerable<VerbEntry> verbs)
    {
        MockQueryEngineContext.Setup(x => x.GetVerbs()).Returns(verbs);
    }

    public void SendInitEvent() => Messenger.Send<InitMessage>();

    public void SetKqlOperators(params string[] operators)
    {
        var entries = operators.Select(x => new IntellisenseEntry { Name = x }).ToArray();
        MockAssetLoader.Setup(x => x.Deserialize<IntellisenseEntry[]>(AssetLocations.IntellisenseOperators)).Returns(entries);
    }

    public void SetKqlFunctions(params string[] functions)
    {
        var entries = functions.Select(x => new IntellisenseEntry { Name = x }).ToArray();
        MockAssetLoader.Setup(x => x.Deserialize<IntellisenseEntry[]>(AssetLocations.IntellisenseFunctions)).Returns(entries);
    }

    public void SetKqlTables(IEnumerable<SchemaLine> tables) => MockQueryEngineContext.Setup(x => x.GetSchema()).Returns(tables.ToArray());

    public void SetFileSystemData(IEnumerable<string> fileSystemData)
    {
        var data = fileSystemData.ToDictionary(x => x, _ => new MockFileData(""));

        Provider.GetFileSystemReader = _ =>
            new FileSystemReader(new MockFileSystem(data, new MockFileSystemOptions() { CreateDefaultTempDir = false })
            );
    }

    public void SetSettings(params string[] settings)
    {
        var data = settings.Select(x => new RawKustoSetting(x, x + "_value"));
        MockQueryEngineContext.Setup(x => x.GetSettings()).Returns(data);
    }
}
