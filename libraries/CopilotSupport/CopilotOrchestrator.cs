using NotNullStrings;

namespace KustoLoco.CopilotSupport;

/// <summary>
/// Result of executing a Copilot action
/// </summary>
public class ActionExecutionResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
    public bool ShouldFeedbackToModel { get; set; }
}

/// <summary>
/// Represents a message in the chat conversation (for display purposes)
/// </summary>
public class CopilotChatHistoryItem
{
    public string Role { get; set; } = string.Empty; // "user", "assistant", "system"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

/// <summary>
/// Orchestrates the Copilot conversation and action execution
/// </summary>
public class CopilotOrchestrator
{
    private readonly ChatSession _session;
    private readonly Func<string, Task<ActionExecutionResult>> _executeKql;
    private readonly Func<string, Task<ActionExecutionResult>> _executeCommand;
    private readonly Action<string> _displayExplanation;
    private readonly Action<string, string> _displayQueryResult;
    private readonly Action<string>? _displayStatus;
    private readonly Action<string>? _displayRawResponse;

    private CopilotOrchestrator(
        ChatSession session,
        Func<string, Task<ActionExecutionResult>> executeKql,
        Func<string, Task<ActionExecutionResult>> executeCommand,
        Action<string> displayExplanation,
        Action<string, string> displayQueryResult,
        Action<string>? displayStatus = null,
        Action<string>? displayRawResponse = null)
    {
        _session = session;
        _executeKql = executeKql;
        _executeCommand = executeCommand;
        _displayExplanation = displayExplanation;
        _displayQueryResult = displayQueryResult;
        _displayStatus = displayStatus;
        _displayRawResponse = displayRawResponse;
    }

    /// <summary>
    /// Creates a new CopilotOrchestrator with the specified configuration
    /// </summary>
    public static CopilotOrchestrator Create(
        AISettings settings,
        Func<string[]> getAvailableCommands,
        Func<(string TableName, string ColumnName, string ColumnType)[]> getSchemaInfo,
        Func<string, Task<ActionExecutionResult>> executeKql,
        Func<string, Task<ActionExecutionResult>> executeCommand,
        Action<string> displayExplanation,
        Action<string, string> displayQueryResult,
        Action<string>? displayStatus = null,
        Action<string>? displayRawResponse = null)
    {
        var systemPrompt = CopilotPromptBuilder.CreateSystemPrompt(getAvailableCommands, getSchemaInfo);
        var session = ChatSession.Create(settings, systemPrompt);

        return new CopilotOrchestrator(session, executeKql, executeCommand, displayExplanation, displayQueryResult, displayStatus, displayRawResponse);
    }

    /// <summary>
    /// Processes a user message and executes resulting actions
    /// </summary>
    public async Task<ConversationResult> ProcessUserMessage(string userMessage)
    {
        var result = new ConversationResult();

        // Send to model
        var modelResponse = await _session.SendUserRequest(userMessage);

        // Track token usage
        result.InputTokens += modelResponse.InputTokens;
        result.OutputTokens += modelResponse.OutputTokens;

        if (modelResponse.Error.IsNotBlank())
        {
            result.Error = modelResponse.Error;
            return result;
        }

        // Show raw response in debug mode
        _displayRawResponse?.Invoke(modelResponse.Response);

        // Parse the response
        var parseResult = CopilotResponseParser.Parse(modelResponse.Response);
        if (!parseResult.IsSuccess)
        {
            result.Error = parseResult.ErrorMessage;
            return result;
        }

        // Execute actions
        await ExecuteActions(parseResult.Response.Actions, result);

        return result;
    }

