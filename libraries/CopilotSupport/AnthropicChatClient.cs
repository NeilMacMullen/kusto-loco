using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.AI;
using AITextContent = Microsoft.Extensions.AI.TextContent;

namespace KustoLoco.CopilotSupport;

/// <summary>
/// Wrapper around Anthropic SDK that implements the Microsoft.Extensions.AI IChatClient interface
/// </summary>
public class AnthropicChatClient : IChatClient
{
    private readonly AnthropicClient _client;
    private readonly string _model;

    public AnthropicChatClient(string apiKey, string model)
    {
        _client = new AnthropicClient(apiKey);
        _model = model;
    }

    public ChatClientMetadata Metadata => new("Anthropic", new Uri("https://api.anthropic.com"), _model);

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var messageList = messages.ToList();

        // Extract system message if present
        string? systemPrompt = null;
        var conversationMessages = new List<Message>();

        foreach (var msg in messageList)
        {
            if (msg.Role == ChatRole.System)
            {
                systemPrompt = GetTextContent(msg);
            }
            else
            {
                var role = msg.Role == ChatRole.User ? RoleType.User : RoleType.Assistant;
                var content = GetTextContent(msg);
                if (!string.IsNullOrEmpty(content))
                {
                    conversationMessages.Add(new Message(role, content));
                }
            }
        }

        // Build the request
        var parameters = new MessageParameters
        {
            Model = _model,
            MaxTokens = options?.MaxOutputTokens ?? 4096,
            Messages = conversationMessages,
            SystemMessage = systemPrompt
        };

        // Call Anthropic API
        var response = await _client.Messages.GetClaudeMessageAsync(parameters);

        // Convert response to ChatMessage format
        var responseText = string.Join("", response.Content
            .OfType<Anthropic.SDK.Messaging.TextContent>()
            .Select(c => c.Text ?? string.Empty));

        var responseMessage = new ChatMessage(ChatRole.Assistant, responseText);

        return new ChatResponse([responseMessage])
        {
            ModelId = _model,
            FinishReason = response.StopReason switch
            {
                "end_turn" => ChatFinishReason.Stop,
                "max_tokens" => ChatFinishReason.Length,
                "stop_sequence" => ChatFinishReason.Stop,
                _ => null
            },
            Usage = new UsageDetails
            {
                InputTokenCount = response.Usage?.InputTokens,
                OutputTokenCount = response.Usage?.OutputTokens
            }
        };
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // For now, just wrap the non-streaming response
        // A full implementation would use the streaming API
        throw new NotSupportedException("Streaming is not currently supported for Anthropic client. Use GetResponseAsync instead.");
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(AnthropicClient))
            return _client;
        return null;
    }

    public void Dispose()
    {
        // AnthropicClient doesn't require disposal
    }

    private static string GetTextContent(ChatMessage message)
    {
        if (message.Text != null)
            return message.Text;

        // Handle content items if Text is null
        if (message.Contents != null)
        {
            return string.Join("", message.Contents
                .OfType<AITextContent>()
                .Select(tc => tc.Text ?? string.Empty));
        }

        return string.Empty;
    }
}
