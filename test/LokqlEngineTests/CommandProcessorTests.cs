using AwesomeAssertions;
using AwesomeAssertions.Execution;
using Lokql.Engine.Commands;

namespace LokqlEngineTests;

[TestClass]
public class CommandProcessorTests
{
    [TestMethod]
    public void GetVerbsIdentifiesVerbsOfOptionsAcceptingFiles()
    {
        var processor = CommandProcessor.Default();

        var result = processor.GetVerbs().ToArray();

        using var _ = new AssertionScope();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("load", StringComparison.OrdinalIgnoreCase))
            .Which.SupportsFiles.Should()
            .BeTrue();

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("synonym", StringComparison.OrdinalIgnoreCase))
            .Which.SupportsFiles.Should()
            .BeFalse();
    }
}
