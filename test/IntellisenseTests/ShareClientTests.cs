using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Intellisense;
using IntellisenseTests.Platforms;
using Xunit;

namespace IntellisenseTests;

public class ShareClientTests
{


    [WindowsOnlyFact]
    public void Run()
    {
        var res = ShareClient.GetNetworkShares();

        res.Should().BeEquivalentTo("a","b");
        // var shares = new ShareClient().GetShares().ToList();

        // shares.Should().BeEquivalentTo("a", "b");
    }
}
