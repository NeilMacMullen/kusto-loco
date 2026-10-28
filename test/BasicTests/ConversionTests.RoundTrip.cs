using System.Globalization;
using AwesomeAssertions;

namespace BasicTests;

public partial class ConversionTests
{
    private async Task AssertRoundTrip(string fn, string baseUnit, string[] units, double value)
    {
        var list = string.Join(",", units.Select(u => $"'{u}'"));
        var v = value.ToString("R", CultureInfo.InvariantCulture);
        var query = $"""
                     datatable(u:string)[{list}]
                     | extend r = {fn}({fn}({v}, '{baseUnit}', u), u, '{baseUnit}')
                     | where isnull(r) or abs(r - {v}) > 1e-6 * abs({v})
                     | project u
                     """;
        var result = await Result(query);
        result.Error.Should().BeEmpty();
        var failures = string.Join(",", result.EnumerateRows().Select(RowString));
        failures.Should().BeEmpty($"these units failed to round-trip via {fn}");
    }

    private static readonly string[] AngleUnits =
    [
        "Radian", "Deciradian", "Centiradian", "Milliradian", "Microradian", "Nanoradian", "Degree",
        "Millidegree", "Microdegree", "Nanodegree", "Arcminute", "Arcsecond", "Gradian", "NatoMil", "Revolution"
    ];

    private static readonly string[] EnergyUnits =
    [
        "Joule", "Millijoule", "Kilojoule", "Megajoule", "Gigajoule", "Terajoule", "Petajoule", "Calorie",
        "Kilocalorie", "Megacalorie", "ElectronVolt", "KiloelectronVolt", "MegaelectronVolt", "GigaelectronVolt",
        "TeraelectronVolt", "Erg", "FootPound", "BritishThermalUnit", "KilobritishThermalUnit",
        "MegabritishThermalUnit", "GigabritishThermalUnit", "HorsepowerHour", "WattHour", "KilowattHour",
        "MegawattHour", "GigawattHour", "TerawattHour", "WattDay", "KilowattDay", "MegawattDay", "GigawattDay",
        "TerawattDay", "ThermEc", "DecathermEc", "ThermUs", "DecathermUs", "ThermImperial", "DecathermImperial"
    ];

    private static readonly string[] ForceUnits =
    [
        "Newton", "Micronewton", "Millinewton", "Decanewton", "Kilonewton", "Meganewton", "Dyn", "KilogramForce",
        "KiloPond", "TonneForce", "OunceForce", "PoundForce", "KilopoundForce", "ShortTonForce", "Poundal"
    ];

    private static readonly string[] LengthUnits =
    [
        "Meter", "Nanometer", "Micrometer", "Millimeter", "Centimeter", "Decimeter", "Hectometer", "Kilometer",
        "Megameter", "Microinch", "Mil", "Inch", "Hand", "Foot", "UsSurveyFoot", "Yard", "Fathom", "Chain", "Mile",
        "NauticalMile", "Twip", "PrinterPoint", "PrinterPica", "DtpPoint", "DtpPica", "Shackle", "AstronomicalUnit",
        "LightYear", "Parsec", "Kiloparsec", "Megaparsec", "KilolightYear", "MegalightYear", "SolarRadius"
    ];

    private static readonly string[] MassUnits =
    [
        "Kilogram", "Nanogram", "Microgram", "Milligram", "Centigram", "Decigram", "Gram", "Decagram", "Hectogram",
        "Tonne", "Kilotonne", "Megatonne", "Grain", "Ounce", "Pound", "Kilopound", "Megapound", "Stone", "Slug",
        "ShortHundredweight", "LongHundredweight", "ShortTon", "LongTon", "EarthMass", "SolarMass"
    ];

    private static readonly string[] SpeedUnits =
    [
        "MeterPerSecond", "MeterPerMinute", "MeterPerHour", "NanometerPerSecond", "NanometerPerMinute",
        "MicrometerPerSecond", "MicrometerPerMinute", "MillimeterPerSecond", "MillimeterPerMinute",
        "MillimeterPerHour", "CentimeterPerSecond", "CentimeterPerMinute", "CentimeterPerHour",
        "DecimeterPerSecond", "DecimeterPerMinute", "KilometerPerSecond", "KilometerPerMinute", "KilometerPerHour",
        "InchPerSecond", "InchPerMinute", "InchPerHour", "FootPerSecond", "FootPerMinute", "FootPerHour",
        "UsSurveyFootPerSecond", "UsSurveyFootPerMinute", "UsSurveyFootPerHour", "YardPerSecond", "YardPerMinute",
        "YardPerHour", "MilePerHour", "Knot", "Mach"
    ];

