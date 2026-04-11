using Microsoft.Extensions.AI;
using NotNullStrings;

namespace KustoLoco.CopilotSupport;

public class ChatSession
{
    private readonly IChatClient _chatClient;

    public ChatSession(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }
    private readonly List<ChatMessage> _history = [];

    /// <summary>
    /// Gets the current message count in the conversation history
    /// </summary>
    public int MessageCount => _history.Count;

    /// <summary>
    /// Gets the approximate total character count of all messages in history
    /// </summary>
    public int TotalCharacterCount => _history.Sum(m => m.Text?.Length ?? 0);

    public static ChatSession Create(AISettings settings,string systemInstructions)
    {
        var chatClient = OrchestratorMethods.CreateAIChatClient(settings);
        var session = new ChatSession(chatClient);

        session.AddMessage(ChatRole.System, systemInstructions);
        return session;
    }
    public void AddMessage(ChatRole role, string message)
    {
        _history.Add(new ChatMessage(role, message));
    }

    public async Task<ModelResponse> SendUserRequest(string message)
    {
        AddMessage(ChatRole.User, message);
        var resp = await OrchestratorMethods.CallOpenAI(_chatClient, _history);
        if (resp.Error.IsBlank())
        {
            AddMessage(ChatRole.Assistant,resp.Response);

        }

        return resp;
    }


}
