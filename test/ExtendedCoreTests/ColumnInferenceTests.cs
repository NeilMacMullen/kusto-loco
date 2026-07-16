using AwesomeAssertions;
using Kusto.Language.Symbols;
using KustoLoco.Core;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;
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
        src.GetRawDataValue(0)!.ToString().Should().Be("89457300000022721768");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.String);
        inferredColumn.GetRawDataValue(0)!.ToString().Should().Be("89457300000022721768");
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

    [TestMethod]
    public void DoubleIsAllowedToTruncate()
    {
        var src = Create("12.34567890123456789012");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.Real);
        inferredColumn.GetRawDataValue(0)!.ToString().Should().Be("12.345678901234567");
    }


    [TestMethod]
    public void DateTimeResultsInUTC()
    {
        //
        var src = Create("2026-07-15 10:07:48");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);
        var dt = (DateTime) inferredColumn.GetRawDataValue(0)!;
        dt.Kind.Should().Be(DateTimeKind.Utc);
        dt.ToString("O").Should().Contain("10:07:48");
    }

    [TestMethod]
    public void DateTimeWithoutTimezone_AssumesUTC()
    {
        // When a DateTime string has no timezone, it should be assumed to be UTC
        var src = Create("2024-03-15 14:30:00");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);

        var dt = (DateTime) inferredColumn.GetRawDataValue(0)!;
        dt.Kind.Should().Be(DateTimeKind.Utc);
        dt.Year.Should().Be(2024);
        dt.Month.Should().Be(3);
        dt.Day.Should().Be(15);
        dt.Hour.Should().Be(14);
        dt.Minute.Should().Be(30);
        dt.Second.Should().Be(0);
    }

    [TestMethod]
    public void DateTimeWithExplicitUTC_ResultsInUTC()
    {
        // DateTime strings with explicit UTC marker (Z) should result in UTC
        var src = Create("2024-03-15T14:30:00Z");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);

        var dt = (DateTime) inferredColumn.GetRawDataValue(0)!;
        dt.Kind.Should().Be(DateTimeKind.Utc);
        dt.Year.Should().Be(2024);
        dt.Month.Should().Be(3);
        dt.Day.Should().Be(15);
        dt.Hour.Should().Be(14);
        dt.Minute.Should().Be(30);
        dt.Second.Should().Be(0);
    }

    [TestMethod]
    public void DateTimeWithPositiveOffset_ConvertedToUTC()
    {
        // DateTime with +05:00 offset should be converted to UTC (subtract 5 hours)
        var src = Create("2024-03-15T14:30:00+05:00");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);

        var dt = (DateTime) inferredColumn.GetRawDataValue(0)!;
        dt.Kind.Should().Be(DateTimeKind.Utc);
        // 14:30 - 05:00 = 09:30 UTC
        dt.Year.Should().Be(2024);
        dt.Month.Should().Be(3);
        dt.Day.Should().Be(15);
        dt.Hour.Should().Be(9);
        dt.Minute.Should().Be(30);
    }

    [TestMethod]
    public void DateTimeWithNegativeOffset_ConvertedToUTC()
    {
        // DateTime with -08:00 offset should be converted to UTC (add 8 hours)
        var src = Create("2024-03-15T14:30:00-08:00");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);

        var dt = (DateTime) inferredColumn.GetRawDataValue(0)!;
        dt.Kind.Should().Be(DateTimeKind.Utc);
        // 14:30 + 08:00 = 22:30 UTC
        dt.Year.Should().Be(2024);
        dt.Month.Should().Be(3);
        dt.Day.Should().Be(15);
        dt.Hour.Should().Be(22);
        dt.Minute.Should().Be(30);
    }

    [TestMethod]
    public void MultipleDateTimesWithoutTimezone_AllAssumeUTC()
    {
        // Multiple DateTime values without timezone should all be assumed UTC
        var src = Create("2024-01-01 00:00:00", "2024-06-15 12:00:00", "2024-12-31 23:59:59");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);

        var dt1 = (DateTime) inferredColumn.GetRawDataValue(0)!;
        dt1.Kind.Should().Be(DateTimeKind.Utc);
        dt1.Year.Should().Be(2024);
        dt1.Month.Should().Be(1);
        dt1.Hour.Should().Be(0);

        var dt2 = (DateTime) inferredColumn.GetRawDataValue(1)!;
        dt2.Kind.Should().Be(DateTimeKind.Utc);
        dt2.Month.Should().Be(6);
        dt2.Hour.Should().Be(12);

        var dt3 = (DateTime) inferredColumn.GetRawDataValue(2)!;
        dt3.Kind.Should().Be(DateTimeKind.Utc);
        dt3.Month.Should().Be(12);
        dt3.Hour.Should().Be(23);
        dt3.Minute.Should().Be(59);
    }

    [TestMethod]
    public void DateTimeWithMixedTimezones_AllConvertedToUTC()
    {
        // DateTime values with different timezones should all be converted to UTC
        var src = Create(
            "2024-03-15T10:00:00Z",           // UTC
            "2024-03-15T10:00:00+02:00",      // 08:00 UTC
            "2024-03-15T10:00:00-05:00"       // 15:00 UTC
        );
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);

        var dt1 = (DateTime) inferredColumn.GetRawDataValue(0)!;
        dt1.Kind.Should().Be(DateTimeKind.Utc);
        dt1.Hour.Should().Be(10);

        var dt2 = (DateTime) inferredColumn.GetRawDataValue(1)!;
        dt2.Kind.Should().Be(DateTimeKind.Utc);
        dt2.Hour.Should().Be(8);

        var dt3 = (DateTime) inferredColumn.GetRawDataValue(2)!;
        dt3.Kind.Should().Be(DateTimeKind.Utc);
        dt3.Hour.Should().Be(15);
    }

    [TestMethod]
    public void DateTimeWithNullValues_RemainingAssumeUTC()
    {
        // Null/empty values should not affect UTC assumption for non-null DateTime values
        var src = Create(null, "2024-03-15 14:30:00", "", "2024-03-16 16:45:00");
        var inferredColumn = ColumnTypeInferrer.AutoInfer(src);
        inferredColumn.Type.Should().Be(ScalarTypes.DateTime);

        inferredColumn.GetRawDataValue(0).Should().BeNull();

        var dt1 = (DateTime) inferredColumn.GetRawDataValue(1)!;
        dt1.Kind.Should().Be(DateTimeKind.Utc);
        dt1.Hour.Should().Be(14);

        inferredColumn.GetRawDataValue(2).Should().BeNull();

        var dt2 = (DateTime) inferredColumn.GetRawDataValue(3)!;
        dt2.Kind.Should().Be(DateTimeKind.Utc);
        dt2.Hour.Should().Be(16);
    }
}
