using AwesomeAssertions;
using KustoLoco.Core.DataSource.Columns;

namespace ExtendedCoreTests;

[TestClass]
public class NullableSetTests
{
    [TestMethod]
    public void TestNullIsReturned()
    {
        var data = new int?[] { 0, 1, null, 3 }.Cast<object?>().ToArray();
        var dataset = NullableSetOfint.FromObjectsOfCorrectType(data);

        dataset.NullableObject(0).Should().Be(0);
        dataset.NullableObject(1).Should().Be(1);
        dataset.NullableObject(2).Should().Be(null);

        dataset.NullableT(0).Should().Be(0);
        dataset.NullableT(1).Should().Be(1);
        dataset.NullableT(2).Should().Be(null);
    }


    [TestMethod]
    public void NullableStringNeverExposesNull()
    {
        var data = new string?[] { null}.Cast<object?>().ToArray();
        var dataset = NullableSetOfstring.FromObjectsOfCorrectType(data);
        dataset.NullableObject(0).Should().Be("");
        dataset.NullableT(0).Should().Be("");
        var a = (string?[])dataset.GetDataAsArray();
        a[0].Should().Be("");
    }
}
