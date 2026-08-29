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

    [TestMethod]
    public async Task Ipv4Lookup_ReturnUnmatched_Positional_KeepsUnmatchedRows()
    {
        // The trailing return_unmatched flag written positionally: unmatched rows are kept with null lookup columns.
        var query = Lookup +
            "datatable(SourceIP:string)['10.1.2.3', '8.8.8.8'] " +
            "| evaluate ipv4_lookup(LookupTable, SourceIP, Network, true)";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(2);
        var rendered = await ResultAsString(query, ";");
        rendered.Should().Contain("US").And.Contain("8.8.8.8");
    }

    [TestMethod]
    public async Task Ipv4Lookup_ReturnUnmatched_Named_KeepsUnmatchedRows()
    {
        // The documented named form: evaluate ipv4_lookup(.., return_unmatched = true).
        var query = Lookup +
            "datatable(SourceIP:string)['10.1.2.3', '8.8.8.8'] " +
            "| evaluate ipv4_lookup(LookupTable, SourceIP, Network, return_unmatched = true)";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(2);
    }

    [TestMethod]
    public async Task Ipv4Lookup_ExtraKeysNarrowTheMatch()
    {
        // ExtraKeys are columns in BOTH tables that must also match by equality, so an IP inside the CIDR whose
        // Env differs is NOT joined.
        var query =
            "let Ranges = datatable(Network:string, Env:string, Label:string)" +
            "['10.0.0.0/8','prod','ProdNet', '10.0.0.0/8','test','TestNet'];" +
            "datatable(SourceIP:string, Env:string)['10.1.2.3','test'] " +
            "| evaluate ipv4_lookup(Ranges, SourceIP, Network, Env)";
        var result = await ResultAsString(query, ";");
        result.Should().Contain("TestNet").And.NotContain("ProdNet");
    }
}
