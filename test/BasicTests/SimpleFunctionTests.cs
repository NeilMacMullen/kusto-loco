using AwesomeAssertions;
using NotNullStrings;

namespace BasicTests;


[TestClass]
public class SimpleFunctionTests : TestMethods
{


    [TestMethod]
    public async Task Cast()
    {
        var query = "print trim_start(@'a+','aaainnerbbba')";
        var result = await LastLineOfResult(query);
        result.Should().Be("innerbbba");
    }


    [TestMethod]
    public async Task TrimStart()
    {
        var query = "print trim_start(@'a+','aaainnerbbba')";
        var result = await LastLineOfResult(query);
        result.Should().Be("innerbbba");
    }

    [TestMethod]
    public async Task TrimEnd()
    {
        var query = "print trim_end(@'b+','baaainnerbbb')";
        var result = await LastLineOfResult(query);
        result.Should().Be("baaainner");
    }

    [TestMethod]
    public async Task Trim()
    {
        var query = "print trim(@'a+','aaaainneraaaa')";
        var result = await LastLineOfResult(query);
        result.Should().Be("inner");
    }

    [TestMethod]
    public async Task Case()
    {
        var query = @" 
datatable(Size:int) [7] 
| extend S= case(Size <= 3, 'Small',                        
                 Size <= 10, 'Medium', 
                             'Large')";
        var result = await ResultAsString(query);
        result.Should().Be("7,Medium");
    }

    [TestMethod]
    public async Task CaseDefault()
    {
        var query = @" 
datatable(Size:int) [50] 
|extend S= case(Size <= 3, 'Small',                        
              Size <= 10, 'Medium', 
                          'Large')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("Large");
    }

    [TestMethod]
    public async Task GeoDistance2PointsScalar()
    {
        var query = " print distance_in_meters = geo_distance_2points(-122.407628, 47.578557, -118.275287, 34.019056)";
        var result = await LastLineOfResult(query);
        result.Should().Contain("15467");
    }

    [TestMethod]
    public async Task GeoPointToGeoHashScalar()
    {
        var query = "print geohash = geo_point_to_geohash(139.806115, 35.554128, 12)";
        var result = await LastLineOfResult(query);
        result.Should().Contain("xn76m27ty9g4");
    }

    [TestMethod]
    public async Task GeoPointToGeoHashScalarWithDefault()
    {
        var query = "print geohash = geo_point_to_geohash(139.806115, 35.554128)";
        var result = await LastLineOfResult(query);
        result.Should().Be("xn76m");
    }


    [TestMethod]
    public async Task GeoHashToCentralPointScalar()
    {
        var query = @"print point = geo_geohash_to_central_point('sunny')
                      | extend coordinates = point.coordinates
                      | project longitude = coordinates[0]";
        var result = await LastLineOfResult(query);
        result.Should().Contain("42.4731445");
    }


    [TestMethod]
    public async Task SplitScalar()
    {
        var query = "print c=split('this.is.a.string.and.I.need.the.last.part', '.')[-1]";
        var result = await LastLineOfResult(query);
        result.Should().Be("part");
    }

    [TestMethod]
    public async Task SplitWithRequestedIndex()
    {
        var query = """print split("aaa_bbb_ccc", "_", 1)""";
        var result = await LastLineOfResult(query);
        result.Should().Be("bbb");
    }


    [TestMethod]
    public async Task ToLower()
    {
        var query = "print c=tolower('ABCdef')";
        var result = await LastLineOfResult(query);
        result.Should().Be("abcdef");
    }

    [TestMethod]
    public async Task ToUpper()
    {
        var query = "print c=toupper('ABCdef')";
        var result = await LastLineOfResult(query);
        result.Should().Be("ABCDEF");
    }

    [TestMethod]
    public async Task Strlen()
    {
        var query = "print c=strlen('ABCdef')";
        var result = await LastLineOfResult(query);
        result.Should().Be("6");
    }

