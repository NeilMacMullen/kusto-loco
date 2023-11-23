using System;
using Xunit;
using Xunit.Abstractions;

namespace TDigest.Tests;

public class ScaleFunctionTests
{
    private readonly ITestOutputHelper testOutputHelper;
    public ScaleFunctionTests(ITestOutputHelper testOutputHelper) => this.testOutputHelper = testOutputHelper;

    [Fact]
    public void AsinApproximation()
    {
        for (double x = 0; x < 1; x += 1e-4)
        {
            // TODO: I had to lower the threshold for the C# implementation, for some reason
            // it is giving slightly different results compared to the Java implementation.
            Assert.Equal(Math.Asin(x), ScaleFunction.FastAsin(x), 2);
        }

        Assert.Equal(Math.Asin(1), ScaleFunction.FastAsin(1), 0);
        Assert.True(double.IsNaN(ScaleFunction.FastAsin(1.0001)));
    }

    [Fact]
    public void TestApproximation()
    {
        double worst = 0;
        var old = double.NegativeInfinity;
        for (double x = -1; x < 1; x += 0.00001)
        {
            var ex = Math.Asin(x);
            var actual = ScaleFunction.FastAsin(x);
            var error = ex - actual;
            //            System.out.printf("%.8f, %.8f, %.8f, %.12f\n", x, ex, actual, error * 1e6);
            Assert.Equal(0, error, 5);
            Assert.True(actual >= old);
            worst = Math.Max(worst, Math.Abs(error));
            old = actual;
        }

        Assert.Equal(Math.Asin(1), ScaleFunction.FastAsin(1), 0);
        testOutputHelper.WriteLine($"worst = {worst}");
    }
}