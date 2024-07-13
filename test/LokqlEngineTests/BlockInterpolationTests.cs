using FluentAssertions;
using KustoLoco.Core.Settings;
using Lokql.Engine;

namespace LokqlEngineTests;

[TestClass]
public class BlockInterpolationTests
{

   
    [TestMethod]
    public void CheckThatVariablesAreCorrectlyInterpolated()
    {
        var settings = new KustoSettingsProvider();
        settings.Set("abc", "123");
        settings.Set("def", "456");
        settings.Set("same", "same");
        var blockInterpolator = new BlockInterpolator(settings);
        blockInterpolator.Interpolate("xxx $abc yyy").Should().Be("xxx 123 yyy");
        blockInterpolator.Interpolate("xxx$(abc)yyy").Should().Be("xxx123yyy");

        blockInterpolator.Interpolate("xxx $same yyy").Should().Be("xxx same yyy");
        blockInterpolator.Interpolate("xxx $unrecognised yyy").Should().Be("xxx $unrecognised yyy");
        blockInterpolator.Interpolate("xxx $$abc yyy").Should().Be("xxx $abc yyy");
    }

    [TestMethod]
    public void CheckThatJoinExpressionsAreNotInterpolated()
    {
        var settings = new KustoSettingsProvider();
        var blockInterpolator = new BlockInterpolator(settings);
        var joinExpr = "xxx $left.Id ==$right.Id";
        blockInterpolator.Interpolate(joinExpr).Should().Be(joinExpr);

        var joinExprWithAdditionalDollars = joinExpr.Replace("$","$$");
        blockInterpolator.Interpolate(joinExprWithAdditionalDollars).Should().Be(joinExpr);

    }
    [TestMethod]
    public void CheckThatUnknownVariablesAreLeftUntouched()
    {
        var settings = new KustoSettingsProvider();
        var blockInterpolator = new BlockInterpolator(settings);
        var expr = "xxx $xyz  def";
        blockInterpolator.Interpolate(expr).Should().Be(expr);
    }

    [TestMethod]
    public void CheckThatClosingBracketsWithoutOpeningOnesAreLeft()
    {
        var settings = new KustoSettingsProvider();
        settings.Set("abc", "123");
        var blockInterpolator = new BlockInterpolator(settings);
        var expr = "xxx($abc)yyy";
        blockInterpolator.Interpolate(expr).Should().Be("xxx(123)yyy");
    }
}
