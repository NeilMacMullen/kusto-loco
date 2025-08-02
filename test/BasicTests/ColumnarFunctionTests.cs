using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class ColumnarFunctionTests : TestMethods
{
    [TestMethod]
    public async Task Columnar()
    {
        var query = """
                    range i from 0 to 10 step 1
                    | extend tid = partest(0)
                    | project tid 
                    | summarize diff = max(tid)-min(tid)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().NotBe("0");
    }
}
