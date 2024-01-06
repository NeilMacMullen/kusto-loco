using FluentAssertions;
using JPoke;

namespace TestProject1
{
    [TestClass]
    public class ElementParsingTests
    {
        [TestMethod]
        public void SimpleElementIsParsed()
        {
            var e = PathParser.ParseElement("abc");
            e.Name.Should().Be("abc");
            e.IsIndex.Should().BeFalse();
        }
    }
}