    private async Task ExecuteActions(List<CopilotAction> actions, ConversationResult result)
    {
        foreach (var action in actions)
        {
            var actionResult = await ExecuteAction(action);
            result.ExecutedActions.Add(new ExecutedAction
            {
                Action = action,
                Result = actionResult
            });

            // If this was an inspect action, feed results back to model
            if (action.Type == "inspect" && actionResult.ShouldFeedbackToModel)
            {
                var feedbackMessage = CopilotPromptBuilder.CreateInspectResultMessage(
                    action.Query,
                    actionResult.Success ? actionResult.Output : actionResult.Error,
                    !actionResult.Success);

                _displayStatus?.Invoke($"Feeding inspect results back to model...");

                // Send feedback and process any follow-up actions
                var followUpResponse = await _session.SendUserRequest(feedbackMessage);
                result.InputTokens += followUpResponse.InputTokens;
                result.OutputTokens += followUpResponse.OutputTokens;

                if (followUpResponse.Error.IsBlank())
                {
                    _displayRawResponse?.Invoke(followUpResponse.Response);
                    var followUpParse = CopilotResponseParser.Parse(followUpResponse.Response);
                    if (followUpParse.IsSuccess)
                    {
                        await ExecuteActions(followUpParse.Response.Actions, result);
                    }
                }
            }

            // If this was a command action, feed results back to model
            if (action.Type == "command" && actionResult.ShouldFeedbackToModel)
            {
                var feedbackMessage = CopilotPromptBuilder.CreateCommandResultMessage(
                    action.Command,
                    actionResult.Success ? actionResult.Output : actionResult.Error,
                    !actionResult.Success);

                _displayStatus?.Invoke($"Feeding command results back to model...");

                // Send feedback and process any follow-up actions
                var followUpResponse = await _session.SendUserRequest(feedbackMessage);
                result.InputTokens += followUpResponse.InputTokens;
                result.OutputTokens += followUpResponse.OutputTokens;

                if (followUpResponse.Error.IsBlank())
                {
                    _displayRawResponse?.Invoke(followUpResponse.Response);
                    var followUpParse = CopilotResponseParser.Parse(followUpResponse.Response);
                    if (followUpParse.IsSuccess)
                    {
                        await ExecuteActions(followUpParse.Response.Actions, result);
                    }
                }
            }

            // If a KQL query failed, inform the model so it can retry
            if (action.Type == "kql" && !actionResult.Success)
            {
                var errorMessage = CopilotPromptBuilder.CreateQueryErrorMessage(
                    action.Query,
                    actionResult.Error);

                _displayStatus?.Invoke($"Informing model about query error...");

                var retryResponse = await _session.SendUserRequest(errorMessage);
                result.InputTokens += retryResponse.InputTokens;
                result.OutputTokens += retryResponse.OutputTokens;

                if (retryResponse.Error.IsBlank())
                {
                    _displayRawResponse?.Invoke(retryResponse.Response);
                    var retryParse = CopilotResponseParser.Parse(retryResponse.Response);
                    if (retryParse.IsSuccess)
                    {
                        // Only retry once to avoid infinite loops
                        foreach (var retryAction in retryParse.Response.Actions)
                        {
                            var retryResult = await ExecuteAction(retryAction);
                            result.ExecutedActions.Add(new ExecutedAction
                            {
                                Action = retryAction,
                                Result = retryResult,
                                IsRetry = true
                            });
                        }
                    }
                }
            }
        }
    }

    private async Task<ActionExecutionResult> ExecuteAction(CopilotAction action)
    {
        switch (action.Type.ToLowerInvariant())
        {
            case "explanation":
                _displayExplanation(action.Text);
                return new ActionExecutionResult { Success = true, Output = action.Text };

            case "kql":
                var kqlResult = await _executeKql(action.Query);
                if (kqlResult.Success)
                {
                    _displayQueryResult(action.Query, kqlResult.Output);
                }
                return kqlResult;

            case "command":
                _displayStatus?.Invoke($"Executing command: {action.Command}");
                var cmdResult = await _executeCommand(action.Command);

                // Show command results in debug mode
                if (cmdResult.Success && !string.IsNullOrEmpty(cmdResult.Output))
                {
                    _displayStatus?.Invoke($"Command output:\n{cmdResult.Output}");
                }
                else if (!cmdResult.Success)
                {
                    _displayStatus?.Invoke($"Command failed: {cmdResult.Error}");
                }

                // Feed command results back to model so it can see the output
                cmdResult.ShouldFeedbackToModel = true;
                return cmdResult;

            case "inspect":
                _displayStatus?.Invoke($"Inspecting: {action.Query}");

                // Inspect can handle both KQL queries and commands (starting with .)
                ActionExecutionResult inspectResult;
                if (action.Query.TrimStart().StartsWith("."))
                {
                    // It's a command
                    inspectResult = await _executeCommand(action.Query);
                }
                else
                {
                    // It's a KQL query
                    inspectResult = await _executeKql(action.Query);
                }

                inspectResult.ShouldFeedbackToModel = true;

                // Show a brief status about what was found
                if (inspectResult.Success && !string.IsNullOrEmpty(inspectResult.Output))
                {
                    var lineCount = inspectResult.Output.Split('\n').Length;
                    _displayStatus?.Invoke($"Inspect found {lineCount} lines of data");
                }
                else if (!inspectResult.Success)
                {
                    _displayStatus?.Invoke($"Inspect failed: {inspectResult.Error}");
                }

                return inspectResult;

            default:
                return new ActionExecutionResult
                {
                    Success = false,
                    Error = $"Unknown action type: {action.Type}"
                };
        }
    }
}

/// <summary>
/// Result of a conversation turn
/// </summary>
public class ConversationResult
{
    public string Error { get; set; } = string.Empty;
    public List<ExecutedAction> ExecutedActions { get; } = [];
    public bool HasError => !string.IsNullOrEmpty(Error);

    // Token usage tracking
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens => InputTokens + OutputTokens;
}

/// <summary>
/// Record of an executed action and its result
/// </summary>
public class ExecutedAction
{
    public CopilotAction Action { get; set; } = new();
    public ActionExecutionResult Result { get; set; } = new();
    public bool IsRetry { get; set; }
}
