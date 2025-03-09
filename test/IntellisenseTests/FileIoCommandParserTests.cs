using FluentAssertions;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;


public class FileIoCommandParserTests
{
    private readonly FileIoCommandParser _parser = new();

    [Fact]
    public void Parse_Quoted_ExtractsFileFromQuotes()
    {
        const string text = """
                            .load "/myFile.txt"
                            """;
        var command = _parser.Parse(text);
        command.Should().NotBeNull();
    }

    [InlineData(".load /myFile.txt", "/myFile.txt")]
    [InlineData(".save /myFil", "/myFil")]
    [InlineData(".load -f /myFile.txt", "/myFile.txt")]
    [InlineData(".save -abc /myFile", "/myFile")]
    [InlineData(".save C:/myFile", "C:/myFile")]
    [InlineData(".load E:/myF-ile.txt", "E:/myF-ile.txt")]
    [InlineData(".load /myF--ile.txt", "/myF--ile.txt")]
    [InlineData("""
                .load -f "/.load -f myFile.txt"
                """, "/.load -f myFile.txt")]
    [InlineData("""
                .load /asjdio /admsd q "asdj" asdq /abcd
                """, "/abcd")]
    [Theory]
    public void Parse_ValidCases_GetsFileName(string text, string expected)
    {
        var command = _parser.Parse(text);
        command.Should().Be(expected);
    }

    [InlineData(".loada /myFile.txt")]
    [InlineData(".load /myFile.txt -f")]
    [InlineData(".load /myFile.txt --f")]
    [InlineData(".Load /myFile.txt")]
    [Theory]
    public void Parse_InvalidCases_Null(string text)
    {
        var command = _parser.Parse(text);
        command.Should().BeNull();
    }
}
