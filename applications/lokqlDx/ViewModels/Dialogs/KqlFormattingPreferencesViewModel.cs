using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kusto.Language.Editor;
using KustoLoco.Core;
using LokqlDx.Preferences;

namespace LokqlDx.ViewModels.Dialogs;

public partial class KqlFormattingPreferencesViewModel : ObservableObject, IDialogViewModel
{
    private const string SampleQuery = """
                                        StormEvents
                                        | where State == "TEXAS" and EventType == "Flood"
                                        | summarize TotalDamage = sum(DamageProperty), Count = count() by State, EventType
                                        | project State, EventType, Count, TotalDamage
                                        | order by TotalDamage desc
                                        | render barchart
                                        """;

    private readonly PreferencesManager _preferencesManager;
    private readonly TaskCompletionSource _completionSource;
    private bool _isInitialized;

    [ObservableProperty] private TextDocument _document = new();

    [ObservableProperty] private int _indentationSize;
    [ObservableProperty] private bool _insertMissingTokens;

    [ObservableProperty] private BrackettingStyle _brackettingStyle;
    [ObservableProperty] private BrackettingStyle _schemaStyle;
    [ObservableProperty] private BrackettingStyle _dataTableValueStyle;
    [ObservableProperty] private BrackettingStyle _functionBodyStyle;
    [ObservableProperty] private BrackettingStyle _functionParameterStyle;
    [ObservableProperty] private BrackettingStyle _functionArgumentStyle;

    [ObservableProperty] private PlacementStyle _pipeOperatorStyle;
    [ObservableProperty] private PlacementStyle _expressionStyle;
    [ObservableProperty] private PlacementStyle _statementStyle;
    [ObservableProperty] private PlacementStyle _semicolonStyle;

    [ObservableProperty] private SpacingStyle _generalSpacing;
    [ObservableProperty] private SpacingStyle _prefixOperatorSpacing;

    [ObservableProperty] private DualSpacingStyle _infixOperatorSpacing;
    [ObservableProperty] private DualSpacingStyle _pipeOperatorSpacing;
    [ObservableProperty] private DualSpacingStyle _commaSpacing;
    [ObservableProperty] private DualSpacingStyle _colonSpacing;
    [ObservableProperty] private DualSpacingStyle _assignmentSpacing;
    [ObservableProperty] private DualSpacingStyle _rangeOperatorSpacing;
    [ObservableProperty] private DualSpacingStyle _semicolonSpacing;

    [ObservableProperty] private SpacingStyle _parenthesizedExpressionSpacing;
    [ObservableProperty] private SpacingStyle _argumentListSpacing;
    [ObservableProperty] private SpacingStyle _emptyArgumentListSpacing;
    [ObservableProperty] private SpacingStyle _parameterListSpacing;
    [ObservableProperty] private SpacingStyle _emptyParameterListSpacing;
    [ObservableProperty] private SpacingStyle _jsonArraySpacing;
    [ObservableProperty] private SpacingStyle _emptyJsonArraySpacing;
    [ObservableProperty] private SpacingStyle _jsonObjectSpacing;
    [ObservableProperty] private SpacingStyle _emptyJsonObjectSpacing;
    [ObservableProperty] private SpacingStyle _dataTableValueSpacing;
    [ObservableProperty] private SpacingStyle _emptyDataTableValueSpacing;
    [ObservableProperty] private SpacingStyle _functionBodySpacing;
    [ObservableProperty] private SpacingStyle _emptyFunctionBodySpacing;
    [ObservableProperty] private SpacingStyle _beforeFunctionBodySpacing;
    [ObservableProperty] private SpacingStyle _beforeParameterListSpacing;
    [ObservableProperty] private SpacingStyle _beforeArgumentListSpacing;
    [ObservableProperty] private SpacingStyle _beforeDataTableValueSpacing;

    public KqlFormattingPreferencesViewModel(PreferencesManager preferencesManager)
    {
        _preferencesManager = preferencesManager;
        var applicationPreferences = preferencesManager.FetchApplicationPreferencesFromDisk();
        LoadFrom(applicationPreferences.Formatting);

        Document.Text = SampleQuery;

        _isInitialized = true;
        Reformat();

        _completionSource = new TaskCompletionSource();
        Result = _completionSource.Task;
    }

    public static BrackettingStyle[] BrackettingStyles { get; } = Enum.GetValues<BrackettingStyle>();
    public static PlacementStyle[] PlacementStyles { get; } = Enum.GetValues<PlacementStyle>();
    public static SpacingStyle[] SpacingStyles { get; } = Enum.GetValues<SpacingStyle>();
    public static DualSpacingStyle[] DualSpacingStyles { get; } = Enum.GetValues<DualSpacingStyle>();

    public Task Result { get; }

