using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public partial class ConversionTests : TestMethods
{
    private async Task<double> Eval(string expr)
    {
        var result = await LastLineOfResult($"print a={expr}");
        return double.Parse(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    [TestMethod]
    public async Task Angle()
        => (await Eval("convert_angle(180, 'Degree', 'Radian')")).Should().BeApproximately(Math.PI, 1e-9);

    [TestMethod]
    public async Task Energy()
        => (await Eval("convert_energy(1, 'KilowattHour', 'Joule')")).Should().BeApproximately(3.6e6, 1e-3);

    [TestMethod]
    public async Task Force()
        => (await Eval("convert_force(1, 'KilogramForce', 'Newton')")).Should().BeApproximately(9.80665, 1e-9);

    [TestMethod]
    public async Task Length()
        => (await Eval("convert_length(1.2, 'Meter', 'Foot')")).Should().BeApproximately(3.93700787, 1e-6);

    [TestMethod]
    public async Task Mass()
        => (await Eval("convert_mass(1, 'Pound', 'Gram')")).Should().BeApproximately(453.59237, 1e-6);

    [TestMethod]
    public async Task Speed()
        => (await Eval("convert_speed(36, 'KilometerPerHour', 'MeterPerSecond')")).Should().BeApproximately(10, 1e-9);

    [TestMethod]
    public async Task Temperature()
        => (await Eval("convert_temperature(1, 'DegreeCelsius', 'DegreeFahrenheit')")).Should().BeApproximately(33.8, 1e-9);

    [TestMethod]
    public async Task TemperatureKelvin()
        => (await Eval("convert_temperature(0, 'Kelvin', 'DegreeCelsius')")).Should().BeApproximately(-273.15, 1e-9);

    [TestMethod]
    public async Task Volume()
        => (await Eval("convert_volume(1, 'Liter', 'CubicCentimeter')")).Should().BeApproximately(1000, 1e-9);

    [TestMethod]
    public async Task CaseInsensitive()
        => (await Eval("convert_length(1, 'kilometer', 'METER')")).Should().BeApproximately(1000, 1e-9);

    [TestMethod]
    public async Task UnknownUnitReturnsNull()
    {
        var result = await LastLineOfResult("print a=isnull(convert_length(1, 'Meter', 'Smoot'))");
        result.Should().Contain("True");
    }
}
