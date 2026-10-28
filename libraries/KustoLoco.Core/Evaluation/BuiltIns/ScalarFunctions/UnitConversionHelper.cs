using System;
using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <summary>
///     Factor tables and conversion logic for the convert_xxx family of functions.
///     Each table maps a unit name to the number of base units it represents.
///     Unit names are matched case-insensitively and unknown units produce null.
/// </summary>
internal static class UnitConversionHelper
{
    public static double? Convert(IReadOnlyDictionary<string, double> table, double value, string from, string to)
    {
        if (from is null || to is null)
            return null;
        if (!table.TryGetValue(from.Trim(), out var fromFactor) || !table.TryGetValue(to.Trim(), out var toFactor))
            return null;
        return value * fromFactor / toFactor;
    }

    private static Dictionary<string, double> Make(params (string Name, double Factor)[] units)
    {
        var d = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        foreach (var (name, factor) in units)
            d[name] = factor;
        return d;
    }

    private const double Inch = 0.0254;
    private const double Foot = 0.3048;
    private const double Yard = 0.9144;
    private const double Mile = 1609.344;
    private const double UsGallon = 3.785411784e-3;
    private const double ImperialGallon = 4.54609e-3;
    private const double Btu = 1055.05585262;

    // base unit: radian
    public static readonly IReadOnlyDictionary<string, double> Angle = Make(
        ("Radian", 1),
        ("Deciradian", 1e-1),
        ("Centiradian", 1e-2),
        ("Milliradian", 1e-3),
        ("Microradian", 1e-6),
        ("Nanoradian", 1e-9),
        ("Degree", Math.PI / 180),
        ("Millidegree", Math.PI / 180 * 1e-3),
        ("Microdegree", Math.PI / 180 * 1e-6),
        ("Nanodegree", Math.PI / 180 * 1e-9),
        ("Arcminute", Math.PI / 180 / 60),
        ("Arcsecond", Math.PI / 180 / 3600),
        ("Gradian", Math.PI / 200),
        ("NatoMil", 2 * Math.PI / 6400),
        ("Revolution", 2 * Math.PI));

    // base unit: joule
    public static readonly IReadOnlyDictionary<string, double> Energy = Make(
        ("Joule", 1),
        ("Millijoule", 1e-3),
        ("Kilojoule", 1e3),
        ("Megajoule", 1e6),
        ("Gigajoule", 1e9),
        ("Terajoule", 1e12),
        ("Petajoule", 1e15),
        ("Calorie", 4.184),
        ("Kilocalorie", 4184),
        ("Megacalorie", 4.184e6),
        ("ElectronVolt", 1.602176634e-19),
        ("KiloelectronVolt", 1.602176634e-16),
        ("MegaelectronVolt", 1.602176634e-13),
        ("GigaelectronVolt", 1.602176634e-10),
        ("TeraelectronVolt", 1.602176634e-7),
        ("Erg", 1e-7),
        ("FootPound", 1.3558179483314004),
        ("BritishThermalUnit", Btu),
        ("KilobritishThermalUnit", Btu * 1e3),
        ("MegabritishThermalUnit", Btu * 1e6),
        ("GigabritishThermalUnit", Btu * 1e9),
        ("HorsepowerHour", 2684519.537696172792),
        ("WattHour", 3600),
        ("KilowattHour", 3.6e6),
        ("MegawattHour", 3.6e9),
        ("GigawattHour", 3.6e12),
        ("TerawattHour", 3.6e15),
        ("WattDay", 86400),
        ("KilowattDay", 8.64e7),
        ("MegawattDay", 8.64e10),
        ("GigawattDay", 8.64e13),
        ("TerawattDay", 8.64e16),
        ("ThermEc", 1.05505585262e8),
        ("DecathermEc", 1.05505585262e9),
        ("ThermUs", 1.054804e8),
        ("DecathermUs", 1.054804e9),
        ("ThermImperial", 1.05505585257348e8),
        ("DecathermImperial", 1.05505585257348e9));

    // base unit: newton
    public static readonly IReadOnlyDictionary<string, double> Force = Make(
        ("Newton", 1),
        ("Micronewton", 1e-6),
        ("Millinewton", 1e-3),
        ("Decanewton", 10),
        ("Kilonewton", 1e3),
        ("Meganewton", 1e6),
        ("Dyn", 1e-5),
        ("KilogramForce", 9.80665),
        ("KiloPond", 9.80665),
        ("TonneForce", 9806.65),
        ("OunceForce", 0.27801385095378125),
        ("PoundForce", 4.4482216152605),
        ("KilopoundForce", 4448.2216152605),
        ("ShortTonForce", 8896.443230521),
        ("Poundal", 0.138254954376));

    // base unit: meter
    public static readonly IReadOnlyDictionary<string, double> Length = Make(
        ("Meter", 1),
        ("Nanometer", 1e-9),
        ("Micrometer", 1e-6),
        ("Millimeter", 1e-3),
        ("Centimeter", 1e-2),
        ("Decimeter", 1e-1),
        ("Hectometer", 1e2),
        ("Kilometer", 1e3),
        ("Megameter", 1e6),
        ("Microinch", Inch * 1e-6),
        ("Mil", Inch * 1e-3),
        ("Inch", Inch),
        ("Hand", Inch * 4),
        ("Foot", Foot),
        ("UsSurveyFoot", 1200.0 / 3937),
        ("Yard", Yard),
        ("Fathom", 1.8288),
        ("Chain", 20.1168),
        ("Mile", Mile),
        ("NauticalMile", 1852),
        ("Twip", Inch / 1440),
        ("PrinterPoint", Inch / 72.27),
        ("PrinterPica", Inch / 6),
        ("DtpPoint", Inch / 72),
        ("DtpPica", Inch / 6),
        ("Shackle", 27.432),
        ("AstronomicalUnit", 1.495978707e11),
        ("LightYear", 9.46073047258e15),
        ("Parsec", 3.08567758128e16),
        ("Kiloparsec", 3.08567758128e19),
        ("Megaparsec", 3.08567758128e22),
        ("KilolightYear", 9.46073047258e18),
        ("MegalightYear", 9.46073047258e21),
        ("SolarRadius", 6.95510e8));

