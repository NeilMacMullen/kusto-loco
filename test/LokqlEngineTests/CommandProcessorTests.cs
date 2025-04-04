using FluentAssertions;
using Lokql.Engine.Commands;

namespace LokqlEngineTests;

[TestClass]
public class CommandProcessorTests
{
    [TestMethod]
    public void GetVerbsOfOptionsThatTakeFilesAsInputDoesWhatItsNameDescribes()
    {
        var processor = CommandProcessor.Default();

        var result = processor.GetVerbsOfOptionsThatTakeFilesAsInput();

        result
            .Should()
            .ContainSingle(x => x.Equals("load", StringComparison.OrdinalIgnoreCase))
            .And.NotContain(x => x.Equals("synonym", StringComparison.OrdinalIgnoreCase)); // SynTableCommand
    }
}
