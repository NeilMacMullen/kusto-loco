using Intellisense;
using Microsoft.Extensions.Logging;

namespace lokqlDx;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
{
    public bool Handle(Exception ex)
    {
        if (ex is OperationCanceledException exc)
        {
            logger.LogDebug(exc, "Operation cancelled.");
            return true;
        }

        if (ex is IntellisenseException iExc)
        {
            logger.LogWarning(iExc, "Intellisense exception occurred.");
            return true;
        }

        logger.LogError(ex, "Unhandled exception occurred.");
        return true;
    }
}
