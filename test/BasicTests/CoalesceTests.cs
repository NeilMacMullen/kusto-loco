using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class CoalesceTests : TestMethods
{

    [TestMethod]
    public async Task Coalesce()
    {
        var query = """
                    print b=coalesce(bool(null),true),
                    i=coalesce(int(null),int(1)),
                    l2=coalesce(long(null),long(1)),
                    l3=coalesce(long(null),long(null),long(123)),
                    l4=coalesce(long(null),long(null),long(5),long(6)),
                    r=coalesce(real(null),real(1)),
                    dt=coalesce(datetime(null),datetime(2023-01-01)),
                    ts=coalesce(timespan(null),10s),
                    s=coalesce('','a')
                    """;
        var res = await LastLineOfResult(query);
        res.Should().Be("True,1,1,123,5,1,2023-01-01 00:00:00Z,00:00:10,a");
    }
    [TestMethod]
    public async Task BuiltIns_coalesce_Columnar()
    {
        var query = @"
let d =
    datatable(b:bool, i:int, l:long, r:real, dt:datetime, ts:timespan, s:string)
    [
       true, 1, 1, 1, datetime(2023-01-01), 10s, 'a'
    ];
d
| where i==2 // get zero rows
| extend jc=1
| join kind=fullouter (d|extend jc=1) on jc
| project b=coalesce(b,b1),
          i=coalesce(i,i1),
          l=coalesce(l,l1),
          r=coalesce(r,r1),
          dt=coalesce(dt,dt1),
          ts=coalesce(ts,ts1),
          s=coalesce(s,s1)
";
        var res = await LastLineOfResult(query);
        res.Should().Be("True,1,1,1,2023-01-01 00:00:00Z,00:00:10,a");
    }
}
