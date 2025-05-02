using System.Linq;
using FluentAssertions;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;

public class NetViewShareRowTests
{
    [Fact]
    public void Parse_OneItem_GetsRowData()
    {
        // value is always lined up with row index except when blank?
        const string text = """
                            Shared resources at \\172.12.123.123
                            
                            User server (Samba, Ubuntu)
                            
                            Share name  Type  Used as  Comment
                            
                            -------------------------------------------------------------------------------
                            TestShare   Disk  Z:
                            The command completed successfully.
                            """;

        var expected = new[]
        {
            new NetViewShareRow
            {
                ShareName = "TestShare",
                Type = "Disk",
                UsedAs = "Z:",
            }
        };

        var result = NetViewShareRow.Parse(text);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Parse_IrregularSpace_GetsRowData()
    {
        const string text = """
                            Shared resources at localhost
                            
                            
                            
                            Share name                          Type  Used as  Comment
                            
                            -------------------------------------------------------------------------------
                            TmpShare                            Disk
                            TmpShare55                          Disk
                            TmpShare5528347812347   asdfh  asd  Disk
                            TmpShare5528347812347 asdfh asd     Disk
                            Users                               Disk
                            The command completed successfully.
                            
                            """;

        var expected = new[] { "TmpShare", "TmpShare55", "Users", "TmpShare5528347812347   asdfh  asd", "TmpShare5528347812347 asdfh asd" };

        var result = NetViewShareRow.Parse(text);

        result.Select(x => x.ShareName).Should().BeEquivalentTo(expected);
    }
}
