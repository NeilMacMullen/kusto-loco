using AwesomeAssertions;
using System.Globalization;

namespace BasicTests;

[TestClass]
public class CultureTests : TestMethods
{

    [TestMethod]
    public async Task GreekReal()
    {
        using var _ = new CultureScope("el-GR");
        var query = @"print 1199.1";
        var result = await LastLineOfResult(query);
        result.Should().Contain("1199.1");
    }



    [TestMethod]
    public async Task GreekDecimal()
    {
        using var _ = new CultureScope("el-GR");
        var query = @"datatable(A:decimal) [123.456] | take 1";
        var result = await LastLineOfResult(query);
        result.Should().Contain("123.456");
    }
    [TestMethod]
    public async Task GreekToReal()
    {
        using var _ = new CultureScope("el-GR");
        var query = @"print toreal('1199.1')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("1199.1");
    }

    [TestMethod]
    public async Task GreekToDecimal()
    {
        using var _ = new CultureScope("el-GR");
        var query = @"print todecimal('1199.1')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("1199.1");
    }
    [TestMethod]
    public async Task GreekToDouble()
    {
        using var _ = new CultureScope("el-GR");
        var query = @"print todouble('1199.1')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("1199.1");
    }

    [TestMethod]
    public async Task FrenchReal()
    {
        using var _ = new CultureScope("fr-FR");
        var query = @"datatable(A:real) [123.456] | take 1";
        var result = await LastLineOfResult(query);
        result.Should().Contain("123.456");
    }
}

public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUiCulture;

    public CultureScope(string cultureName)
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUiCulture = CultureInfo.CurrentUICulture;
        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    public void Dispose()
    {
        CultureInfo.CurrentCulture = _originalCulture;
        CultureInfo.CurrentUICulture = _originalUiCulture;
    }
}
