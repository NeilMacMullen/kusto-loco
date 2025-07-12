using AwesomeAssertions;
using AwesomeAssertions.Execution;
using AwesomeAssertions.Primitives;
using NotNullStrings;

namespace lokqlDxComponentsTests;

public static class TestExtensions
{
    // it's really not worth making a FluentAssertion style extension with an unconstrained generic
    /// <summary>
    /// Continually retries the FluentAssertion delegate until either success or timeout. If this times out, this fails with the message of the most recently failed assertion.
    /// </summary>
    public static async Task ShouldEventuallySatisfy<T>(
        this T subject,
        Action<T> assertion,
        int timeoutMs = 5000
    )
    {

        var cts = new CancellationTokenSource(timeoutMs);
        string[] errors = [];

        while (!cts.Token.IsCancellationRequested)
        {
            using var scope = new AssertionScope();
            assertion(subject);
            if (!scope.HasFailures())
            {
                return;
            }
            errors = scope.Discard();
            await Task.Delay(100);
        }

        AssertionChain
            .GetOrCreate()
            .WithExpectation($"Expected to satisfy assertion within {timeoutMs} ms, but failed: {Environment.NewLine}", c => c.FailWith(errors.JoinString(Environment.NewLine)));
    }
}
