using lokqlDxComponents.Models;
using lokqlDxComponents.Views.Dialogs;

namespace lokqlDxComponents.Handlers;

public interface IIntellisenseHandler
{
    Task<CompletionRequest> GetCompletionRequest(HandleKeyDownMessage message);
    int Priority => 1000;
}
