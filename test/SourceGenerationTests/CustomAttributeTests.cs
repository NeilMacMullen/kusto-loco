using FluentAssertions;
using SourceGeneration;

namespace SourceGenerationTests;

[TestClass]
public class CustomAttributeTests
{
    [TestMethod]
    public void NameOfAttributeIsCorrectlyDetermined()
    {
        CustomAttributeHelper<KustoImplementationAttribute>.Name()
            .Should()
            .Be("KustoImplementation");
    }
}