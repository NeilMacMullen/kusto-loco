using System.IO.Abstractions.TestingHelpers;
using AvaloniaEdit;
using Intellisense.FileSystem;
using lokqlDxComponents;
using lokqlDxComponents.Services;
using lokqlDxComponents.Views.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace lokqlDxComponentsTests.Fixtures;

public class AutocompletionTestFixture
{
    public AutocompletionTestFixture()
    {
        var provider = new CompletionManagerTestContainer();
        var window = new TestTextEditorWindow();
        window.Show();
        // we need to obtain the editor from a view because editor internals assume it is attached to the logical tree (null exceptions)
        var editor = window.TextEditor;
        var editorHelper = new EditorHelper(editor);
        var resourceProvider = new MockResourceProvider
        {
            _intellisenseClient = provider.GetRequiredService<IntellisenseClientAdapter>()
        };
        var windowWrapper = new CompletionWindowWrapper(editor.TextArea);
        var completionManager = new CompletionManager(editor, editorHelper, resourceProvider, windowWrapper);
        editor.TextArea.TextEntered += async (_, args) => await completionManager.HandleKeyDown(args);
        Editor = editor;
        EditorHelper = editorHelper;
        CompletionManager = completionManager;
        Provider = provider;
        ResourceProvider = resourceProvider;
        CompletionWindow = windowWrapper;
    }

    public CompletionWindowWrapper CompletionWindow { get; set; }

    public CompletionManagerTestContainer Provider { get; set; }

    public CompletionManager CompletionManager { get; set; }

    public EditorHelper EditorHelper { get; set; }

    public TextEditor Editor { get; set; }

    public MockResourceProvider ResourceProvider { get; set; }

    public void SetFileSystemData(IEnumerable<string> fileSystemData)
    {
        var data = fileSystemData.ToDictionary(x => x, _ => new MockFileData(""));

        Provider.GetFileSystemReader = _ => new FileSystemReader(new MockFileSystem(data, new MockFileSystemOptions(){CreateDefaultTempDir = false}));
    }
}
