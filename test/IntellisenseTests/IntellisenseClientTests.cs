using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Intellisense;
using Intellisense.Concurrency;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace IntellisenseTests;

public class IntellisenseClientTests
{
    [Fact]
    public async Task GetCompletionResults_BeforeRequestFinishes_IgnoresPreviousPendingRequest()
    {
        // integration test
        var provider = new ServiceCollection()
            .AddIntellisenseWithMockedIo()
            .AddScoped<IFileSystemIntellisenseService, FakeFsIntellisense>()
            .BuildServiceProvider();

        var client = provider.GetRequiredService<IntellisenseClient>();

        var events = new ConcurrentQueue<string>();

        var t1 = PushEvent("100");
        var t2 = PushEvent("200");

        await FluentActions
            .Awaiting(async () => await Task.WhenAll(t1, t2))
            .Should()
            .ThrowAsync<OperationCanceledException>();

        events.Should().ContainSingle().Which.Should().Be("200");

        return;

        async Task PushEvent(string delay)
        {
            var result = await client.GetCompletionResultAsync(delay);
            events.Enqueue(result.Entries[0].Name);
        }
    }

    [Fact]
    public async Task GetCompletionResultsAsync_WrapsGeneralExceptions()
    {
        var mock = new Mock<IFileSystemIntellisenseService>();
        mock.Setup(x => x.GetPathIntellisenseOptionsAsync(It.IsAny<string>())).ThrowsAsync(new Exception());
        var client = new ServiceCollection()
            .AddIntellisenseWithMockedIo()
            .AddScoped<IFileSystemIntellisenseService>(_ => mock.Object)
            .BuildServiceProvider()
            .GetRequiredService<IntellisenseClient>();


        await client
            .Awaiting(x => x.GetCompletionResultAsync("abcd"))
            .Should()
            .ThrowAsync<IntellisenseException>();
    }

    [Fact]
    public async Task GetCompletionResultsAsync_DoesNotWrapOperationCanceledExceptions()
    {
        var mock = new Mock<IFileSystemIntellisenseService>();
        mock
            .Setup(x => x.GetPathIntellisenseOptionsAsync(It.IsAny<string>()))
            .ThrowsAsync(new OperationCanceledException());
        var client = new ServiceCollection()
            .AddIntellisenseWithMockedIo()
            .AddScoped<IFileSystemIntellisenseService>(_ => mock.Object)
            .BuildServiceProvider()
            .GetRequiredService<IntellisenseClient>();


        await client
            .Awaiting(x => x.GetCompletionResultAsync("abcd"))
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }
}

file class FakeFsIntellisense(CancellationContext context) : IFileSystemIntellisenseService
{
    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(string delayMs)
    {
        var delay = int.Parse(delayMs);
        await Task.Delay(delay.Milliseconds(), context.TokenSource.Token);

        return new CompletionResult()
        {
            Entries = [new IntellisenseEntry() { Name = delayMs }]
        };
    }
}
