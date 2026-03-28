using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Mvvm.Controls;
using KustoLoco.CopilotSupport;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using NotNullStrings;

namespace LokqlDx.ViewModels;

/// <summary>
/// Represents a single message in the Copilot chat
/// </summary>
public class CopilotChatMessage
{
    public string Role { get; set; } = string.Empty; // "user", "assistant", "system", "error"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public bool IsQuery { get; set; }
    public string QueryText { get; set; } = string.Empty;
    public bool HasError { get; set; }

    public static CopilotChatMessage UserMessage(string content) =>
        new() { Role = "user", Content = content };

    public static CopilotChatMessage AssistantMessage(string content) =>
        new() { Role = "assistant", Content = content };

    public static CopilotChatMessage QueryMessage(string query) =>
        new() { Role = "assistant", Content = "Executing query...", IsQuery = true, QueryText = query };

    public static CopilotChatMessage ErrorMessage(string error) =>
        new() { Role = "error", Content = error, HasError = true };

    public static CopilotChatMessage SystemMessage(string content) =>
        new() { Role = "system", Content = content };
}

/// <summary>
/// ViewModel for the Copilot chat document
/// </summary>
public partial class CopilotDocumentViewModel : Document
{
    private readonly InteractiveTableExplorer _explorer;
    private readonly ConsoleViewModel _consoleViewModel;
    private readonly DisplayPreferencesViewModel _displayPreferences;
    private readonly KustoSettingsProvider _settings;

    private CopilotOrchestrator? _orchestrator;
    private bool _isInitialized;

    [ObservableProperty] 
    private ObservableCollection<CopilotChatMessage> _messages = [];

    [ObservableProperty] 
    private string _userInput = string.Empty;

    [ObservableProperty] 
    private bool _isProcessing;

    [ObservableProperty] 
    private bool _isConfigured;

    [ObservableProperty] 
    private string _configurationMessage = string.Empty;

    [ObservableProperty] 
    private RenderingSurfaceViewModel _renderingSurfaceViewModel = null!;

    [ObservableProperty] 
    private double _fontSize = 14;

    [ObservableProperty] 
    private bool _showResults = true;  // Start with results pane visible

    [ObservableProperty]
    private bool _debugMode = false;

    // Token usage tracking for the session
    [ObservableProperty]
    private int _sessionInputTokens;

    [ObservableProperty]
    private int _sessionOutputTokens;

    public int SessionTotalTokens => SessionInputTokens + SessionOutputTokens;

    /// <summary>
    /// Estimated cost in USD based on model pricing
    /// </summary>
    public string EstimatedCost => CalculateEstimatedCost();

    // Pricing per million tokens (as of 2025)
    private static readonly Dictionary<string, (decimal InputPer1M, decimal OutputPer1M)> ModelPricing = new(StringComparer.OrdinalIgnoreCase)
    {
        // Anthropic Claude models
        ["claude-sonnet-4-20250514"] = (3.00m, 15.00m),
        ["claude-3-5-sonnet-20241022"] = (3.00m, 15.00m),
        ["claude-3-5-sonnet"] = (3.00m, 15.00m),
        ["claude-3-opus"] = (15.00m, 75.00m),
        ["claude-3-haiku"] = (0.25m, 1.25m),

        // OpenAI GPT models
        ["gpt-4o"] = (2.50m, 10.00m),
        ["gpt-4o-mini"] = (0.15m, 0.60m),
        ["gpt-4-turbo"] = (10.00m, 30.00m),
        ["gpt-4"] = (30.00m, 60.00m),
        ["gpt-3.5-turbo"] = (0.50m, 1.50m),

        // Azure OpenAI (same as OpenAI)
        ["gpt-4o-2024-05-13"] = (2.50m, 10.00m),
    };

    private string CalculateEstimatedCost()
    {
        var model = _settings.GetOr("copilot.model", string.Empty);

        // Find matching pricing (try exact match first, then prefix match)
        (decimal inputRate, decimal outputRate) = (0m, 0m);

        if (ModelPricing.TryGetValue(model, out var exactMatch))
        {
            (inputRate, outputRate) = exactMatch;
        }
        else
        {
            // Try prefix matching for versioned model names
            foreach (var (key, value) in ModelPricing)
            {
                if (model.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    (inputRate, outputRate) = value;
                    break;
                }
            }
        }

        if (inputRate == 0 && outputRate == 0)
        {
            return $"{SessionTotalTokens:N0} tokens";
        }

        // Calculate cost: (tokens / 1,000,000) * rate
        var inputCost = (SessionInputTokens / 1_000_000m) * inputRate;
        var outputCost = (SessionOutputTokens / 1_000_000m) * outputRate;
        var totalCost = inputCost + outputCost;

        if (totalCost < 0.01m)
        {
            return $"{SessionTotalTokens:N0} tokens (~${totalCost:F4})";
        }
        return $"{SessionTotalTokens:N0} tokens (~${totalCost:F2})";
    }

