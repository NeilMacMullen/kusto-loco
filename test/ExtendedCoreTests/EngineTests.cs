using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;

namespace ExtendedCoreTests;

[TestClass]
public class EngineTests
{
    [TestMethod]
    public void FunctionsAreReportedAsImplemented()
    {
        var engine = new BabyKustoEngine(new NullConsole(), new KustoSettingsProvider());
        var functions = engine.GetImplementedFunctionList();
        //we should have a mix of implemented and not implemented functions
        functions.Any(f => f.Implemented).Should().BeTrue();
        functions.Any(f => !f.Implemented).Should().BeTrue();
    }

    [TestMethod]
    public void SpecificFunctionsAreReportedAsImplemented()
    {
        var engine = new BabyKustoEngine(new NullConsole(), new KustoSettingsProvider());
        var functions = engine.GetImplementedFunctionList();
        var xor =functions.Single(f => f.Name == "binary_xor");
        xor.Implemented.Should().BeTrue();
    }
}
