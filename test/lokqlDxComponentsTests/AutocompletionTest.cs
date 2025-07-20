using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.LogicalTree;
using AwesomeAssertions;
using Lokql.Engine;
using lokqlDxComponentsTests.Fixtures;

namespace lokqlDxComponentsTests;

public class AutocompletionTest
{
    [AvaloniaFact]
    public async Task GeneralCompletionBehavior_ObviousNonmatch_ClosesWindow()
    {
        var f = new AutocompletionTestFixture();


        f.SetInternalCommands("abc", "def");
        f.SendInitEvent();


        await f.Editor.TextArea.Type(".xyz");

        await f.CompletionWindow.ShouldEventuallySatisfy(x => x.IsOpen.Should().BeFalse());
    }

    [AvaloniaTheory]
    [InlineData(".ab", new[] { "abc" })]
    [InlineData(".de", new[] { "def" })]
    public async Task GeneralCompletionBehavior_PartialMatch_RemovesIrrelevantResults(string input, string[] expected)
    {
        var f = new AutocompletionTestFixture();

        f.SetInternalCommands("abc", "def");
        f.SendInitEvent();


        await f.Editor.TextArea.Type(input);

        await f.CompletionWindow.ShouldEventuallySatisfy(c => c
            .GetCurrentCompletionListEntries()
            .Select(x => x.Text)
            .Should()
            .BeEquivalentTo(expected)
        );
    }

