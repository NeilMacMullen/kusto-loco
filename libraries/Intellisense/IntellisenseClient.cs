using Intellisense.Concurrency;

namespace Intellisense;

public class IntellisenseClient(IIntellisenseService intellisenseService, ExclusiveRequestSession session)
{
    public async Task<CompletionResult> GetCompletionResultAsync(string input) =>
        await ExceptionContext(() => session.RunAsync(token =>
                intellisenseService.GetCompletionResultAsync(input, token)
            )
        );

    public async Task CancelRequestAsync() => await ExceptionContext(session.CancelRequestAsync);

    private static async Task<T> ExceptionContext<T>(Func<Task<T>> func)
    {
        try
        {
            return await func();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new IntellisenseException(e);
        }
    }

    private static async Task ExceptionContext(Func<Task> func)
    {
        try
        {
            await func();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new IntellisenseException(e);
        }
    }
}
