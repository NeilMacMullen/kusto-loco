using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AwesomeAssertions;
using AwesomeAssertions.Extensions;
using Intellisense;
using Intellisense.FileSystem;
using Intellisense.FileSystem.Shares;
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
        var provider = new MockedIoTestContainer();

        provider.GetShareService = cts =>
        {
            var mock = new Mock<IShareService>();
            mock
                .Setup(x => x.GetSharesAsync(It.IsAny<string>()))
                .Returns(async (string delayMs) =>
                    {
                        var delay = int.Parse(delayMs);
                        await Task.Delay(delay.Milliseconds(), cts.Token);
                        return [delayMs];
                    }
                );
            return mock.Object;
        };

        var client = provider.GetRequiredService<IntellisenseClient>();

        var events = new ConcurrentQueue<string>();

        var t1 = PushEvent("//100/");
        var t2 = PushEvent("//200/");

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
        var container = new MockedIoTestContainer();
        container.GetFileSystemReader = () =>
        {
            var mock = new Mock<IFileSystemReader>();
            mock.Setup(x => x.GetChildren(It.IsAny<string>())).Throws<Exception>();
            return mock.Object;
        };

        var client = container.GetRequiredService<IntellisenseClient>();


        await client
            .Awaiting(x => x.GetCompletionResultAsync("/abcd"))
            .Should()
            .ThrowAsync<IntellisenseException>();
    }

    [Fact]
    public async Task GetCompletionResultsAsync_DoesNotWrapOperationCanceledExceptions()
    {
        var container = new MockedIoTestContainer();
        container.GetFileSystemReader = () =>
        {
            var mock = new Mock<IFileSystemReader>();
            mock.Setup(x => x.GetChildren(It.IsAny<string>())).Throws<OperationCanceledException>();
            return mock.Object;
        };

        var client = container.GetRequiredService<IntellisenseClient>();


        await client
            .Awaiting(x => x.GetCompletionResultAsync("/abcd"))
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }
}
