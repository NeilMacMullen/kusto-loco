using Avalonia.Headless.XUnit;
using AvaloniaEdit;
using AwesomeAssertions;
using Intellisense;
using Intellisense.Configuration;
using Jab;
using LogSetup;
using lokqlDxComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponentsTests;

public class AutocompletionTest
{
    public class Commands
    {
        [AvaloniaFact]
        public void CompletionWindow_NonMatch_ShouldNotBeShown()
        {
            var f = new AutocompletionTestFixture();

            f.ResourceProvider.InternalCommands = [new("abc"), new("def")];


            f.Editor.TextArea.Type(".xyz");
            f.CompletionWindow.IsOpen.Should().BeFalse();
        }

        [AvaloniaTheory]
        [InlineData(".ab", new[] { "abc" })]
        [InlineData(".de", new[] { "def" })]
        public void CurrentList_PartialMatch_ShouldFilterOutIrrelevantResults(string input, string[] expected)
        {
            var f = new AutocompletionTestFixture();

            f.ResourceProvider.InternalCommands = [new("abc"), new("def")];


            f.Editor.TextArea.Type(input);
            var entries = f.CompletionWindow.GetCurrentCompletionListEntries();

            entries.Should().BeEquivalentTo(expected);

        }

    }

}

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
            _intellisenseClient = provider.GetRequiredService<IntellisenseClient>(),
            _logger = provider.GetRequiredService<ILogger<IntellisenseClient>>(),
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
}

[ServiceProvider]
[Import<IIntellisenseModule>]
[Import<ILoggingModule>]
public partial class CompletionManagerTestContainer;
