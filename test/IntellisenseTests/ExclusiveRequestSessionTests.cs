using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;
using Intellisense.Concurrency;
using Xunit;

namespace IntellisenseTests;

[SuppressMessage("Reliability", "CA2016:Forward the \'CancellationToken\' parameter to methods")]
[SuppressMessage("ReSharper", "MethodSupportsCancellation")]
public class ExclusiveRequestSessionTests
{
    [Fact]
    public async Task RunAsync_BeforeRequestFinishes_IgnoresPreviousPendingRequest()
    {
        var session = new ExclusiveRequestSession();
        var events = new ConcurrentQueue<int>();

        var t1 = PushEvent(100);
        var t2 = PushEvent(200);

        await FluentActions
            .Awaiting(async () => await Task.WhenAll(t1, t2))
            .Should()
            .ThrowAsync<OperationCanceledException>();

        events.Should().ContainSingle().Which.Should().Be(200);

        return;

        async Task PushEvent(int delay)
        {
            var result = await session.RunAsync(async token =>
                {
                    await Task.Delay(delay.Milliseconds(), token);
                    return delay;
                }
            );
            events.Enqueue(result);
        }
    }

    [Fact]
    public async Task GetCompletionResults_AfterPreviousRequestFinishes_EmitsBothEvents()
    {
        var session = new ExclusiveRequestSession();
        var events = new ConcurrentQueue<int>();

        await PushEvent(100);
        await PushEvent(110);

        events.TryDequeue(out var s1);
        events.TryDequeue(out var s2);
        s1.Should().Be(100);
        s2.Should().Be(110);

        return;

        async Task PushEvent(int delay)
        {
            var result = await session.RunAsync(async token =>
                {
                    await Task.Delay(delay.Milliseconds(), token);
                    return delay;
                }
            );
            events.Enqueue(result);
        }
    }

    [Fact]
    public async Task CancelRequestAsync_CancelsRequest()
    {
        var session = new ExclusiveRequestSession();
        var events = new ConcurrentQueue<int>();

        _ = PushEvent(100);
        await session.CancelRequestAsync();
        await Task.Delay(150);

        events.Should().BeEmpty();

        return;

        async Task PushEvent(int delay)
        {
            var result = await session.RunAsync(async token =>
                {
                    await Task.Delay(delay.Milliseconds(), token);
                    return delay;
                }
            );
            events.Enqueue(result);
        }
    }

    [Fact]
    public async Task GetCompletionResults_CallbackDoesNotThrowOnCancellation_ThrowsAnyway()
    {
        var session = new ExclusiveRequestSession();
        var events = new ConcurrentQueue<int>();

        var task = PushEvent(100);
        await session.CancelRequestAsync();
        await Task.Delay(150);

        using var _ = new AssertionScope();

        events.Should().BeEmpty();
        await FluentActions
            .Awaiting(async () => await task)
            .Should()
            .ThrowAsync<OperationCanceledException>();

        return;

        async Task PushEvent(int delay)
        {
            var result = await session.RunAsync(async token =>
                {
                    await Task.Delay(delay.Milliseconds());
                    token.IsCancellationRequested.Should().BeTrue();
                    return delay;
                }
            );
            events.Enqueue(result);
        }
    }
}
