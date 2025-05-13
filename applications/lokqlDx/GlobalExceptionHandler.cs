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
            logger.LogInformation(iExc, "Intellisense exception occurred.");
            return true;
        }

        logger.LogCritical(ex, "Unhandled exception occurred.");
        return false;
    }
}
