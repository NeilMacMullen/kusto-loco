using CommunityToolkit.Mvvm.Messaging;
using Intellisense;
using lokqlDxComponents.Contexts;
using lokqlDxComponents.Events;
using lokqlDxComponents.Handlers;
using lokqlDxComponents.Models;
using lokqlDxComponents.Views.Dialogs;
using Microsoft.Extensions.Logging;
using CompletionRequest = lokqlDxComponents.Models.CompletionRequest;

namespace lokqlDxComponents.Services;

public class IntellisenseClientAdapter : IRecipient<HandleKeyDownMessage>, IRecipient<CaretPositionChangedMessage>
{
    private readonly IntellisenseClient _intellisenseClient;
    private readonly IEnumerable<IIntellisenseHandler> _handlers;
    private readonly IImageProvider _imageProvider;
    private readonly ILogger<IntellisenseClientAdapter> _logger;

    public IntellisenseClientAdapter(
        IntellisenseClient intellisenseClient,
        IQueryEditorContext queryEditorContext,
        IEnumerable<IIntellisenseHandler> handlers,
        IImageProvider imageProvider,
        ILogger<IntellisenseClientAdapter> logger
    )
    {
        _intellisenseClient = intellisenseClient;
        _handlers = handlers.OrderBy(x => x.Priority).ToList();
        _imageProvider = imageProvider;
        _logger = logger;
        queryEditorContext.Messenger.RegisterAll(this);
    }

    private async Task OnCaretPositionChanged() => await _intellisenseClient.CancelRequestAsync();

    private async Task<ShowCompletionOptions> HandleKeyDown(HandleKeyDownMessage message)
    {
        _logger.LogTrace("Received {HandleKeyDownMessage}", message);
        try
        {
            foreach (var handler in _handlers)
            {
                var result = await handler.GetCompletionRequest(message);
                if (result.IsEmpty)
                {
                    continue;
                }

                return CreateShowCompletionOptions(result);
            }

            return ShowCompletionOptions.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in intellisense");
            return ShowCompletionOptions.Empty;
        }
    }

    private ShowCompletionOptions CreateShowCompletionOptions(CompletionRequest completionRequest) => new()
    {
        Completions = completionRequest
            .Completions.OrderBy(x => x.Name)
            .Select(entry =>
                new QueryEditorCompletionData(entry, completionRequest.Prefix, completionRequest.Rewind)
                {
                    Image = _imageProvider.GetImage(entry.Hint)
                }
            )
            .ToList(),
        OnCompletionWindowDataPopulated = completionRequest.OnCompletionWindowDataPopulated
    };

    public void Receive(HandleKeyDownMessage message) => message.Reply(HandleKeyDown(message));

    public void Receive(CaretPositionChangedMessage message) => _ = OnCaretPositionChanged();
}
