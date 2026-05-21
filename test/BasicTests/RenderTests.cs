using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class RenderTests:TestMethods
{
    [TestMethod]
    public async Task RenderWithEmptyWithStatement()
    {
        var query = """
                    range N from 1 to 10 step 1
                    | project-rename V=N
                    | extend R=1
                    | summarize Count=count() by R,V
                    | render columnchart with ()
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("1,10,1");
    }
}