    public CopilotDocumentViewModel(
        InteractiveTableExplorer explorer,
        ConsoleViewModel consoleViewModel,
        DisplayPreferencesViewModel displayPreferences,
        KustoSettingsProvider settings)
    {
        _explorer = explorer;
        _consoleViewModel = consoleViewModel;
        _displayPreferences = displayPreferences;
        _settings = settings;

        Title = "Copilot";

        // Create a rendering surface for showing query results
        RenderingSurfaceViewModel = new RenderingSurfaceViewModel(
            "Copilot Results",
            settings,
            displayPreferences,
            consoleViewModel);

        // Set default font size from display preferences
        FontSize = displayPreferences.FontSize;

        // Check initial configuration
        CheckConfiguration();

        // Add welcome message
        Messages.Add(CopilotChatMessage.SystemMessage(
            "Welcome to Copilot! I can help you explore your data using KQL queries. " +
            "Ask me questions about your data, and I'll generate and execute queries for you."));
    }

    private void CheckConfiguration()
    {
        var apiKey = _settings.GetOr("copilot.apikey", string.Empty);
        var model = _settings.GetOr("copilot.model", string.Empty);

        if (apiKey.IsBlank() || model.IsBlank())
        {
            IsConfigured = false;
            ConfigurationMessage = """
                Copilot is not configured. Please use .set to configure:

                For OpenAI:
                  .set copilot.apikey YOUR_OPENAI_API_KEY
                  .set copilot.model gpt-4o
                  .set copilot.type OpenAI

                For Anthropic/Claude:
                  .set copilot.apikey YOUR_ANTHROPIC_API_KEY
                  .set copilot.model claude-sonnet-4-20250514
                  .set copilot.type Anthropic

                For Azure OpenAI:
                  .set copilot.apikey YOUR_AZURE_API_KEY
                  .set copilot.model YOUR_DEPLOYMENT_NAME
                  .set copilot.type AzureOpenAI
                  .set copilot.endpoint https://YOUR_RESOURCE.openai.azure.com/
                """;
            Messages.Add(CopilotChatMessage.ErrorMessage(ConfigurationMessage));
        }
        else
        {
            IsConfigured = true;
            ConfigurationMessage = string.Empty;
        }
    }

    private void InitializeOrchestrator()
    {
        if (_isInitialized || !IsConfigured)
            return;

        var aiSettings = new AISettings
        {
            ApiKey = _settings.GetOr("copilot.apikey", string.Empty),
            AIModel = _settings.GetOr("copilot.model", string.Empty),
            AIType = _settings.GetOr("copilot.type", "OpenAI"),
            Endpoint = _settings.GetOr("copilot.endpoint", string.Empty)
        };

        _orchestrator = CopilotOrchestrator.Create(
            aiSettings,
            GetAvailableCommands,
            GetSchemaInfo,
            ExecuteKql,
            ExecuteCommand,
            DisplayExplanation,
            DisplayQueryResult,
            DisplayStatus,
            DisplayRawResponse);

        _isInitialized = true;
    }

    private string[] GetAvailableCommands()
    {
        var verbs = _explorer._commandProcessor.GetVerbs(_explorer._loader);
        return verbs.Select(v => $".{v.Name} - {v.HelpText}").ToArray();
    }

    private (string TableName, string ColumnName, string ColumnType)[] GetSchemaInfo()
    {
        var schema = _explorer.GetSchema();
        return schema
            .Where(s => s.Table.IsNotBlank())
            .Select(s => (s.Table, s.Column, s.Type))
            .ToArray();
    }

