using AwesomeAssertions;
using AwesomeAssertions.Execution;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using Lokql.Engine.Commands;

namespace LokqlEngineTests;

[TestClass]
public class CommandProcessorTests
{
    [TestMethod]
    public void GetVerbsIdentifiesVerbsOfOptionsAcceptingFiles()
    {
        var processor = CommandProcessor.Default();

        var settingsProvider = new KustoSettingsProvider();
        var console = new NullConsole();
        var loader = new StandardFormatAdaptor(settingsProvider, console);


        var result = processor.GetVerbs(loader).ToArray();

        using var _ = new AssertionScope();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("load", StringComparison.OrdinalIgnoreCase))
            .Which.SupportsFiles.Should()
            .BeTrue();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("synonym", StringComparison.OrdinalIgnoreCase))
            .Which.SupportsFiles.Should()
            .BeFalse();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("cd", StringComparison.OrdinalIgnoreCase))
            .Which.SupportsFiles.Should()
            .BeTrue();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("cd", StringComparison.OrdinalIgnoreCase))
            .Which.FoldersOnly.Should()
            .BeTrue();
    }

    [TestMethod]
    public async Task CdCommandSetsKustoDataPath()
    {
        var processor = CommandProcessor.Default();
        var settingsProvider = new KustoSettingsProvider();
        var console = new NullConsole();
        var loader = new StandardFormatAdaptor(settingsProvider, console);
        var explorer = new InteractiveTableExplorer(console, settingsProvider, processor,
            new NullResultRenderingSurface(), new Dictionary<Kusto.Language.Symbols.FunctionSymbol, KustoLoco.Core.Evaluation.BuiltIns.ScalarFunctionInfo>());

        await explorer.RunInput(".cd C:\\mydata");

        var dataPath = settingsProvider.Get(StandardFormatAdaptor.Settings.KustoDataPath);
        dataPath.Should().Be("C:\\mydata");
    }
}
