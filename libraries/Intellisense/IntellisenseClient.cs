using Intellisense.Concurrency;

namespace Intellisense;

public class IntellisenseClient(IIntellisenseService intellisenseService, ExclusiveRequestSession session)
{
    public async Task<CompletionResult> GetCompletionResultAsync(string input)
    {
        try
        {
            // handle synchronous blocking IO calls in win32 API and invalidate stale results
            return await session.RunAsync(token =>
                intellisenseService.GetCompletionResultAsync(input, token)
            );
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


    public async Task CancelRequestAsync()
    {
        try
        {
            await session.CancelRequestAsync();
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
