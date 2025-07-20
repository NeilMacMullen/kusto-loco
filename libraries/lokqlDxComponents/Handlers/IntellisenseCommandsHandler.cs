using lokqlDxComponents.Models;
using lokqlDxComponents.Services;
using lokqlDxComponents.Views.Dialogs;

namespace lokqlDxComponents.Handlers;

public class IntellisenseCommandsHandler(IIntellisenseCommandsProvider provider) : IIntellisenseHandler
{
    public Task<CompletionRequest> GetCompletionRequest(HandleKeyDownMessage message)
    {
        var cursor = message.Cursor;

        // only show completions if we are at the start of a line
        if (message.Text != "." || cursor.TextToLeftOfCaret().TrimStart() != ".")
            return Task.FromResult(CompletionRequest.Empty);

        return Task.FromResult(new CompletionRequest
        {
            Completions = provider.GetInternalCommands().ToArray()
        });
    }
}
