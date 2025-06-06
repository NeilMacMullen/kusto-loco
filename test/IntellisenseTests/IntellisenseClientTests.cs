using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AwesomeAssertions;
using Intellisense;
using IntellisenseTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntellisenseTests;

public class IntellisenseClientTests
{
    [Fact]
    public async Task GetCompletionResults_BeforeRequestFinishes_IgnoresPreviousPendingRequest()
    {
        // integration test
        var provider = new MockDelayTestContainer();

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
        var client = new MockExceptionContainer().GetRequiredService<IntellisenseClient>();


        await client
            .Awaiting(x => x.GetCompletionResultAsync("abcd"))
            .Should()
            .ThrowAsync<IntellisenseException>();
    }

    [Fact]
    public async Task GetCompletionResultsAsync_DoesNotWrapOperationCanceledExceptions()
    {

        var client = new MockExceptionContainer2().GetRequiredService<IntellisenseClient>();


        await client
            .Awaiting(x => x.GetCompletionResultAsync("abcd"))
            .Should()
            .ThrowAsync<OperationCanceledException>();
    }
}