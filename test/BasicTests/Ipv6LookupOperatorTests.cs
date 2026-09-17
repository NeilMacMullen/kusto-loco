using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KustoLoco.Core;
using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class Ipv6LookupOperatorTests : TestMethods
{
    private const string Lookup =
        "let LookupTable = datatable(Network:string, Country:string)" +
        "['2001:db8::/32','DOC', 'fd00::/8','ULA'];";

    [TestMethod]
    public async Task Ipv6Lookup_MatchesCidrAndAppendsColumns()
    {
        var query = Lookup +
            "datatable(SourceIP:string)['2001:db8::1', 'fd00::5', '2606:4700::1111'] " +
            "| evaluate ipv6_lookup(LookupTable, SourceIP, Network)";
        var result = await ResultAsString(query, ";");
        // 2001:db8::1 -> DOC, fd00::5 -> ULA; 2606:4700::1111 matches nothing and is dropped.
        result.Should().Contain("DOC").And.Contain("ULA");
        result.Should().NotContain("2606:4700");
    }

    [TestMethod]
    public async Task Ipv6Lookup_UnmatchedRowsDropped()
    {
        var query = Lookup +
            "datatable(SourceIP:string)['2606:4700::1111', '2620:fe::fe'] " +
            "| evaluate ipv6_lookup(LookupTable, SourceIP, Network)";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task Ipv6Lookup_ReturnUnmatched_Positional_KeepsUnmatchedRows()
    {
        var query = Lookup +
            "datatable(SourceIP:string)['2001:db8::1', '2606:4700::1111'] " +
            "| evaluate ipv6_lookup(LookupTable, SourceIP, Network, true)";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(2);
        var rendered = await ResultAsString(query, ";");
        rendered.Should().Contain("DOC").And.Contain("2606:4700");
    }

    [TestMethod]
    public async Task Ipv6Lookup_ReturnUnmatched_Named_KeepsUnmatchedRows()
    {
        var query = Lookup +
            "datatable(SourceIP:string)['2001:db8::1', '2606:4700::1111'] " +
            "| evaluate ipv6_lookup(LookupTable, SourceIP, Network, return_unmatched = true)";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(2);
    }

    // ADX's ipv6_* functions accept BOTH families, mapping IPv4 into IPv6-mapped space rather than rejecting it.
    // These two tests pin that behaviour in both directions, because a matcher that silently refused IPv4 would
    // look correct on pure-IPv6 data and quietly drop every IPv4 row in a mixed table.
    [TestMethod]
    public async Task Ipv6Lookup_AcceptsAnIpv4SourceAgainstAnIpv4Cidr()
    {
        var query =
            "let Ranges = datatable(Network:string, Label:string)['10.0.0.0/8','corp'];" +
            "datatable(SourceIP:string)['10.1.2.3', '8.8.8.8'] " +
            "| evaluate ipv6_lookup(Ranges, SourceIP, Network)";
        var result = await ResultAsString(query, ";");
        result.Should().Contain("corp");
        result.Should().NotContain("8.8.8.8");
    }

    [TestMethod]
    public async Task Ipv6Lookup_MatchesAnIpv4SourceAgainstTheIpv4MappedRange()
    {
        // ::ffff:10.0.0.0/104 is the IPv4-mapped form of 10.0.0.0/8 (96 + 8), so an IPv4 source must match it.
        var query =
            "let Ranges = datatable(Network:string, Label:string)['::ffff:10.0.0.0/104','mapped'];" +
            "datatable(SourceIP:string)['10.1.2.3'] " +
            "| evaluate ipv6_lookup(Ranges, SourceIP, Network)";
        var result = await ResultAsString(query, ";");
        result.Should().Contain("mapped");
    }

    [TestMethod]
    public async Task Ipv6Lookup_ZeroPrefixMatchesEveryAddress()
    {
        // A /0 is the one prefix that cannot be produced by shifting: UInt128 shift counts are masked by 127, so a
        // naive MaxValue << 128 yields MaxValue and a /0 would match NOTHING instead of everything.
        var query =
            "let Ranges = datatable(Network:string, Label:string)['::/0','any'];" +
            "datatable(SourceIP:string)['2001:db8::1', '10.1.2.3'] " +
            "| evaluate ipv6_lookup(Ranges, SourceIP, Network)";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(2);
    }

    // ExtraKeys are NOT expressible for ipv6_lookup today, and that is a limit of the query grammar rather than of
    // this implementation. Kusto.Language (12.4.1) types the 4th argument of ipv6_lookup as a bool LITERAL — a
    // column reference there is rejected during semantic analysis with "A value of type bool expected / The
    // expression must be a literal" — whereas the same position on ipv4_lookup accepts extra key columns (see
    // Ipv4LookupOperatorTests.Ipv4Lookup_ExtraKeysNarrowTheMatch, which passes).
    //
    // The evaluator itself is family-agnostic and would narrow on extra keys identically if the grammar ever
    // admitted them, so nothing here special-cases IPv6. This test pins the CURRENT boundary so the difference is
    // visible rather than surfacing later as a confusing semantic error; it starts failing if the grammar gains
    // extra-key support, which is the signal to replace it with the positive ipv4 equivalent.
    [TestMethod]
    public async Task Ipv6Lookup_ExtraKeysAreRejectedByTheQueryGrammar()
    {
        var query =
            "let Ranges = datatable(Network:string, Env:string, Label:string)" +
            "['2001:db8::/32','prod','ProdNet', '2001:db8::/32','test','TestNet'];" +
            "datatable(SourceIP:string, Env:string)['2001:db8::1','test'] " +
            "| evaluate ipv6_lookup(Ranges, SourceIP, Network, Env)";
        var result = await CreateContext().RunQuery(query);
        result.Error.Should().NotBeEmpty();
    }

    /// <summary>Serves the lookup table on demand, the way a host supplies reference data.</summary>
    private sealed class OnDemandRanges : IKustoQueryContextTableLoader
    {
        public Task LoadTablesAsync(KustoQueryContext context, IReadOnlyCollection<string> tableNames)
        {
            if (tableNames.Contains("V6Ranges"))
            {
                var t = new System.Data.DataTable("V6Ranges");
                t.Columns.Add("Cidr", typeof(string));
                t.Columns.Add("Label", typeof(string));
                t.Rows.Add("2001:db8::/32", "doc");
                context.AddTableFromDataTable(t, "V6Ranges");
            }
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task Ipv6Lookup_ResolvesADemandLoadedLookupTable()
    {
        // Same requirement as ipv4_lookup: at analyze time the demand-loaded table's name is unresolved, so the
        // plugin's table argument must be discovered syntactically or the loader is never asked and the query
        // fails to bind. This is the assertion that catches forgetting to list ipv6_lookup alongside ipv4_lookup.
        var context = new KustoQueryContext();
        context.SetTableLoader(new OnDemandRanges());
        var result = await context.RunQuery(
            "datatable(SourceIP:string)['2001:db8::1'] | evaluate ipv6_lookup(V6Ranges, SourceIP, Cidr)");
        result.Error.Should().BeEmpty();
        result.RowCount.Should().Be(1);
    }

    [TestMethod]
    public async Task Ipv6Lookup_ThenFilterAndSummarizeIntoSets()
    {
        // The shape a reputation rule actually forms: enrich by CIDR, filter on an appended column, then aggregate
        // both source and appended columns into sets per actor.
        var query =
            "let Ranges = datatable(Cidr:string, NetworkTrust:string)" +
            "['2001:db8::/32','tor', 'fd00::/8','vpn'];" +
            "datatable(Actor:string, IPAddress:string)" +
            "['a@x','2001:db8::1', 'a@x','fd00::5', 'b@x','2606:4700::1111'] " +
            "| evaluate ipv6_lookup(Ranges, IPAddress, Cidr) " +
            "| where NetworkTrust in ('tor','vpn') " +
            "| summarize Ips = make_set(IPAddress), Trusts = make_set(NetworkTrust) by Actor";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("tor").And.Contain("vpn");
    }
}
