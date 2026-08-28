using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class Ipv4LookupOperatorTests : TestMethods
{
    private const string Lookup =
        "let LookupTable = datatable(Network:string, Country:string)" +
        "['10.0.0.0/8','US', '192.168.0.0/16','LAN'];";

    [TestMethod]
    public async Task Ipv4Lookup_MatchesCidrAndAppendsColumns()
    {
        var query = Lookup +
            "datatable(SourceIP:string)['10.1.2.3', '192.168.5.5', '8.8.8.8'] " +
            "| evaluate ipv4_lookup(LookupTable, SourceIP, Network)";
        var result = await ResultAsString(query, ";");
        // 10.1.2.3 -> US, 192.168.5.5 -> LAN; 8.8.8.8 matches nothing and is dropped.
        result.Should().Contain("US").And.Contain("LAN");
        result.Should().NotContain("8.8.8.8");
    }

    [TestMethod]
    public async Task Ipv4Lookup_UnmatchedRowsDropped()
    {
        var query = Lookup +
            "datatable(SourceIP:string)['8.8.8.8', '1.1.1.1'] " +
            "| evaluate ipv4_lookup(LookupTable, SourceIP, Network)";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(0);
    }
}
