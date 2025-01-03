using FluentAssertions;
using KustoLoco.Core;

namespace ExtendedCoreTests;

[TestClass]
public class EscapingTests
{
    [TestMethod]
    public void EscapingWorksAsExpected()
    {
        KustoNameEscaping.EscapeIfNecessary("abcd").Should().Be("abcd");
        KustoNameEscaping.EscapeIfNecessary("ab_de_xy").Should().Be("ab_de_xy");
        KustoNameEscaping.EscapeIfNecessary("a-b").Should().Be("['a-b']");
        KustoNameEscaping.EscapeIfNecessary("a b").Should().Be("['a b']");

        //pre-escaped

        KustoNameEscaping.EscapeIfNecessary("['abc']").Should().Be("abc");
        KustoNameEscaping.EscapeIfNecessary("['a b']").Should().Be("['a b']");
    }
}
