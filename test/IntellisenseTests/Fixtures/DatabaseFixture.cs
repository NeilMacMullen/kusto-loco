using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Intellisense.Configuration;
using Intellisense.FileSystem;
using LogSetup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntellisenseTests.Fixtures;

internal class DatabaseFixture : IDisposable
{
    public IFileSystemIntellisenseService IntellisenseService { get; }
    public readonly IDirectoryInfo Directory = TestHelper.CreateCleanTestDirectory();

    public DatabaseFixture(Action<IServiceCollection>? configureServices = null)
    {


        var builder = Host.CreateApplicationBuilder();

        var services = builder.Services;

        services
            .AddDefault()
            .Configure<IntellisenseOptions>(options => options.Directory = Directory.FullName);

        builder.UseTestLogging();

        configureServices?.Invoke(services);
        Provider = services.BuildServiceProvider();

        IntellisenseService = Provider.GetRequiredService<IFileSystemIntellisenseService>();
    }

    public ServiceProvider Provider { get; }

    public void Dispose() => Provider.Dispose();

    public async ValueTask DisposeAsync() => await Provider.DisposeAsync();
}
