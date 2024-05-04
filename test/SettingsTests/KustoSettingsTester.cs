using FluentAssertions;
using KustoLoco.FileFormats;

namespace SettingsTests
{
    [TestClass]
    public class KustoSettingsTester
    {
        [TestMethod]
        public void BoolCanBeRead()
        {
            var settings = new KustoSettings();

            // true/false
            settings.Set("bool", "true");
            settings.Get("bool", false).Should().BeTrue();

            settings.Set("bool", "false");
            settings.Get("bool", true).Should().BeFalse();

            //yes/no
            settings.Set("bool", "yes");
            settings.Get("bool", false).Should().BeTrue();

            settings.Set("bool", "no");
            settings.Get("bool", true).Should().BeFalse();

            // numeric
            settings.Set("bool", "1");
            settings.Get("bool", false).Should().BeTrue();

            settings.Set("bool", "0");
            settings.Get("bool", true).Should().BeFalse();


        }

        [TestMethod]
        public void PathCanBeRead()
        {
            var settings = new KustoSettings();
            settings.Set("path", @"c:\kusto ; c:\work\test");
            settings.GetPathList("path", []).Should().BeEquivalentTo(new[] { @"c:\kusto", @"c:\work\test" });
        }
    }
}