    [AvaloniaFact]
    public async Task Commands_PreservesPrefixOnInsertion()
    {
        var f = new AutocompletionTestFixture();

        f.SetInternalCommands("abc");
        f.SendInitEvent();

        await f.Editor.TextArea.Type(".ab");
        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );
        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be(".abc"));
    }


    [AvaloniaFact]
    public async Task KqlOperators_PrependsSpaceOnInsertion()
    {
        var f = new AutocompletionTestFixture();

        f.SetKqlOperators("summarize");
        f.SendInitEvent();

        await f.Editor.TextArea.Type("traffic |sum");

        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );

        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be("traffic | summarize"));
    }

    [AvaloniaFact]
    public async Task KqlFunctions_RemovesPrefixOnInsertion()
    {
        var f = new AutocompletionTestFixture();

        f.SetKqlFunctions("setting1");
        f.SendInitEvent();

        await f.Editor.TextArea.Type("?sett");

        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );

        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be("setting1"));
    }

    [AvaloniaFact]
    public async Task TableCompletion_RemovesPrefixOnInsertion()
    {
        var f = new AutocompletionTestFixture();
        f.SetKqlTables([
                new SchemaLine
                {
                    Table = "abc",
                    Command = "",
                    Column = "ghi"
                }
            ]
        );
        f.SendInitEvent();

        await f.Editor.TextArea.Type("[ab");

        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );

        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be("abc"));
    }

    [AvaloniaFact]
    public async Task ColumnCompletion_RemovesPrefixOnInsertion()
    {
        var f = new AutocompletionTestFixture();
        f.SetKqlTables([
                new SchemaLine
                {
                    Table = "abc",
                    Command = "",
                    Column = "ghi"
                }
            ]
        );
        f.SendInitEvent();

        await f.Editor.TextArea.Type("abc @gh");

        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );

        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be("abc ghi"));
    }

    [AvaloniaTheory]
    [InlineData("table1 @", new[] { "t1_o1" })]
    [InlineData("table2 @", new[] { "t2_o1", "t2_o2" })]
    public async Task ColumnCompletion_ConstrainsColumnOptionsToMatchingTable(string input, string[] expected)
    {
        var f = new AutocompletionTestFixture();
        f.SetKqlTables([
                new SchemaLine
                {
                    Table = "table1",
                    Command = "",
                    Column = "t1_o1"
                },
                new SchemaLine
                {
                    Table = "table2",
                    Command = "",
                    Column = "t2_o1"
                },
                new SchemaLine
                {
                    Table = "table2",
                    Command = "",
                    Column = "t2_o2"
                }
            ]
        );
        f.SendInitEvent();

        await f.Editor.TextArea.Type(input);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be(input));
        await f.CompletionWindow.ShouldEventuallySatisfy(c => c
            .GetCurrentCompletionListEntries()
            .Select(x => x.Text)
            .Should()
            .BeEquivalentTo(expected)
        );
    }

    [AvaloniaFact]
    public async Task Settings_PreservesPrefixOnInsertion()
    {
        var f = new AutocompletionTestFixture();

        f.SetSettings("abc");
        f.SendInitEvent();

        await f.Editor.TextArea.Type("$ab");
        await f.CompletionWindow.ShouldEventuallySatisfy(x =>
            x.IsOpen.Should().BeTrue()
        );
        await f.Editor.TextArea.Press(PhysicalKey.Enter);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be("$abc"));
    }

    [AvaloniaFact]
    public async Task PathCompletion_ExcludesUnsupportedExtensionsButAlwaysIncludesDirectories()
    {
        var f = new AutocompletionTestFixture();
        var data = new List<string>
        {
            "/folder1",
            "/file1.txt",
            "/file2.json",
        };
        f.SetFileSystemData(data);

        f.SetVerbs([
                new()
                {
                    Name = "load",
                    SupportedExtensions = [".json"],
                    SupportsFiles = true
                }
            ]
        );
        f.SendInitEvent();


        await f.Editor.TextArea.Type(".load /");

        await f.CompletionWindow.ShouldEventuallySatisfy(c => c
            .GetCurrentCompletionListEntries()
            .Select(x => x.Text)
            .Should()
            .BeEquivalentTo("folder1", "file2.json")
        );
    }

    [AvaloniaFact]
    public async Task PathCompletion_EraseWithinWord_WindowRemainsOpen()
    {
        var f = new AutocompletionTestFixture();
        var data = new List<string>
        {
            "/folder1/myFolder11"
        };
        f.SetFileSystemData(data);

        f.SetVerbs([
                new()
                {
                    Name = "load",
                    SupportedExtensions = [],
                    SupportsFiles = true
                }
            ]
        );
        f.SendInitEvent();


        var initialText = ".load /folder1";
        var numCharsToErase = "er1".Length;
        var afterErase = ".load /fold";


        await f.Editor.TextArea.Type(initialText);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be(initialText));
        await f.Editor.TextArea.Erase(numCharsToErase);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be(afterErase));
        f.CompletionWindow.IsOpen.Should().BeTrue();
    }

    [AvaloniaFact]
    public async Task PathCompletion_EraseUntilSecondSeparator_RemainsOpen()
    {
        var f = new AutocompletionTestFixture();
        var data = new List<string>
        {
            "/tmp/rag_s1.json"
        };
        f.SetFileSystemData(data);

        f.SetVerbs([
                new()
                {
                    Name = "load",
                    SupportedExtensions = [],
                    SupportsFiles = true
                }
            ]
        );
        f.SendInitEvent();


        var initialText = ".load /tmp/rag_s1.json";
        var numCharsToErase = "ag_s1.json".Length;
        var afterErase = ".load /tmp/r";


        await f.Editor.TextArea.Type(initialText);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be(initialText));
        await f.Editor.TextArea.Erase(numCharsToErase);
        await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be(afterErase));
        f.CompletionWindow.IsOpen.Should().BeTrue();
    }

    [AvaloniaFact]
    public async Task PathCompletion_Defocus_ProducesSameOutputAsUninterrupted()
    {
        var res1 = await SubTest(false);
        var res2 = await SubTest(true);

        res1.Should().BeEquivalentTo(res2);
        return;

        async Task<IEnumerable<string>> SubTest(bool loseFocus)
        {
            var f = new AutocompletionTestFixture();
            var data = new List<string> { "/tmp/rag_s1.json", "/tmp/log_wpf.txt" };
            f.SetFileSystemData(data);

            f.SetVerbs([new() { Name = "load", SupportedExtensions = [], SupportsFiles = true }]);
            f.SendInitEvent();


            await f.Editor.TextArea.Type(".load /tmp/ra");
            await f.Editor.ShouldEventuallySatisfy(x => x.Text.Should().Be(".load /tmp/ra"));
            if (loseFocus)
            {
                f.Editor.TextArea.FindLogicalAncestorOfType<Window>()!.KeyPressQwerty(PhysicalKey.Escape,
                    RawInputModifiers.None
                );
                f.CompletionWindow.IsOpen.Should().BeFalse();
            }

            await f.Editor.TextArea.Type("g_");
            await f.CompletionWindow.ShouldEventuallySatisfy(x => x.IsOpen.Should().BeTrue());
            return f.CompletionWindow.GetCurrentCompletionListEntries().Select(x => x.Text);
        }
    }
}
