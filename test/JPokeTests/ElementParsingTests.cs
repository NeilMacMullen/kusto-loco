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

        [TestMethod]
        public void IndexedElementIsParsed()
        {
            var e = PathParser.ParseElement("abc[1]");
            e.Name.Should().Be("abc");
            e.IsIndex.Should().BeTrue();
            e.Index.Should().Be(1);
        }

        [TestMethod]
        public void EmptyIndexerReturnsIndexOfMinus1()
        {
            var e = PathParser.ParseElement("abc[]");
            e.Name.Should().Be("abc");
            e.IsIndex.Should().BeTrue();
            e.Index.Should().Be(-1);
        }
    }
}