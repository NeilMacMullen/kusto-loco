using AwesomeAssertions;
using NotNullStrings;

namespace BasicTests;

[TestClass]
public class AdditionalFunctionTests : TestMethods
{
    [TestMethod]
    public async Task SqrtFunction_Scalar()
    {
        var query = "print result = sqrt(16)";
        var result = await LastLineOfResult(query);
        result.Should().Be("4");
    }

    [TestMethod]
    public async Task PowFunction_Scalar()
    {
        var query = "print result = pow(2, 8)";
        var result = await LastLineOfResult(query);
        result.Should().Be("256");
    }

    [TestMethod]
    public async Task ExpFunction_Scalar()
    {
        var query = "print result = exp(1)";
        var result = await LastLineOfResult(query);
        double.Parse(result).Should().BeApproximately(2.718, 0.01);
    }

    [TestMethod]
    public async Task Log10Function_Scalar()
    {
        var query = "print result = log10(1000)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task ToBoolFunction_Scalar()
    {
        var query = "print result = tobool('true')";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task IsNanFunction_Scalar()
    {
        var query = "print result = isnan(0.0/0.0)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task IsInfFunction_Scalar()
    {
        var query = "print result = isinf(1.0/0.0)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }
    

    [TestMethod]
    public async Task ToLowerFunction_Scalar()
    {
        var query = "print result = tolower('HELLO')";
        var result = await LastLineOfResult(query);
        result.Should().Be("hello");
    }

    [TestMethod]
    public async Task ToUpperFunction_Scalar()
    {
        var query = "print result = toupper('hello')";
        var result = await LastLineOfResult(query);
        result.Should().Be("HELLO");
    }

    [TestMethod]
    public async Task StrLenFunction_Scalar()
    {
        var query = "print result = strlen('hello')";
        var result = await LastLineOfResult(query);
        result.Should().Be("5");
    }


    [TestMethod]
    public async Task Bin_ZeroInterval_ShouldReturnError()
    {
        var query = "print c=bin(10, 0)";
        var result = await LastLineOfResult(query);
        result.Should().Be("<null>");
    }

    [TestMethod]
    public async Task ReplaceStringFunction_Scalar()
    {
        var query = "print result = replace_string('hello world', 'world', 'Kusto')";
        var result = await LastLineOfResult(query);
        result.Should().Be("hello Kusto");
    }

    [TestMethod]
    public async Task SplitFunction_Scalar()
    {
        var query = "print result = split('a,b,c', ',')";
        var result = await LastLineOfResult(query);
        result
            .Tokenize("\r\n ")
            .JoinString("")
            .Should().Be("""["a","b","c"]""");
    }

    [TestMethod]
    public async Task StrcatFunction_Scalar()
    {
        var query = "print result = strcat('a', 'b', 'c')";
        var result = await LastLineOfResult(query);
        result.Should().Be("abc");
    }

    [TestMethod]
    public async Task StrcatDelimFunction_Scalar()
    {
        var query = "print result = strcat_delim(',', 'a', 'b', 'c')";
        var result = await LastLineOfResult(query);
        result.Should().Be("a,b,c");
    }

    [TestMethod]
    public async Task ReverseFunction_Scalar()
    {
        var query = "print result = reverse('abc')";
        var result = await LastLineOfResult(query);
        result.Should().Be("cba");
    }

    [TestMethod]
    public async Task IsEmptyFunction_Scalar()
    {
        var query = "print result = isempty('')";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task IsNotEmptyFunction_Scalar()
    {
        var query = "print result = isnotempty('abc')";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task IndexOfFunction_Scalar()
    {
        var query = "print result = indexof('hello world', 'world')";
        var result = await LastLineOfResult(query);
        result.Should().Be("6");
    }

    [TestMethod]
    public async Task AbsFunction_Scalar()
    {
        var query = "print result = abs(-42)";
        var result = await LastLineOfResult(query);
        result.Should().Be("42");
    }

    [TestMethod]
    public async Task RoundFunction_Scalar()
    {
        var query = "print result = round(3.14159, 2)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.14");
    }

    [TestMethod]
    public async Task LogFunction_Scalar()
    {
        var query = "print result = log(100)";
        var result = await LastLineOfResult(query);
        double.Parse(result).Should().BeApproximately(4.605, 0.01);
    }

    [TestMethod]
    public async Task NowFunction_Scalar()
    {
        var query = "print result = now()";
        var result = await LastLineOfResult(query);
        result.Should().Contain("20"); // year prefix
    }

    [TestMethod]
    public async Task StartOfDayFunction_Scalar()
    {
        var query = "print result = startofday(datetime(2023-01-01 15:30:00))";
        var result = await LastLineOfResult(query);
        result.Should().Contain("2023-01-01 00:00:00");
    }

    [TestMethod]
    public async Task ToIntFunction_Scalar()
    {
        var query = "print result = toint('42')";
        var result = await LastLineOfResult(query);
        result.Should().Be("42");
    }

    [TestMethod]
    public async Task ToLongFunction_Scalar()
    {
        var query = "print result = tolong('42')";
        var result = await LastLineOfResult(query);
        result.Should().Be("42");
    }

    [TestMethod]
    public async Task ToRealFunction_Scalar()
    {
        var query = "print result = toreal('3.14')";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.14");
    }

    [TestMethod]
    public async Task ToDecimalFunction_Scalar()
    {
        var query = "print result = todecimal('3.14')";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.14");
    }

    [TestMethod]
    public async Task ToStringFunction_Scalar()
    {
        var query = "print result = tostring(42)";
        var result = await LastLineOfResult(query);
        result.Should().Be("42");
    }

    [TestMethod]
    public async Task ToDateTimeFunction_Scalar()
    {
        var query = "print result = todatetime('2023-01-01')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("2023-01-01");
    }

    [TestMethod]
    public async Task ToTimespanFunction_Scalar()
    {
        var query = "print result = totimespan('01:00:00')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("01:00:00");
    }

    [TestMethod]
    public async Task ToGuidFunction_Scalar()
    {
        var query = "print result = toguid('d3c1a1e2-5b6a-4c2a-8e2b-1a2b3c4d5e6f')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("d3c1a1e2-5b6a-4c2a-8e2b-1a2b3c4d5e6f");
    }

    [TestMethod]
    public async Task ArrayLengthFunction_Scalar()
    {
        var query = "print result = array_length(dynamic([1,2,3]))";
        var result = await LastLineOfResult(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task ArrayReverseFunction_Scalar()
    {
        var query = "print result = array_reverse(dynamic([1,2,3]))";
        var result = await LastLineOfResult(query);
        result
            .Tokenize("\r\n ")
            .JoinString("")
            .Should().Be("[3,2,1]");
    }

    [TestMethod]
    public async Task IsNullFunction_Scalar()
    {
        var query = "print result = isnull(int(null))";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task IsNotNullFunction_Scalar()
    {
        var query = "print result = isnotnull(42)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task CoalesceFunction_Scalar()
    {
        var query = "print result = coalesce(int(null), 42)";
        var result = await LastLineOfResult(query);
        result.Should().Be("42");
    }

    [TestMethod]
    public async Task IffFunction_Scalar()
    {
        var query = "print result = iff(true, 'yes', 'no')";
        var result = await LastLineOfResult(query);
        result.Should().Be("yes");
    }

    [TestMethod]
    public async Task NotFunction_Scalar()
    {
        var query = "print result = not(false)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task GeoDistance2PointsFunction_Scalar()
    {
        var query = "print result = geo_distance_2points(0,0,1,1)";
        var result = await LastLineOfResult(query);
        double.Parse(result).Should().BeInRange(157000, 158000);
    }

    [TestMethod]
    public async Task StrRepFunction_Scalar()
    {
        var query = "print result = strrep('ab', 3)";
        var result = await LastLineOfResult(query);
        result.Should().Be("ababab");
    }
    
    [TestMethod]
    public async Task StrcatArrayFunction_Scalar()
    {
        var query = "print str = strcat_array(dynamic([1, 2, 3]), '->')";
        var result = await LastLineOfResult(query);
        result.Should().Be("1->2->3");
    }
}