    [TestMethod]
    public async Task SubString()
    {
        var query = "print c=substring('ABCdef',2,3)";
        var result = await LastLineOfResult(query);
        result.Should().Be("Cde");
    }

    [TestMethod]
    public async Task SubStringSingleParameter()
    {
        var query = "print c=substring('ABCdef',2)";
        var result = await LastLineOfResult(query);
        result.Should().Be("Cdef");
    }

    [TestMethod]
    public async Task Substring_OutOfRange_ShouldReturnEmpty()
    {
        var query = "print c=substring('abc', 10, 5)";
        var result = await LastLineOfResult(query);
        result.Should().Be(""); // KQL returns empty string if start index is out of range
    }

    [TestMethod]
    public async Task DivideByZero_ShouldReturnNullOrError()
    {
        var query = "print c=1/0";
        var result = await LastLineOfResult(query);
        result.Should().Contain("null");
    }


    [TestMethod]
    public async Task ToInt_InvalidString_ShouldReturnNull()
    {
        var query = "print c=toint('notanumber')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("null"); // KQL returns null for invalid conversion
    }

    [TestMethod]
    public async Task Trimws()
    {
        var query = "print c=trimws('   abc   ')";
        var result = await LastLineOfResult(query);
        result.Should().Be("abc");
    }


    [TestMethod]
    public async Task PadLeft()
    {
        var query = "print c=padleft('abc',6)";
        var result = await LastLineOfResult(query);
        result.Should().Be("   abc");
    }

    [TestMethod]
    public async Task PadLeftWithChar()
    {
        var query = "print c=padleft('abc',6,'A')";
        var result = await LastLineOfResult(query);
        result.Should().Be("AAAabc");
    }


    [TestMethod]
    public async Task PadLeftWithBlankChar()
    {
        var query = "print c=padleft('abc',6,'')";
        var result = await LastLineOfResult(query);
        result.Should().Be("   abc");
    }

    [TestMethod]
    public async Task PadRight()
    {
        var query = "print c=padright('abc',6)";
        var result = await LastLineOfResult(query);
        result.Should().Be("abc   ");
    }

    [TestMethod]
    public async Task PadRightWithChar()
    {
        var query = "print c=padright('abc',6,'A')";
        var result = await LastLineOfResult(query);
        result.Should().Be("abcAAA");
    }


    [TestMethod]
    public async Task PadRightWithBlankChar()
    {
        var query = "print c=padright('abc',6,'')";
        var result = await LastLineOfResult(query);
        result.Should().Be("abc   ");
    }

    [TestMethod]
    public async Task TimespanFormatting()
    {
        var query = "print 1d";
        var result = await LastLineOfResult(query);
        result.Should().Be("1.00:00:00");
    }


    [TestMethod]
    public async Task Range()
    {
        var query = "range i from 1 to 10 step 1";
        var result = await LastLineOfResult(query);
        result.Should().Be("10");
    }

    [TestMethod]
    public async Task RangeDescending()
    {
        var query = "range i from 10 to 1 step -1";
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }


    [TestMethod]
    public async Task RowNumberNoParam()
    {
        var query = "range i from 1 to 10 step 1 | extend r =row_number()";
        var result = await LastLineOfResult(query);
        result.Should().Be("10,10");
    }

    [TestMethod]
    public async Task RowNumberStartingAt7()
    {
        var query = "range i from 1 to 5 step 1 | extend r =row_number(7)";
        var result = await LastLineOfResult(query);
        result.Should().Be("5,11");
    }

    [TestMethod]
    public async Task RowNumberWithRanking()
    {
        var query = @"range i from 1 to 100 step 1 
| extend r =row_number(1,i%10==0) 
| where r==1 
| count";
        var result = await LastLineOfResult(query);
        result.Should().Be("11");
    }

