using FluentAssertions;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace SettingsTests
{
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
            settings.GetPathList(_pathList).Should().BeEquivalentTo(new[] { @"c:\kusto", @"c:\work\test" });
        }

        [TestMethod]
        public void SettingsCanBeRegisteredMultipleTimes()
        {
            var settings = new KustoSettingsProvider();
            settings.Register(_pathList,_pathList);

         }
    }
}
