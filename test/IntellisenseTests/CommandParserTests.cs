using AwesomeAssertions;
using Intellisense;
using Xunit;

namespace IntellisenseTests;

public class CommandParserTests
{
    [Fact]
    public void GetLastArgument_IgnoresCase()
    {
        var commands = new[] { "LOAD" };
        var parser = new CommandParser(commands, ".");

        var result = parser.GetLastArgument(".load /abcdefgh");

        result.Should().Be("/abcdefgh");
    }

    [Fact]
    public void GetLastArgument_HandlesQuotes()
    {
        var commands = new[] { "save" };
        var parser = new CommandParser(commands, ".");

        var result = parser.GetLastArgument(".save \"/abcdefgh\"");

        result.Should().Be("/abcdefgh");
    }

    [Fact]
    public void GetLastArgument_TextDoesntStartWithCommand_ReturnsEmpty()
    {
        var commands = new[] { "save" };
        var parser = new CommandParser(commands, ".");

        var result = parser.GetLastArgument("abc .save /abcde");

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetLastArgument_IgnoresSurroundingWhitespaceAndExcessWhitespace()
    {
        var commands = new[] { "save" };
        var parser = new CommandParser(commands, ".");

        var result = parser.GetLastArgument(" \r\n \t \n  .save \r\n \n \r\n \n  \t  /abcde   \r\n \n \t ");

        result.Should().Be("/abcde");
    }
}
