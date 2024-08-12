using KustoLoco.Core;
using KustoLoco.Core.Util;
using FluentAssertions;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource.Columns;
using LogSetup;
using NLog;

namespace ExtendedCoreTests;

[TestClass]
public class ColumnInferenceTests
{
    public ColumnInferenceTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    private BaseColumn Create(params object?[] items) =>
        ColumnHelpers.CreateFromObjectArray(items,
            TypeMapping.SymbolForType(typeof(string)));

    [TestMethod]
    public void SingleFloat()
    {
        var src = Create("0.5");
        ColumnTypeInferrer.AutoInfer(src).Type.Should().Be(ScalarTypes.Real);
    }

    [TestMethod]
    public void NumericString()
    {
        //we want to ensure that long strings of digits are not turned into doubles (ala Excel)
        //so we should infer as string
        var src = Create("89457300000022721768");
        ColumnTypeInferrer.AutoInfer(src).Type.Should().Be(ScalarTypes.String);
    }

    [TestMethod]
    public void SingleInt()
    {
        var src = Create("5");
        ColumnTypeInferrer.AutoInfer(src).Type.Should().Be(ScalarTypes.Long);
    }

    [TestMethod]
    public void NullThenInt()
    {
        var src = Create(null, "5");
        ColumnTypeInferrer.AutoInfer(src).Type.Should().Be(ScalarTypes.Long);
    }

    [TestMethod]
    public void IntsThenString()
    {
        var src = Create("5", "6", "abc");
        ColumnTypeInferrer.AutoInfer(src).Type.Should().Be(ScalarTypes.String);
    }
}
