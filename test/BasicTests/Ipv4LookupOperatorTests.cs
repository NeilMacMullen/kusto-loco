using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KustoLoco.Core;
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

    /// <summary>Serves the lookup table on demand, the way a host supplies reference data.</summary>
    private sealed class OnDemandRanges : IKustoQueryContextTableLoader
    {
        public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames)
        {
            if (tableNames.Contains("Ranges"))
            {
                var t = new System.Data.DataTable("Ranges");
                t.Columns.Add("Cidr", typeof(string));
                t.Columns.Add("Label", typeof(string));
                t.Rows.Add("10.0.0.0/8", "corp");
                context.AddTableFromDataTable(t, "Ranges");
            }
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task Ipv4Lookup_ResolvesADemandLoadedLookupTable()
    {
        // The lookup table is supplied by a table loader, so at analyze time its name is UNRESOLVED — the schema
        // arrives only after the engine has decided which tables to ask for. The plugin's table argument must
        // therefore be discovered syntactically, or the query never gets its table and fails to bind.
        var context = new KustoQueryContext();
        context.SetTableLoader(new OnDemandRanges());
        var result = await context.RunQuery(
            "datatable(SourceIP:string)['10.1.2.3'] | evaluate ipv4_lookup(Ranges, SourceIP, Cidr)");
        result.Error.Should().BeEmpty();
        result.RowCount.Should().Be(1);
    }

    [TestMethod]
    public async Task Ipv4Lookup_ThenFilterAndSummarizeIntoSets()
    {
        // The shape a reputation rule actually forms: enrich by CIDR, filter on an appended column, then aggregate
        // both source and appended columns into sets per actor.
        var query =
            "let Ranges = datatable(Cidr:string, NetworkTrust:string)" +
            "['10.0.0.0/8','tor', '192.168.0.0/16','vpn', '172.16.0.0/12','hosting'];" +
            "datatable(Actor:string, IPAddress:string)" +
            "['a@x','10.1.1.1', 'a@x','192.168.5.5', 'b@x','8.8.8.8'] " +
            "| evaluate ipv4_lookup(Ranges, IPAddress, Cidr) " +
            "| where NetworkTrust in ('tor','vpn','hosting') " +
            "| summarize Ips = make_set(IPAddress), Trusts = make_set(NetworkTrust) by Actor";
        var result = await SquashedLastLineOfResult(query);
        // 'a@x' matched two different anonymizer classes; 'b@x' matched nothing and is absent.
        result.Should().Contain("tor").And.Contain("vpn");
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
