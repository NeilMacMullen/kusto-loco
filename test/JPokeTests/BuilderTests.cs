using System.Text.Json;
using FluentAssertions;


namespace TestProject1;

[TestClass]
public class BuilderTests
{
    private void Check(JObjectBuilder b, object expected)
    {
        var e = JsonSerializer.Serialize(expected, new
            JsonSerializerOptions
            {
                WriteIndented = true
            });
        b.Serialize().Should().Be(e);
    }

    [TestMethod]
    public void SimpleElementIsParsed()
    {
        var b = JObjectBuilder.CreateEmpty();
        b.Set("abc.def", 123);
        var expected = new
        {
            abc = new
            {
                def = 123
            }
        };
        Check(b, expected);
    }

    [TestMethod]
    public void ElementCanBeAdded()
    {
        var b = JObjectBuilder.CreateEmpty();
        b.Set("abc.def", 123);
        b.Set("abc.xyz", 456);
        var expected = new
        {
            abc = new
            {
                def = 123,
                xyz = 456
            }
        };
        Check(b, expected);
    }

    [TestMethod]
    public void ArrayElementCanBeSet()
    {
        var b = JObjectBuilder.CreateEmpty();
        b.Set("abc.def[0]", 123);

        var expected = new
        {
            abc = new
            {
                def = new[] { 123 }
            }
        };
        Check(b, expected);
    }

    [TestMethod]
    public void ObjectCanBeSet()
    {
        var b = JObjectBuilder.CreateEmpty();
        b.Set("abc.def", new {xyz=5});

        var expected = new
        {
            abc = new
            {
                def = new { xyz=5 }
            }
        };
        Check(b, expected);
    }

    [TestMethod]
    public void ArrayCanBeAdded()
    {
        var b = JObjectBuilder.CreateEmpty();
        var arr = new string[] { "first", "second" };
        b.Set("abc.def", arr);

        var expected = new
        {
            abc = new
            {
                def = arr
            }
        };
        Check(b, expected);
    }


    [TestMethod]
    public void ArrayElementCanBeSetInPassing()
    {
        var b = JObjectBuilder.CreateEmpty();
        b.Set("abc.def[0].a", 123);
        var expected = new
        {
            abc = new
            {
                def = new[] { new
                {
                    a=123
                    
                } }
            }
        };
        Check(b, expected);
    }

    [TestMethod]
    public void ArrayElementCanBeSetInMiddle()
    {
        var b = JObjectBuilder.CreateEmpty();
        b.Set("abc.def[1]", 123);

        var expected = new
        {
            abc = new
            {
                def = new[] { 0,123 }
            }
        };
        Check(b, expected);
    }
}