    private void LoadFrom(FormattingPreferences p)
    {
        IndentationSize = p.IndentationSize;
        InsertMissingTokens = p.InsertMissingTokens;
        BrackettingStyle = p.BrackettingStyle;
        SchemaStyle = p.SchemaStyle;
        DataTableValueStyle = p.DataTableValueStyle;
        FunctionBodyStyle = p.FunctionBodyStyle;
        FunctionParameterStyle = p.FunctionParameterStyle;
        FunctionArgumentStyle = p.FunctionArgumentStyle;
        PipeOperatorStyle = p.PipeOperatorStyle;
        ExpressionStyle = p.ExpressionStyle;
        StatementStyle = p.StatementStyle;
        SemicolonStyle = p.SemicolonStyle;
        GeneralSpacing = p.GeneralSpacing;
        PrefixOperatorSpacing = p.PrefixOperatorSpacing;
        InfixOperatorSpacing = p.InfixOperatorSpacing;
        PipeOperatorSpacing = p.PipeOperatorSpacing;
        CommaSpacing = p.CommaSpacing;
        ColonSpacing = p.ColonSpacing;
        AssignmentSpacing = p.AssignmentSpacing;
        RangeOperatorSpacing = p.RangeOperatorSpacing;
        SemicolonSpacing = p.SemicolonSpacing;
        ParenthesizedExpressionSpacing = p.ParenthesizedExpressionSpacing;
        ArgumentListSpacing = p.ArgumentListSpacing;
        EmptyArgumentListSpacing = p.EmptyArgumentListSpacing;
        ParameterListSpacing = p.ParameterListSpacing;
        EmptyParameterListSpacing = p.EmptyParameterListSpacing;
        JsonArraySpacing = p.JsonArraySpacing;
        EmptyJsonArraySpacing = p.EmptyJsonArraySpacing;
        JsonObjectSpacing = p.JsonObjectSpacing;
        EmptyJsonObjectSpacing = p.EmptyJsonObjectSpacing;
        DataTableValueSpacing = p.DataTableValueSpacing;
        EmptyDataTableValueSpacing = p.EmptyDataTableValueSpacing;
        FunctionBodySpacing = p.FunctionBodySpacing;
        EmptyFunctionBodySpacing = p.EmptyFunctionBodySpacing;
        BeforeFunctionBodySpacing = p.BeforeFunctionBodySpacing;
        BeforeParameterListSpacing = p.BeforeParameterListSpacing;
        BeforeArgumentListSpacing = p.BeforeArgumentListSpacing;
        BeforeDataTableValueSpacing = p.BeforeDataTableValueSpacing;
    }

    private FormattingPreferences ToPreferences() =>
        new()
        {
            IndentationSize = IndentationSize,
            InsertMissingTokens = InsertMissingTokens,
            BrackettingStyle = BrackettingStyle,
            SchemaStyle = SchemaStyle,
            DataTableValueStyle = DataTableValueStyle,
            FunctionBodyStyle = FunctionBodyStyle,
            FunctionParameterStyle = FunctionParameterStyle,
            FunctionArgumentStyle = FunctionArgumentStyle,
            PipeOperatorStyle = PipeOperatorStyle,
            ExpressionStyle = ExpressionStyle,
            StatementStyle = StatementStyle,
            SemicolonStyle = SemicolonStyle,
            GeneralSpacing = GeneralSpacing,
            PrefixOperatorSpacing = PrefixOperatorSpacing,
            InfixOperatorSpacing = InfixOperatorSpacing,
            PipeOperatorSpacing = PipeOperatorSpacing,
            CommaSpacing = CommaSpacing,
            ColonSpacing = ColonSpacing,
            AssignmentSpacing = AssignmentSpacing,
            RangeOperatorSpacing = RangeOperatorSpacing,
            SemicolonSpacing = SemicolonSpacing,
            ParenthesizedExpressionSpacing = ParenthesizedExpressionSpacing,
            ArgumentListSpacing = ArgumentListSpacing,
            EmptyArgumentListSpacing = EmptyArgumentListSpacing,
            ParameterListSpacing = ParameterListSpacing,
            EmptyParameterListSpacing = EmptyParameterListSpacing,
            JsonArraySpacing = JsonArraySpacing,
            EmptyJsonArraySpacing = EmptyJsonArraySpacing,
            JsonObjectSpacing = JsonObjectSpacing,
            EmptyJsonObjectSpacing = EmptyJsonObjectSpacing,
            DataTableValueSpacing = DataTableValueSpacing,
            EmptyDataTableValueSpacing = EmptyDataTableValueSpacing,
            FunctionBodySpacing = FunctionBodySpacing,
            EmptyFunctionBodySpacing = EmptyFunctionBodySpacing,
            BeforeFunctionBodySpacing = BeforeFunctionBodySpacing,
            BeforeParameterListSpacing = BeforeParameterListSpacing,
            BeforeArgumentListSpacing = BeforeArgumentListSpacing,
            BeforeDataTableValueSpacing = BeforeDataTableValueSpacing
        };

    /// <summary>
    ///     Re-runs the sample query through the formatter using the current settings, so the
    ///     user can see a live preview of the effect of the formatting options.
    /// </summary>
    private void Reformat()
    {
        if (!_isInitialized) return;
        try
        {
            var options = ToPreferences().ToFormattingOptions();
            var kustoCode = new KustoQueryContext().GetParseTree(Document.Text);
            var kustoCodeService = new KustoCodeService(kustoCode);
            Document.Text = kustoCodeService.GetFormattedText(options).Text;
        }
        catch
        {
            // ignore malformed sample text - just leave the editor content as-is
        }
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName is nameof(Document)) return;
        Reformat();
    }

    [RelayCommand]
    private void RestoreDefaults()
    {
        LoadFrom(FormattingPreferences.Default);
        Reformat();
    }

    [RelayCommand]
    private void Save()
    {
        var applicationPreferences = _preferencesManager.FetchCachedApplicationSettings();
        applicationPreferences.Formatting = ToPreferences();
        _preferencesManager.Save(applicationPreferences);
        _completionSource.SetResult();
    }

    [RelayCommand]
    private void Cancel() => _completionSource.SetResult();
}
