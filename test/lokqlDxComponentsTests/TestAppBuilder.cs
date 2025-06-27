using Avalonia;
using Avalonia.Headless;
using lokqlDxComponentsTests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]
namespace lokqlDxComponentsTests;



public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}