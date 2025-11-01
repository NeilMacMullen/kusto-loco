

using AwesomeAssertions;
using KustoLoco.SourceGeneration;

namespace SourceGenerationTests;

[TestClass]
public class CustomAttributeTests
{
    [TestMethod]
    public void NameOfAttributeIsCorrectlyDetermined()
    {
        CustomAttributeHelper<MyTestAttribute>.Name()
            .Should()
            .Be("MyTest");
    }
}


public class MyTestAttribute : Attribute
{

}
