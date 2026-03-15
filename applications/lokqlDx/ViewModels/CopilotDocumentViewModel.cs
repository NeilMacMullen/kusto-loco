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
            DisplayStatus);

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
            _consoleViewModel.PrepareForOutput();
            await _explorer.RunInput(command);

            return new ActionExecutionResult
            {
                Success = true,
                Output = "Command executed successfully"
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

    private void DisplayStatus(string status)
    {
        Messages.Add(CopilotChatMessage.SystemMessage(status));
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

        Messages.Add(CopilotChatMessage.SystemMessage(
            "Chat cleared. Starting a new conversation."));

        CheckConfiguration();
    }

    [RelayCommand]
    private void ToggleResultsPane()
    {
        ShowResults = !ShowResults;
    }

    public void ApplyDisplayPreferences(DisplayPreferencesViewModel prefs)
    {
        FontSize = prefs.FontSize;
    }
}