    [TestMethod]
    public async Task BetweenLong()
    {
        //ensure we didn't get any fractional values
        var query = @"range x from 1 to 100 step 1
| where x between (50 .. 55)";
        var result = await LastLineOfResult(query);
        result.Should().Be("55");
    }

    [TestMethod]
    public async Task BetweenInt()
    {
        //ensure we didn't get any fractional values
        var query = @"range i from 1 to 100 step 1
| extend i=toint(i) | where i between (50 .. 55)";
        var result = await LastLineOfResult(query);
        result.Should().Be("55");
    }


    [TestMethod]
    public async Task RangeTimeSpan()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from 1d to 20d step 1d";
        var result = await LastLineOfResult(query);
        result.Should().Be("20.00:00:00");
    }

    [TestMethod]
    public async Task RangeTimeSpanFiltered()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from 1d to 20d step 1d | where x < 5d ";
        var result = await LastLineOfResult(query);
        result.Should().Be("4.00:00:00");
    }


    [TestMethod]
    public async Task NotBetweenLong()
    {
        //ensure we didn't get any fractional values
        var query = @"range x from 1 to 10 step 1
| where x !between (9 .. 11)";
        var result = await LastLineOfResult(query);
        result.Should().Be("8");
    }


    [TestMethod]
    public async Task LogTest()
    {
        var query = "print v1 = log(4.5)";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("1.504");
    }

    [TestMethod]
    public async Task LogTest2()
    {
        var query = "datatable(a:real) [4.5,4.5] | project v1 = log(a)";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("1.504");
    }

    [TestMethod]
    public async Task StrLen()
    {
        var query = "datatable(a:string) ['abc'] | project v1 = strlen(a)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task StrLen0()
    {
        var query = "datatable(a:string) [''] | project v1 = strlen(a)";
        var result = await LastLineOfResult(query);
        result.Should().Be("0");
    }


    [TestMethod]
    public async Task MultiplyTimespan()
    {
        var query = "print D=1d * 10";
        var result = await LastLineOfResult(query);
        result.Should().Be("10.00:00:00");
    }

    [TestMethod]
    public async Task MultiplyTimespanR()
    {
        var query = "print D=10*1d";
        var result = await LastLineOfResult(query);
        result.Should().Be("10.00:00:00");
    }

    [TestMethod]
    public async Task AbsInt()
    {
        var query = "print D=abs(toint(-99))";
        var result = await LastLineOfResult(query);
        result.Should().Be("99");
    }

    [TestMethod]
    public async Task TimeSpanBin()
    {
        var query = "print D=bin(26h,1d)";
        var result = await LastLineOfResult(query);
        result.Should().Be("1.00:00:00");
    }


    [TestMethod]
    public async Task Range_Step_0_GeneratesError()
    {
        var query = "range x from 1 to 10 step 0";
        var context = CreateContext();
        var result = await context.RunQuery(query);
        result.RowCount.Should().Be(0);
        if (result.Error.Length != 0)
            result.Error.Should().Contain("The expression must not be the value: 0");
    }


    [TestMethod]
    public async Task TimespanDiv()
    {
        var query = "print 10d/2";
        var result = await LastLineOfResult(query);
        result.Should().Be("5.00:00:00");
    }

    [TestMethod]
    public async Task TimespanDiv2()
    {
        var query = "print 10d/2.5";
        var result = await LastLineOfResult(query);
        result.Should().Be("4.00:00:00");
    }

    [TestMethod]
    public async Task ToTimespan()
    {
        var query = " print totimespan('1.02:04:45')";
        var result = await LastLineOfResult(query);
        result.Should().Be("1.02:04:45");
    }

    [TestMethod]
    public async Task AvgNumber()
    {
        var query = "datatable(a:long) [1,3,2] | summarize avg(a)";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task AvgTimeSpan()
    {
        var query = "datatable(a:timespan) [1d,3d,2d] | summarize avg(a)";
        var result = await LastLineOfResult(query);
        result.Should().Be("2.00:00:00");
    }

    [TestMethod]
    public async Task SumTimeSpan()
    {
        var query = "datatable(a:timespan) [1d,3d,2d] | summarize sum(a)";
        var result = await LastLineOfResult(query);
        result.Should().Be("6.00:00:00");
    }


    [TestMethod]
    public async Task RepeatedUnion()
    {
        var query = @"
let d=datatable(v1: int,v2: int, v3:int) [
1,2,3,
4,5,6
];
d | project Type='v1',Val=v1
| union (d | project Type='v2',Val=v2)
| union (d | project Type='v3',Val=v3)
| where Type == 'v2'
| count";

        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task QuoteSlash()
    {
        var query = """ 
                    datatable(T:string) ['abcd','"def']
                    | where T contains "\"def"
                    | count
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("1");
    }


    [TestMethod]
    public async Task RoundDouble()
    {
        var query = "print round(3.14,1)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.1");
    }


    [TestMethod]
    public async Task LogDouble()
    {
        var query = "print log(4)";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("1.3");
    }

    [TestMethod]
    public async Task SignInt()
    {
        var query = "print sign(-4)";
        var result = await LastLineOfResult(query);
        result.Should().Be("-1");
    }

    [TestMethod]
    public async Task ToLongHex()
    {
        var query = "print parsehex('a0')";
        var result = await LastLineOfResult(query);
        result.Should().Be("160");
    }

    [TestMethod]
    public async Task ToLongHexWithPrefix()
    {
        var query = "print parsehex('0xa0')";
        var result = await LastLineOfResult(query);
        result.Should().Be("160");
    }

    [TestMethod]
    public async Task ToHex()
    {
        var query = "print tohex(160)";
        var result = await LastLineOfResult(query);
        result.Should().Be("a0");
    }

    [TestMethod]
    public async Task ToLong2()
    {
        var query = "print tolong('0xa0')";
        var result = await LastLineOfResult(query);
        result.Should().Be("160");
    }

    [TestMethod]
    public async Task MultiStringCat()
    {
        var query = "print strcat('1','2','3','4','5','6','7','8','9','0','1','2','3','4','5')";
        var result = await LastLineOfResult(query);
        result.Should().Be("123456789012345");
    }

    [TestMethod]
    public async Task IsEmpty()
    {
        var query = "print isempty('')";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task IsEmpty2()
    {
        var query = "print isempty(' ')";
        var result = await LastLineOfResult(query);
        result.Should().Be("False");
    }

    [TestMethod]
    public async Task IsNotEmpty()
    {
        var query = "print isnotempty('')";
        var result = await LastLineOfResult(query);
        result.Should().Be("False");
    }

    [TestMethod]
    public async Task IsNotEmpty2()
    {
        var query = "print isnotempty(' ')";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task Ipv4IsPrivate()
    {
        (await LastLineOfResult("print ipv4_is_private('10.0.0.1')")).Should().Be("True");
        (await LastLineOfResult("print ipv4_is_private('192.168.0.1')")).Should().Be("True");
        (await LastLineOfResult("print ipv4_is_private('172.16.0.1')")).Should().Be("True");

        // It should not support loopback and link-local addresses
        (await LastLineOfResult("print ipv4_is_private('127.0.0.1')")).Should().Be("False");
        (await LastLineOfResult("print ipv4_is_private('169.254.0.1')")).Should().Be("False");
    }

    [TestMethod]
    public async Task IsAscii()
    {
        var query = "print isascii('blahhh')";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task IsUtf8()
    {
        var query = "print isutf8('blahhh')";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task Reverse()
    {
        var query = "print reverse('acdef')";
        var result = await LastLineOfResult(query);
        result.Should().Be("fedca");
    }

    [TestMethod]
    public async Task IsFinite()
    {
        var query = "print isfinite(1.0/10)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }


    [TestMethod]
    public async Task IsFinite2()
    {
        var query = "print isfinite(1.0/0.0)";
        var result = await LastLineOfResult(query);
        result.Should().Be("False");
    }

    [TestMethod]
    public async Task BinaryAnd()
    {
        var a = 0b101101;
        var b = 0b111101;
        var query = $"print binary_and({a},{b})";
        var result = await LastLineOfResult(query);
        result.Should().Be((a & b).ToString());
    }

    [TestMethod]
    public async Task BinaryOr()
    {
        var a = 0b101101;
        var b = 0b111101;
        var query = $"print binary_or({a},{b})";
        var result = await LastLineOfResult(query);
        result.Should().Be((a | b).ToString());
    }

    [TestMethod]
    public async Task BinaryXOr()
    {
        var a = 0b101101;
        var b = 0b111101;
        var query = $"print binary_xor({a},{b})";
        var result = await LastLineOfResult(query);
        result.Should().Be((a ^ b).ToString());
    }

    [TestMethod]
    public async Task BinaryNot()
    {
        var a = 0b101101;
        var query = $"print binary_not({a})";
        var result = await LastLineOfResult(query);
        result.Should().Be((~a).ToString());
    }

    [TestMethod]
    public async Task BitSetCountOnes()
    {
        var a = 0b101101;
        var query = $"print bitset_count_ones({a})";
        var result = await LastLineOfResult(query);
        result.Should().Be("4");
    }

    [TestMethod]
    public async Task BinaryShiftLeft()
    {
        var a = 17;
        var b = 6;
        var query = $"print binary_shift_left({a},{b})";
        var result = await LastLineOfResult(query);
        result.Should().Be((a << b).ToString());
    }

    [TestMethod]
    public async Task BinaryShiftRight()
    {
        var a = 123478324875648645;
        var b = 6;
        var query = $"print binary_shift_right({a},{b})";
        var result = await LastLineOfResult(query);
        result.Should().Be((a >> b).ToString());
    }

    [TestMethod]
    public async Task ToGuid()
    {
        var g = Guid.NewGuid().ToString();
        var query = $"print toguid('{g}')";
        var result = await LastLineOfResult(query);
        result.Should().Be(g);
    }


    [TestMethod]
    public async Task MakeTimespan()
    {
        var query = "print make_timespan(1,15)";
        var result = await LastLineOfResult(query);
        result.Should().Be("01:15:00");
    }

    [TestMethod]
    public async Task MakeTimespan2()
    {
        var query = "print make_timespan(1,15,10)";
        var result = await LastLineOfResult(query);
        result.Should().Be("01:15:10");
    }

    [TestMethod]
    public async Task MaxLongTest()
    {
        //ensure precision is not lost with max
        var query = @"let letters = datatable(bitmap:long)
[851673153924341805,];
print toscalar(letters | summarize mx=max(bitmap));";
        var result = await LastLineOfResult(query);
        result.Should().Be("851673153924341805");
    }

    [TestMethod]
    public async Task MinLongTest()
    {
        //ensure precision is not lost with min
        var query = @"let letters = datatable(bitmap:long)
[851673153924341808,851673153924341805];
print toscalar(letters | summarize mx=min(bitmap));";
        var result = await LastLineOfResult(query);
        result.Should().Be("851673153924341805");
    }


    [TestMethod]
    public async Task CountOfTest()
    {
        var query = "print countof('abc abc ab','abc')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task CountOfNormalTest()
    {
        var query = "print countof('abc abc ab','abc','normal')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task CountOfRegexTest()
    {
        var query = "print countof('abc abc ab a','a.','regex')";
        var result = await LastLineOfResult(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task IndexOfTest()
    {
        (await LastLineOfResult("print indexof('abc def ab','def')")).Should().Be("4");
        (await LastLineOfResult("print indexof('abc def ab','xyz')")).Should().Be("-1");
        (await LastLineOfResult("print indexof('abc def ab','ab',7)")).Should().Be("8");
        (await LastLineOfResult("print indexof('abc def ab','ab',-2)")).Should().Be("8");

        //test cases from docs        
        (await LastLineOfResult("""print indexof("abcdefg","cde")""")).Should().Be("2");
        (await LastLineOfResult("""print indexof("abcdefg","cde",1,4)""")).Should().Be("2");
        (await LastLineOfResult("""print indexof(   "abcdefg","cde",1,2     )""")).Should().Be("-1");
        (await LastLineOfResult("""print indexof(   "abcdefg","cde",3,4    )""")).Should().Be("-1");
        (await LastLineOfResult("""print indexof(  "abcdefg","cde",-5    )""")).Should().Be("2");
        (await LastLineOfResult("""print indexof(  1234567,5,1,4  )""")).Should().Be("4");
        (await LastLineOfResult("""print indexof(  "abcdefg","cde",2,-1    )""")).Should().Be("2");
        (await LastLineOfResult("""print indexof(  "abcdefgabcdefg", "cd", 1, 10, 2  )""")).Should().Be("9");
        (await LastLineOfResult("""print indexof(  "abcdefgabcdefg", "cde", 1, -1, 3  )""")).Should().Be("-1");
    }

    [TestMethod]
    public async Task ReplaceString()
    {
        var query = "print replace_string('A magic trick can turn a cat into a dog','cat','hamster')";
        var result = await LastLineOfResult(query);
        result.Should().Be("A magic trick can turn a hamster into a dog");
    }


    [TestMethod]
    public async Task StrRep()
    {
        var query = "print strrep('ABC', 2)";
        var result = await LastLineOfResult(query);

        result.Should().Be("ABCABC");
    }


    [TestMethod]
    public async Task StrcatArray()
    {
        var query = """
                    let words = datatable(word:string, code:string) [
                        "apple","A",
                        "orange","B",
                        "grapes","C"
                    ];
                    words
                    | summarize result = strcat_array(make_list(word), ",")

                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("apple,orange,grapes");
    }


    [TestMethod]
    public async Task array_concat()
    {
        var query = """
                    datatable(id:int, values:dynamic)
                    [
                        1, dynamic([1, 2, 3]),
                        2, dynamic([4, 5]),
                        3, dynamic([6, 7, 8])
                    ]
                    | summarize cat =(array_concat(make_list(values)))
                    | extend t =strcat_array(cat,",")
                    | project t
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("1,2,3,4,5,6,7,8");
    }

    [TestMethod]
    public async Task Strcmp()
    {
        var query = """
                    datatable(string1:string, string2:string) [
                        "ABC","ABC",
                        "abc","ABC",
                        "ABC","abc",
                        "abcde","abc"
                    ]
                    | extend result = strcmp(string1,string2)
                    | summarize cat = strcat_array(make_list(result), ",")

                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("0,1,-1,1");
    }

    [TestMethod]
    public async Task StrcatDelim()
    {
        var query = """
                    print st = strcat_delim('-', 1, '2', 'A', 1s)
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("1-2-A-00:00:01");
    }

    [TestMethod]
    public async Task Around()
    {
        var query = """
                    print st = around(100,99,2)
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("True");
    }

    [TestMethod]
    public async Task Pi()
    {
        var query = """
                    print pi()
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Contain("3.1415");
    }


    [TestMethod]
    public async Task PiColumnar()
    {
        var query = """
                    range i from 1 to 10 step 1
                    | extend p=pi()
                    | summarize sum(p)
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Contain("31.415");
    }

    [TestMethod]
    public async Task ArrayRotateLeft()
    {
        var query = """
                    print arr=dynamic([1,2,3,4,5])
                    | project x=array_rotate_left(arr, 2)
                    | project y=strcat_array(x,",")
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("3,4,5,1,2");
    }

    [TestMethod]
    public async Task ArrayRotateLeftNeg()
    {
        var query = """
                    print arr=dynamic([1,2,3,4,5])
                    | project x=array_rotate_left(arr, -2)
                    | project y=strcat_array(x,",")
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("4,5,1,2,3");
    }

    [TestMethod]
    public async Task ArrayRotateRight()
    {
        var query = """
                    print arr=dynamic([1,2,3,4,5])
                    | project x=array_rotate_right(arr, 2)
                    | project y=strcat_array(x,",")
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("4,5,1,2,3");
    }

    [TestMethod]
    public async Task ArrayRotateRightNeg()
    {
        var query = """
                    print arr=dynamic([1,2,3,4,5])
                    | project x=array_rotate_right(arr,-2)
                    | project y=strcat_array(x,",")
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("3,4,5,1,2");
    }

    [TestMethod]
    public async Task In()
    {
        var query = """
                    datatable(A:string) [
                    "AAA",
                    "B",
                    "C"
                    ]
                     | where A in ('AAA','b') | count
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }

    [TestMethod]
    public async Task InCs()
    {
        var query = """
                    datatable(A:string) [
                    "AAA",
                    "B",
                    "C"
                    ]
                     | where A in~ ('AAA','b') | count
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task InLong()
    {
        var query = """
                    datatable(A:long) [
                    123,
                    456,
                    2
                    ]
                     | where A in (1,2,3,123) | count
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task HasAny()
    {
        var query = """
                    datatable(A:string) [
                    "AAA",
                    "B",
                    "C"
                    ]
                     | where A has_any('aA','b') | count
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task HasAll()
    {
        var query = """
                    datatable(A:string) [
                    "AAA",
                    "BAAAaa",
                    "C"
                    ]
                     | where A has_all('aA','b') | count
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }

    [TestMethod]
    public async Task Has()
    {
        var query = """
                    datatable(A:string) [
                    "AAA",
                    "BAAAaa",
                    "C"
                    ]
                     | where A has('aA') | count
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task DecimalLiteral()
    {
        var query = "print decimal(1.23)";
        var result = await LastLineOfResult(query);
        result.Should().Be("1.23");
    }

    [TestMethod]
    public async Task RealLiteral()
    {
        var query = "print double(1.23)";
        var result = await LastLineOfResult(query);
        result.Should().Be("1.23");
    }

    [TestMethod]
    public async Task LongLiteral()
    {
        var query = "print long(123)";
        var result = await LastLineOfResult(query);
        result.Should().Be("123");
    }

    [TestMethod]
    public async Task DecimalAddition()
    {
        var query = "print result = decimal(1.1) + decimal(2.2)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.3");
    }

    [TestMethod]
    public async Task DecimalSubtraction()
    {
        var query = "print result = decimal(5.5) - decimal(2.2)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.3");
    }

    [TestMethod]
    public async Task DecimalMultiplication()
    {
        var query = "print result = decimal(1.5) * decimal(2.0)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.00");
    }

    [TestMethod]
    public async Task DecimalMultiplicationType()
    {
        var query = "print result = decimal(1.5) * decimal(2.0)|getschema";
        var result = await LastLineOfResult(query);
        result.Should().Contain("decimal");
    }

    [TestMethod]
    public async Task DecimalDivision()
    {
        var query = "print result = decimal(7.5) / decimal(2.5)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task DecimalDivisionType()
    {
        var query = "print result = decimal(7.5) / decimal(2.5)|getschema";
        var result = await LastLineOfResult(query);
        result.Should().Contain("decimal");
    }

    [TestMethod]
    public async Task DecimalComparison()
    {
        var query = "print result = decimal(2.5) > decimal(1.5)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task DecimalEquality()
    {
        var query = "print result = decimal(3.3) == decimal(3.3)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task DecimalInTableSummarize()
    {
        var query = "datatable(a:decimal) [1.1, 2.2, 3.3] | summarize total = sum(a)";
        var result = await LastLineOfResult(query);
        result.Should().Be("6.6");
    }

    [TestMethod]
    public async Task DecimalRound()
    {
        var query = "print result = round(decimal(3.14159), 2)";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.14");
    }

    [TestMethod]
    public async Task NegativeTimespan()
    {
        var query = "print result = -10d";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("-10");
    }


    [TestMethod]
    public async Task BoolSort()
    {
        var dataPrefix = "datatable(b: bool)[true,false,bool(null) ] |";

        async Task Check(string query, string results)
        {
            query = dataPrefix + query;
            var result = await ResultAsString(query);
            result.Should().Be(results);
        }

        await Check("take 2 | order by b asc", "False,True");
        await Check("take 2 | order by b desc", "True,False");
        await Check("take 2 | order by b asc nulls first", "False,True");
        await Check("take 2 | order by b desc nulls first", "True,False");
        await Check("take 2 | order by b asc nulls last", "False,True");
        await Check("take 2 | order by b desc nulls last", "True,False");

        await Check("order by b asc", "<null>,False,True");
        await Check("order by b desc", "True,False,<null>");

        await Check("order by b desc nulls first", "<null>,True,False");
        await Check("order by b asc nulls last", "False,True,<null>");
        await Check("order by b desc nulls last", "True,False,<null>");
        await Check("order by b asc nulls first", "<null>,False,True");
    }


    [TestMethod]
    public async Task PiScalar()
    {
        var query = "print pi()";
        var result = await LastLineOfResult(query);
        result.Should().Contain("3.14");
    }

    [TestMethod]
    public async Task PiColumn()
    {
        var query = """
                    range i from 1 to 3 step 1
                    | extend r = pi()
                    | project r
                    """;
        var result = await ResultAsString(query);
        var lines = result.Tokenize(",");
        lines.Should().AllSatisfy(c => c.Should().Contain("3.14"));
        lines.Length.Should().Be(3);
    }

    [TestMethod]
    public async Task IffCanReturnNull()
    {
        var query = """
                    print iff(true,int(null),5)
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Contain("null");
    }
    [TestMethod]
    public async Task AggChecks()
    {
        var query = """
                    datatable(n:int)[1]
                    | summarize avg(n)
                    | getschema
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Contain("real");
    }





    [TestMethod]
    public async Task BuiltIns_isnull_Columnar()
    {
        var query = """

                    datatable(b:bool, i:int, l:long, r:real, d:datetime, t:timespan, s:string) [
                      bool(null), int(null), long(null), real(null), datetime(null), timespan(null), '',
                      false, 0, 0, 0, datetime(null), 0s, ' ',
                      true, 1, 2, 3.5, datetime(2023-02-26), 5m, 'hello'
                    ]
                    | project b=isnull(b), i=isnull(i), l=isnull(l), r=isnull(r), d=isnull(d), t=isnull(t), s=isnull(s)

                    """;

        var expected = """
                       True,True,True,True,True,True,False
                       False,False,False,False,True,False,False
                       False,False,False,False,False,False,False
                       """;

        var result = await ResultAsString(query, Environment.NewLine);
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task Format()
    {
        var query = "print format(1234, 'x')";
        var result = await LastLineOfResult(query);
        result.Should().Be("4d2");
    }

    [TestMethod]
    public async Task Format1()
    {
        var query = "print format(1234, 'X')";
        var result = await LastLineOfResult(query);
        result.Should().Be("4D2");
    }

    [TestMethod]
    public async Task FormatInterp()
    {
        var query = "print format_interp('hi there {0:x}',256)";
        var result = await LastLineOfResult(query);
        result.Should().Be("hi there 100");
    }

    [TestMethod]
    public async Task FormatInterpColumnar()
    {
        var query = """

                    datatable(b:bool, i:int, ) [
                      false, 0,
                      true, 1 
                    ]
                    | project b=format_interp('bool={0}, int={1}',b,i)

                    """;

        var expected = """
                       bool=False, int=0
                       bool=True, int=1
                       """;

        var result = await ResultAsString(query, Environment.NewLine);
        result.Should().Be(expected);
    }


}

