using BabyKusto.Core;
using BabyKusto.Core.Util;
using FluentAssertions;
using Kusto.Language.Symbols;
using KustoSupport;
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
        ColumnTypeInferencer.AutoInfer(src).Type.Should().Be(ScalarTypes.Real);
    }

    [TestMethod]
    public void SingleInt()
    {
        var src = Create("5");
        ColumnTypeInferencer.AutoInfer(src).Type.Should().Be(ScalarTypes.Long);
    }

    [TestMethod]
    public void NullThenInt()
    {
        var src = Create(null, "5");
        ColumnTypeInferencer.AutoInfer(src).Type.Should().Be(ScalarTypes.Long);
    }

    [TestMethod]
    public void IntsThenString()
    {
        var src = Create("5", "6", "abc");
        ColumnTypeInferencer.AutoInfer(src).Type.Should().Be(ScalarTypes.String);
    }
}