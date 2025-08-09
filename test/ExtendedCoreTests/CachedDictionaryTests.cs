using AwesomeAssertions;
using KustoLoco.Core.Evaluation;

namespace ExtendedCoreTests;

[TestClass]
public class CachedDictionaryTests
{
    [TestMethod]
    public void SimpleTest()
    {
        var dict = new CachedDictionary<int, int>(1);
        for (var i = 0; i < 3; i++)
        {
            var ret = dict.TryGetValue(i, out var v);
            ret.Should().Be(false,$"because element ");
            dict.Add(i, i + 1);
        }

        for (var i = 0; i < 3; i++)
        {
            dict.TryGetValue(i, out var n)
                .Should().BeTrue();
            n.Should().Be(i  +1);
        }
    }
   

}