    private async Task<ActionExecutionResult> ExecuteKql(string query)
    {
        try
        {
            _consoleViewModel.PrepareForOutput();

            // Run the query through the explorer
            await _explorer.RunInput(query);

            // Get the last result
            var result = _explorer._resultHistory.MostRecent;

            if (result.Error.IsNotBlank())
            {
                return new ActionExecutionResult
                {
                    Success = false,
                    Error = result.Error
                };
            }

            // Format results for model feedback
            var resultText = FormatResultForModel(result);

            return new ActionExecutionResult
            {
                Success = true,
                Output = resultText
            };
        }
        catch (Exception ex)
        {
            return new ActionExecutionResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private async Task<ActionExecutionResult> ExecuteCommand(string command)
    {
        try
        {
            // Capture console output
            var previousCount = _consoleViewModel.ConsoleContent.Count;
            _consoleViewModel.PrepareForOutput();

            // Track the previous result to see if command generated new data
            var previousResult = _explorer._resultHistory.MostRecent;
            var previousRowCount = previousResult?.RowCount ?? 0;

            if (DebugMode)
            {
                Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Before command: MostRecent has {previousRowCount} rows"));
                Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Running command (length={command.Length}):\n{command}"));
            }

            await _explorer.RunInput(command);

            // Check if a new result was generated (like .appinsights does)
            var newResult = _explorer._resultHistory.MostRecent;
            var newRowCount = newResult?.RowCount ?? 0;
            var hasError = newResult?.Error.IsNotBlank() ?? false;

            // Check if there's an error in the result
            if (hasError && newResult != previousResult)
            {
                if (DebugMode)
                {
                    Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Query error: {newResult?.Error}"));
                }
                return new ActionExecutionResult
                {
                    Success = false,
                    Error = newResult!.Error
                };
            }

            // Compare by reference AND by checking if row counts changed (for cases where same object is mutated)
            var hasNewResult = (newResult != previousResult || newRowCount != previousRowCount) 
                               && newResult != null 
                               && newResult.Error.IsBlank();

            // Collect new console output
            var newOutput = _consoleViewModel.ConsoleContent
                .Skip(previousCount)
                .Select(ct => ct.Text)
                .ToList();

            // Check for error messages in console output (they're typically in red/error format)
            var errorLines = newOutput.Where(line => 
                line.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("failed", StringComparison.OrdinalIgnoreCase) ||
                line.Contains("exception", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Syntax error", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase)
            ).ToList();

            if (DebugMode)
            {
                Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] After command: MostRecent has {newRowCount} rows, hasNewResult={hasNewResult}, sameRef={ReferenceEquals(newResult, previousResult)}, hasError={hasError}"));
                Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Console lines captured: {newOutput.Count}"));
                if (newOutput.Any())
                {
                    Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Console output:\n{string.Join("\n", newOutput)}"));
                }
            }

            string output;
            if (hasNewResult && newResult!.RowCount > 0)
            {
                // Command generated query results - format them
                output = FormatResultForModel(newResult);

                if (DebugMode)
                {
                    Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Formatted result:\n{output}"));
                }

                // Also show in results pane
                ShowResults = true;
                _ = RenderingSurfaceViewModel.RenderToDisplay(newResult);
            }
            else if (newOutput.Any())
            {
                // Command produced console output
                output = string.Join(Environment.NewLine, newOutput);

                // If there are error lines, report as failure so model can retry
                if (errorLines.Any())
                {
                    return new ActionExecutionResult
                    {
                        Success = false,
                        Error = string.Join(Environment.NewLine, errorLines)
                    };
                }
            }
            else
            {
                output = "Command executed successfully";
            }

            return new ActionExecutionResult
            {
                Success = true,
                Output = output
            };
        }
        catch (Exception ex)
        {
            if (DebugMode)
            {
                Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Exception: {ex}"));
            }
            return new ActionExecutionResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    private void DisplayStatus(string status)
    {
        if (DebugMode)
        {
            Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] {status}"));
        }
    }

    private void DisplayRawResponse(string rawResponse)
    {
        if (DebugMode)
        {
            Messages.Add(CopilotChatMessage.SystemMessage($"[DEBUG] Model Response:\n{rawResponse}"));
        }
    }

    private void DisplayExplanation(string text)
    {
        Messages.Add(CopilotChatMessage.AssistantMessage(text));
    }

    private void DisplayQueryResult(string query, string output)
    {
        Messages.Add(CopilotChatMessage.QueryMessage(query));

        // Show the results in the rendering surface
        var result = _explorer._resultHistory.MostRecent;
        if (result.RowCount > 0)
        {
            ShowResults = true;
            _ = RenderingSurfaceViewModel.RenderToDisplay(result);
            Messages.Add(CopilotChatMessage.AssistantMessage($"Query returned {result.RowCount} rows."));
        }
    }

    private string FormatResultForModel(KustoQueryResult result)
    {
        if (result.RowCount == 0)
            return "No results returned.";

        // Limit to first 20 rows to avoid token overflow
        var maxRows = Math.Min(result.RowCount, 20);
        var lines = new List<string>();

        // Add header
        var columns = result.ColumnNames();
        lines.Add(string.Join(" | ", columns));
        lines.Add(new string('-', lines[0].Length));

        // Add data rows
        var columnDefs = result.ColumnDefinitions();
        for (var i = 0; i < maxRows; i++)
        {
            var rowValues = new List<string>();
            for (var colIndex = 0; colIndex < columnDefs.Length; colIndex++)
            {
                rowValues.Add(result.Get(colIndex, i)?.ToString() ?? "null");
            }
            lines.Add(string.Join(" | ", rowValues));
        }

        if (result.RowCount > maxRows)
        {
            lines.Add($"... and {result.RowCount - maxRows} more rows");
        }

        return string.Join(Environment.NewLine, lines);
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (UserInput.IsBlank() || IsProcessing)
            return;

        // Re-check configuration in case settings changed
        CheckConfiguration();
        if (!IsConfigured)
        {
            Messages.Add(CopilotChatMessage.ErrorMessage(ConfigurationMessage));
            return;
        }

        // Initialize orchestrator if needed
        InitializeOrchestrator();

        var userMessage = UserInput.Trim();
        UserInput = string.Empty;

        // Add user message to chat
        Messages.Add(CopilotChatMessage.UserMessage(userMessage));

        IsProcessing = true;
        try
        {
            var result = await _orchestrator!.ProcessUserMessage(userMessage);

            // Update session token counts
            SessionInputTokens += result.InputTokens;
            SessionOutputTokens += result.OutputTokens;
            OnPropertyChanged(nameof(SessionTotalTokens));
            OnPropertyChanged(nameof(EstimatedCost));

            // Show token usage in debug mode
            if (DebugMode && (result.InputTokens > 0 || result.OutputTokens > 0))
            {
                Messages.Add(CopilotChatMessage.SystemMessage(
                    $"[DEBUG] Tokens used: {result.InputTokens} in / {result.OutputTokens} out (Total this turn: {result.TotalTokens}, Session: {EstimatedCost})"));
            }

            if (result.HasError)
            {
                Messages.Add(CopilotChatMessage.ErrorMessage(result.Error));
            }
        }
        catch (Exception ex)
        {
            Messages.Add(CopilotChatMessage.ErrorMessage($"Error: {ex.Message}"));
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void ClearChat()
    {
        Messages.Clear();
        _isInitialized = false;
        _orchestrator = null;

        // Reset token counters
        SessionInputTokens = 0;
        SessionOutputTokens = 0;
        OnPropertyChanged(nameof(SessionTotalTokens));
        OnPropertyChanged(nameof(EstimatedCost));

        Messages.Add(CopilotChatMessage.SystemMessage(
            "Chat cleared. Starting a new conversation."));

        CheckConfiguration();
    }

    [RelayCommand]
    private void ToggleResultsPane()
    {
        ShowResults = !ShowResults;
    }

    [RelayCommand]
    private async Task CopyDebugToClipboard()
    {
        // Collect all debug messages from the chat
        var debugMessages = Messages
            .Where(m => m.Content.StartsWith("[DEBUG]") || m.Role == "system")
            .Select(m => $"[{m.Timestamp:HH:mm:ss}] {m.Content}")
            .ToList();

        if (!debugMessages.Any())
        {
            Messages.Add(CopilotChatMessage.SystemMessage("No debug messages to copy."));
            return;
        }

        var debugText = string.Join(Environment.NewLine + Environment.NewLine, debugMessages);

        try
        {
            if (OperatingSystem.IsWindows())
            {
                await Clowd.Clipboard.ClipboardAvalonia.SetTextAsync(debugText);
                Messages.Add(CopilotChatMessage.SystemMessage($"Copied {debugMessages.Count} debug messages to clipboard."));
            }
            else
            {
                Messages.Add(CopilotChatMessage.SystemMessage("Clipboard copy is only supported on Windows."));
            }
        }
        catch (Exception ex)
        {
            Messages.Add(CopilotChatMessage.ErrorMessage($"Failed to copy to clipboard: {ex.Message}"));
        }
    }

    public void ApplyDisplayPreferences(DisplayPreferencesViewModel prefs)
    {
        FontSize = prefs.FontSize;
    }
}
