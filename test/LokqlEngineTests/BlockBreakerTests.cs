using FluentAssertions;
using Lokql.Engine;

namespace LokqlEngineTests;

[TestClass]
public class BlockBreakerTests
{

    private string Cram(string input)
    {
        return input
            .Replace("\n", "")
            .Replace("\r","")
            .Replace(" ","");
    }
    private void CheckSimilar(string actual,string expected)
    {
        Cram(actual).Should().Be(Cram(expected));
    }

    private void CheckSimilar(string[] actual, string [] expected)
    {
        actual.Select(Cram).Should().BeEquivalentTo(expected.Select(Cram));
    }
    [TestMethod]
    public void EmptyStringReturnsEmpty()
    {
        var blockBreaker = new BlockBreaker("");
        blockBreaker.Blocks.Length.Should().Be(0);
    }

    [TestMethod]
    public void SingleLineReturnsLine()
    {
        var blockBreaker = new BlockBreaker("this is a test");
        CheckSimilar(blockBreaker.Blocks,["this is a test"]);
    }

    [TestMethod]
    public void SingleQueryIsAsExpected()
    {
        var query = @"table
| where column = 'value'";
        var blockBreaker = new BlockBreaker(query);
        CheckSimilar(blockBreaker.Blocks,[query]);
    }


    [TestMethod]
    public void BlockIsBrokenAtBlankLine()
    {
        var block1 = "table";
        var block2 = @"table 2
| where column = 'value'
";
        var query = $@"{block1}

{block2}";
        var blockBreaker = new BlockBreaker(query);
        CheckSimilar(blockBreaker.Blocks,[block1,block2]);
    }


    [TestMethod]
    public void BlockIsBrokenAtDotCommand()
    {
        var block1 = " .set abc 123";
        var block2 = @"table
| where a>5";
        var block3 = ".set xyz def";
        var query = $@"{block1}
{block2}
{block3}";
        var blockBreaker = new BlockBreaker(query);
        blockBreaker.Blocks.Length.Should().Be(3);
        CheckSimilar(blockBreaker.Blocks, [block1, block2,block3]);
    }

    [TestMethod]
    public void BlockIsBrokenAtComments()
    {
        var block1 = " # this is a query";
        var block2 = @"table
| where a>5";
        var block3 = " // this is also comment";
        var block4 = ".set xyz def";
        var query = $@"{block1}
{block2}
{block3}
{block4}";
        var blockBreaker = new BlockBreaker(query);
        blockBreaker.Blocks.Length.Should().Be(4);
        CheckSimilar(blockBreaker.Blocks, [block1, block2, block3,block4]);
    }

}
