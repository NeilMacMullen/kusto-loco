using lokqlDxComponents.Models;
using lokqlDxComponents.Services;
using lokqlDxComponents.Views.Dialogs;

namespace lokqlDxComponents.Handlers;

public class IntellisenseKqlOperatorsHandler(IIntellisenseKqlOperatorsProvider provider) : IIntellisenseHandler
{
    public Task<CompletionRequest> GetCompletionRequest(HandleKeyDownMessage message)
    {
        if (message.Text != "|") return Task.FromResult(CompletionRequest.Empty);

        return Task.FromResult(new CompletionRequest
        {
            Completions = provider.GetKqlOperators().ToArray(),
            Prefix = " "
        });
    }
}
