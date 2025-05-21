using System.Collections.Immutable;
using FluentAssertions;
using KustoLoco.Core.Settings;

namespace SettingsTests;

[TestClass]
public class KustoSettingsTester
{
    private readonly KustoSettingDefinition _boolDefaultFalse = new("setting", "description", "false", nameof(Boolean));
    private readonly KustoSettingDefinition _boolDefaultTrue = new("setting", "description", "true", nameof(Boolean));
    private readonly KustoSettingDefinition _pathList = new("setting", "description", "", nameof(String));

    [TestMethod]
    public void BoolCanBeRead()
    {
        var settings = new KustoSettingsProvider();

        // true/false
        settings.Set(_boolDefaultFalse.Name, "true");
        settings.GetBool(_boolDefaultFalse).Should().BeTrue();

        settings.Set(_boolDefaultTrue.Name, "false");
        settings.GetBool(_boolDefaultTrue).Should().BeFalse();

        //yes/no
        settings.Set(_boolDefaultFalse.Name, "yes");
        settings.GetBool(_boolDefaultFalse).Should().BeTrue();

        settings.Set(_boolDefaultTrue.Name, "no");
        settings.GetBool(_boolDefaultTrue).Should().BeFalse();


        //on/off
        settings.Set(_boolDefaultFalse.Name, "on");
        settings.GetBool(_boolDefaultFalse).Should().BeTrue();

        settings.Set(_boolDefaultTrue.Name, "off");
        settings.GetBool(_boolDefaultTrue).Should().BeFalse();

        // numeric
        settings.Set(_boolDefaultFalse.Name, "1");
        settings.GetBool(_boolDefaultFalse).Should().BeTrue();

        settings.Set(_boolDefaultTrue.Name, "0");
        settings.GetBool(_boolDefaultTrue).Should().BeFalse();
    }

    [TestMethod]
    public void PathCanBeRead()
    {
        var settings = new KustoSettingsProvider();

        settings.Set(_pathList.Name, @"c:\kusto ; c:\work\test");
        settings.GetPathList(_pathList).Should().BeEquivalentTo(@"c:\kusto", @"c:\work\test");
    }

    [TestMethod]
    public void SettingsCanBeRegisteredMultipleTimes()
    {
        var settings = new KustoSettingsProvider();
        settings.Register(_pathList, _pathList);
    }

    [TestMethod]
    public void SettingsCanBeLayered()
    {
        var settings = new KustoSettingsProvider();
        settings.Set("s1","1");
        settings.Set("s2", "2");
        var layered = new KustoSettingsProvider();
        layered.Set("s1","new");

        settings.AddLayer(layered);
        settings.GetOr("s1","").Should().Be("new");
        settings.GetOr("s2", "").Should().Be("2");

        settings.Pop();

        settings.GetOr("s1", "").Should().Be("1");
        settings.GetOr("s2", "").Should().Be("2");

        settings.Pop();
        settings.GetOr("s1", "").Should().Be("");
        settings.GetOr("s2", "").Should().Be("");

        settings.Pop();
        settings.GetOr("s1", "").Should().Be("");
        settings.GetOr("s2", "").Should().Be("");
    }

    [TestMethod]
    public void SettingsInsideMacrosDontGetLost()
    {
        var coreSettings = new KustoSettingsProvider();
        coreSettings.Set("s1","s1");
        coreSettings.Set("s2","s2");

        
        var newLayer = new KustoSettingsProvider();
        newLayer.Set("s1","new");
        newLayer.Set("s2","layered");
        coreSettings.AddLayer(newLayer);

        coreSettings.Set("s1", "frominside");
        coreSettings.Pop();

        coreSettings.GetOr("s1", "").Should().Be("frominside");
        coreSettings.GetOr("s2", "").Should().Be("s2");
    }
}
