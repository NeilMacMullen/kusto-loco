using FluentAssertions;
using Lokql.Engine.Commands;

namespace LokqlEngineTests;

[TestClass]
public class CommandProcessorTests
{
    [TestMethod]
    public void GetVerbs_SupportsFiles_OnlyContainsVerbsWithFileParameter()
    {
        var processor = CommandProcessor.Default();

        var result = processor.GetVerbs().Where(x => x.SupportsFiles);

        result
            .Should()
            .ContainSingle(x => x.Name.Equals("load", StringComparison.OrdinalIgnoreCase))
            .And.NotContain(x => x.Name.Equals("synonym", StringComparison.OrdinalIgnoreCase)); // SynTableCommand
    }
}