    private static readonly string[] VolumeUnits =
    [
        "CubicMeter", "CubicMillimeter", "CubicCentimeter", "CubicDecimeter", "CubicHectometer", "CubicKilometer",
        "CubicInch", "CubicFoot", "CubicYard", "CubicMile", "Microliter", "Milliliter", "Centiliter", "Deciliter",
        "Liter", "Decaliter", "Hectoliter", "Kiloliter", "Megaliter", "UsGallon", "KilousGallon", "MegausGallon",
        "UsQuart", "UsPint", "UsCustomaryCup", "UsOunce", "UsTablespoon", "UsTeaspoon", "UsBeerBarrel",
        "OilBarrel", "ImperialGallon", "KiloimperialGallon", "MegaimperialGallon", "ImperialQuart", "ImperialPint",
        "ImperialOunce", "ImperialBeerBarrel", "AcreFoot", "MetricCup", "MetricTeaspoon"
    ];

    private static readonly string[] TemperatureUnits =
    [
        "Kelvin", "DegreeCelsius", "MillidegreeCelsius", "DegreeFahrenheit", "DegreeRankine", "DegreeDelisle",
        "DegreeNewton", "DegreeReaumur", "DegreeRoemer", "SolarTemperature"
    ];

    [TestMethod]
    public Task AngleRoundTrip() => AssertRoundTrip("convert_angle", "Radian", AngleUnits, 1.2345);

    [TestMethod]
    public Task EnergyRoundTrip() => AssertRoundTrip("convert_energy", "Joule", EnergyUnits, 123.456);

    [TestMethod]
    public Task ForceRoundTrip() => AssertRoundTrip("convert_force", "Newton", ForceUnits, 123.456);

    [TestMethod]
    public Task LengthRoundTrip() => AssertRoundTrip("convert_length", "Meter", LengthUnits, 123.456);

    [TestMethod]
    public Task MassRoundTrip() => AssertRoundTrip("convert_mass", "Kilogram", MassUnits, 123.456);

    [TestMethod]
    public Task SpeedRoundTrip() => AssertRoundTrip("convert_speed", "MeterPerSecond", SpeedUnits, 123.456);

    [TestMethod]
    public Task VolumeRoundTrip() => AssertRoundTrip("convert_volume", "CubicMeter", VolumeUnits, 123.456);

    [TestMethod]
    public Task TemperatureRoundTrip() => AssertRoundTrip("convert_temperature", "Kelvin", TemperatureUnits, 300);

    [TestMethod]
    public Task LengthRoundTripViaFoot()
        => AssertRoundTrip("convert_length", "Foot", ["Mile", "Inch", "Kilometer"], 5280);

    [TestMethod]
    public Task TemperatureRoundTripViaFahrenheit()
        => AssertRoundTrip("convert_temperature", "DegreeFahrenheit",
            ["DegreeRankine", "DegreeCelsius", "DegreeDelisle"], 98.6);

    [TestMethod]
    public async Task MileToKilometer()
        => (await Eval("convert_length(1, 'Mile', 'Kilometer')")).Should().BeApproximately(1.609344, 1e-9);

    [TestMethod]
    public async Task UsGallonToLiter()
        => (await Eval("convert_volume(1, 'UsGallon', 'Liter')")).Should().BeApproximately(3.785411784, 1e-9);

    [TestMethod]
    public async Task BoilingPointFahrenheitToCelsius()
        => (await Eval("convert_temperature(212, 'DegreeFahrenheit', 'DegreeCelsius')")).Should()
            .BeApproximately(100, 1e-9);

    [TestMethod]
    public async Task RevolutionToDegree()
        => (await Eval("convert_angle(1, 'Revolution', 'Degree')")).Should().BeApproximately(360, 1e-9);

    [TestMethod]
    public async Task MphToKph()
        => (await Eval("convert_speed(1, 'MilePerHour', 'KilometerPerHour')")).Should()
            .BeApproximately(1.609344, 1e-9);

    [TestMethod]
    public async Task KilocalorieToJoule()
        => (await Eval("convert_energy(1, 'Kilocalorie', 'Joule')")).Should().BeApproximately(4184, 1e-9);

    [TestMethod]
    public async Task NegativeValue()
        => (await Eval("convert_temperature(-40, 'DegreeCelsius', 'DegreeFahrenheit')")).Should()
            .BeApproximately(-40, 1e-9);

    [TestMethod]
    public async Task EmptyUnitReturnsNull()
    {
        var result = await LastLineOfResult("print a=isnull(convert_length(1, '', 'Meter'))");
        result.Should().Contain("True");
    }

    [TestMethod]
    public async Task NullUnitReturnsNull()
    {
        var result = await LastLineOfResult("print a=isnull(convert_length(1, 'Meter', tostring(dynamic(null))))");
        result.Should().Contain("True");
    }

    [TestMethod]
    public async Task ColumnConversion()
    {
        var query = """
                    datatable(v:real, src:string, dst:string)
                    [1, 'Meter', 'Centimeter',
                     2, 'Kilogram', 'Gram',
                     3, 'Foot', 'Inch']
                    | extend r = convert_length(v, src, dst)
                    | project r
                    """;
        var result = await Result(query);
        result.Error.Should().BeEmpty();
        var rows = result.EnumerateRows().Select(r => r[0]).ToArray();
        rows.Length.Should().Be(3);
        ((double)rows[0]!).Should().BeApproximately(100, 1e-9);
        rows[1].Should().BeNull();
        ((double)rows[2]!).Should().BeApproximately(36, 1e-9);
    }
}