    // base unit: kilogram
    public static readonly IReadOnlyDictionary<string, double> Mass = Make(
        ("Kilogram", 1),
        ("Nanogram", 1e-12),
        ("Microgram", 1e-9),
        ("Milligram", 1e-6),
        ("Centigram", 1e-5),
        ("Decigram", 1e-4),
        ("Gram", 1e-3),
        ("Decagram", 1e-2),
        ("Hectogram", 1e-1),
        ("Tonne", 1e3),
        ("Kilotonne", 1e6),
        ("Megatonne", 1e9),
        ("Grain", 6.479891e-5),
        ("Ounce", 0.028349523125),
        ("Pound", 0.45359237),
        ("Kilopound", 453.59237),
        ("Megapound", 453592.37),
        ("Stone", 6.35029318),
        ("Slug", 14.593902937206),
        ("ShortHundredweight", 45.359237),
        ("LongHundredweight", 50.80234544),
        ("ShortTon", 907.18474),
        ("LongTon", 1016.0469088),
        ("EarthMass", 5.9722e24),
        ("SolarMass", 1.98947e30));

    // base unit: meter per second
    public static readonly IReadOnlyDictionary<string, double> Speed = Make(
        ("MeterPerSecond", 1),
        ("MeterPerMinute", 1.0 / 60),
        ("MeterPerHour", 1.0 / 3600),
        ("NanometerPerSecond", 1e-9),
        ("NanometerPerMinute", 1e-9 / 60),
        ("MicrometerPerSecond", 1e-6),
        ("MicrometerPerMinute", 1e-6 / 60),
        ("MillimeterPerSecond", 1e-3),
        ("MillimeterPerMinute", 1e-3 / 60),
        ("MillimeterPerHour", 1e-3 / 3600),
        ("CentimeterPerSecond", 1e-2),
        ("CentimeterPerMinute", 1e-2 / 60),
        ("CentimeterPerHour", 1e-2 / 3600),
        ("DecimeterPerSecond", 1e-1),
        ("DecimeterPerMinute", 1e-1 / 60),
        ("KilometerPerSecond", 1e3),
        ("KilometerPerMinute", 1e3 / 60),
        ("KilometerPerHour", 1e3 / 3600),
        ("InchPerSecond", Inch),
        ("InchPerMinute", Inch / 60),
        ("InchPerHour", Inch / 3600),
        ("FootPerSecond", Foot),
        ("FootPerMinute", Foot / 60),
        ("FootPerHour", Foot / 3600),
        ("UsSurveyFootPerSecond", 1200.0 / 3937),
        ("UsSurveyFootPerMinute", 1200.0 / 3937 / 60),
        ("UsSurveyFootPerHour", 1200.0 / 3937 / 3600),
        ("YardPerSecond", Yard),
        ("YardPerMinute", Yard / 60),
        ("YardPerHour", Yard / 3600),
        ("MilePerHour", Mile / 3600),
        ("Knot", 1852.0 / 3600),
        ("Mach", 340.29));

    // base unit: cubic meter
    public static readonly IReadOnlyDictionary<string, double> Volume = Make(
        ("CubicMeter", 1),
        ("CubicMillimeter", 1e-9),
        ("CubicCentimeter", 1e-6),
        ("CubicDecimeter", 1e-3),
        ("CubicHectometer", 1e6),
        ("CubicKilometer", 1e9),
        ("CubicInch", Inch * Inch * Inch),
        ("CubicFoot", Foot * Foot * Foot),
        ("CubicYard", Yard * Yard * Yard),
        ("CubicMile", Mile * Mile * Mile),
        ("Microliter", 1e-9),
        ("Milliliter", 1e-6),
        ("Centiliter", 1e-5),
        ("Deciliter", 1e-4),
        ("Liter", 1e-3),
        ("Decaliter", 1e-2),
        ("Hectoliter", 1e-1),
        ("Kiloliter", 1),
        ("Megaliter", 1e3),
        ("UsGallon", UsGallon),
        ("KilousGallon", UsGallon * 1e3),
        ("MegausGallon", UsGallon * 1e6),
        ("UsQuart", UsGallon / 4),
        ("UsPint", UsGallon / 8),
        ("UsCustomaryCup", UsGallon / 16),
        ("UsOunce", UsGallon / 128),
        ("UsTablespoon", UsGallon / 256),
        ("UsTeaspoon", UsGallon / 768),
        ("UsBeerBarrel", UsGallon * 31),
        ("OilBarrel", UsGallon * 42),
        ("ImperialGallon", ImperialGallon),
        ("KiloimperialGallon", ImperialGallon * 1e3),
        ("MegaimperialGallon", ImperialGallon * 1e6),
        ("ImperialQuart", ImperialGallon / 4),
        ("ImperialPint", ImperialGallon / 8),
        ("ImperialOunce", ImperialGallon / 160),
        ("ImperialBeerBarrel", ImperialGallon * 36),
        ("AcreFoot", 1233.48183754752),
        ("MetricCup", 2.5e-4),
        ("MetricTeaspoon", 5e-6));
}
