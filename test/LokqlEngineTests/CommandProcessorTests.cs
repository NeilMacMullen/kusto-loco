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

    [TestMethod]
    public void LsFilesCommandIsRegisteredAndSupportsFiles()
    {
        var processor = CommandProcessor.Default();
        var settingsProvider = new KustoSettingsProvider();
        var console = new NullConsole();
        var loader = new StandardFormatAdaptor(settingsProvider, console);

        var result = processor.GetVerbs(loader).ToArray();

        using var _ = new AssertionScope();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("lsfiles", StringComparison.OrdinalIgnoreCase))
            .Which.SupportsFiles.Should()
            .BeTrue();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("lsfiles", StringComparison.OrdinalIgnoreCase))
            .Which.FoldersOnly.Should()
            .BeFalse();
    }

    [TestMethod]
    public async Task LsFilesCommandListsFilesInDirectory()
    {
        var processor = CommandProcessor.Default();
        var settingsProvider = new KustoSettingsProvider();
        var console = new NullConsole();
        var loader = new StandardFormatAdaptor(settingsProvider, console);
        var explorer = new InteractiveTableExplorer(console, settingsProvider, processor,
            new NullResultRenderingSurface(), new Dictionary<Kusto.Language.Symbols.FunctionSymbol, KustoLoco.Core.Evaluation.BuiltIns.ScalarFunctionInfo>());

        // Create a temp directory with some test files
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create some test files
            File.WriteAllText(Path.Combine(tempDir, "test1.csv"), "data");
            File.WriteAllText(Path.Combine(tempDir, "test2.txt"), "data");
            File.WriteAllText(Path.Combine(tempDir, "test3.csv"), "data");

            // Run the command
            await explorer.RunInput($".lsfiles {tempDir}");

            // Check that the _files table was created
            var hasTable = explorer.GetCurrentContext().HasTable("_files");
            hasTable.Should().BeTrue();

            // Verify the table contains the expected files
            var result = await explorer.GetCurrentContext().RunQuery("_files | count");
            result.RowCount.Should().Be(1);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
