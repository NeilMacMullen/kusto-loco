using Kusto.Language.Editor;

namespace LokqlDx.Preferences;

/// <summary>
///     A serializable DTO mirroring the settings exposed by <see cref="FormattingOptions" />.
/// </summary>
/// <remarks>
///     <see cref="FormattingOptions" /> itself cannot be serialized directly (it has no public
///     constructor and is built via <see cref="FormattingOptions.Default" /> plus fluent
///     <c>With*</c> methods) so this class mirrors its properties and provides conversion
///     helpers to/from the real type.
/// </remarks>
public class FormattingPreferences
{
    public int IndentationSize { get; set; } = FormattingOptions.Default.IndentationSize;
    public bool InsertMissingTokens { get; set; } = FormattingOptions.Default.InsertMissingTokens;

    public BrackettingStyle BrackettingStyle { get; set; } = FormattingOptions.Default.BrackettingStyle;
    public BrackettingStyle SchemaStyle { get; set; } = FormattingOptions.Default.SchemaStyle;
    public BrackettingStyle DataTableValueStyle { get; set; } = FormattingOptions.Default.DataTableValueStyle;
    public BrackettingStyle FunctionBodyStyle { get; set; } = FormattingOptions.Default.FunctionBodyStyle;
    public BrackettingStyle FunctionParameterStyle { get; set; } = FormattingOptions.Default.FunctionParameterStyle;
    public BrackettingStyle FunctionArgumentStyle { get; set; } = FormattingOptions.Default.FunctionArgumentStyle;

    public PlacementStyle PipeOperatorStyle { get; set; } = FormattingOptions.Default.PipeOperatorStyle;
    public PlacementStyle ExpressionStyle { get; set; } = FormattingOptions.Default.ExpressionStyle;
    public PlacementStyle StatementStyle { get; set; } = FormattingOptions.Default.StatementStyle;
    public PlacementStyle SemicolonStyle { get; set; } = FormattingOptions.Default.SemicolonStyle;

    public SpacingStyle GeneralSpacing { get; set; } = FormattingOptions.Default.GeneralSpacing;
    public SpacingStyle PrefixOperatorSpacing { get; set; } = FormattingOptions.Default.PrefixOperatorSpacing;

    public DualSpacingStyle InfixOperatorSpacing { get; set; } = FormattingOptions.Default.InfixOperatorSpacing;
    public DualSpacingStyle PipeOperatorSpacing { get; set; } = FormattingOptions.Default.PipeOperatorSpacing;
    public DualSpacingStyle CommaSpacing { get; set; } = FormattingOptions.Default.CommaSpacing;
    public DualSpacingStyle ColonSpacing { get; set; } = FormattingOptions.Default.ColonSpacing;
    public DualSpacingStyle AssignmentSpacing { get; set; } = FormattingOptions.Default.AssignmentSpacing;
    public DualSpacingStyle RangeOperatorSpacing { get; set; } = FormattingOptions.Default.RangeOperatorSpacing;
    public DualSpacingStyle SemicolonSpacing { get; set; } = FormattingOptions.Default.SemicolonSpacing;

    public SpacingStyle ParenthesizedExpressionSpacing { get; set; } = FormattingOptions.Default.ParenthesizedExpressionSpacing;
    public SpacingStyle ArgumentListSpacing { get; set; } = FormattingOptions.Default.ArgumentListSpacing;
    public SpacingStyle EmptyArgumentListSpacing { get; set; } = FormattingOptions.Default.EmptyArgumentListSpacing;
    public SpacingStyle ParameterListSpacing { get; set; } = FormattingOptions.Default.ParameterListSpacing;
    public SpacingStyle EmptyParameterListSpacing { get; set; } = FormattingOptions.Default.EmptyParameterListSpacing;
    public SpacingStyle JsonArraySpacing { get; set; } = FormattingOptions.Default.JsonArraySpacing;
    public SpacingStyle EmptyJsonArraySpacing { get; set; } = FormattingOptions.Default.EmptyJsonArraySpacing;
    public SpacingStyle JsonObjectSpacing { get; set; } = FormattingOptions.Default.JsonObjectSpacing;
    public SpacingStyle EmptyJsonObjectSpacing { get; set; } = FormattingOptions.Default.EmptyJsonObjectSpacing;
    public SpacingStyle DataTableValueSpacing { get; set; } = FormattingOptions.Default.DataTableValueSpacing;
    public SpacingStyle EmptyDataTableValueSpacing { get; set; } = FormattingOptions.Default.EmptyDataTableValueSpacing;
    public SpacingStyle FunctionBodySpacing { get; set; } = FormattingOptions.Default.FunctionBodySpacing;
    public SpacingStyle EmptyFunctionBodySpacing { get; set; } = FormattingOptions.Default.EmptyFunctionBodySpacing;
    public SpacingStyle BeforeFunctionBodySpacing { get; set; } = FormattingOptions.Default.BeforeFunctionBodySpacing;
    public SpacingStyle BeforeParameterListSpacing { get; set; } = FormattingOptions.Default.BeforeParameterListSpacing;
    public SpacingStyle BeforeArgumentListSpacing { get; set; } = FormattingOptions.Default.BeforeArgumentListSpacing;
    public SpacingStyle BeforeDataTableValueSpacing { get; set; } = FormattingOptions.Default.BeforeDataTableValueSpacing;

    /// <summary>
    ///     A fresh copy of the settings matching <see cref="FormattingOptions.Default" />.
    /// </summary>
    public static FormattingPreferences Default => new();

    /// <summary>
    ///     Builds a real <see cref="FormattingOptions" /> instance from the current settings.
    /// </summary>
    public FormattingOptions ToFormattingOptions() =>
        FormattingOptions.Default
            .WithIndentationSize(IndentationSize)
            .WithInsertMissingTokens(InsertMissingTokens)
            .WithBrackettingStyle(BrackettingStyle)
            .WithSchemaStyle(SchemaStyle)
            .WithDataTableValueStyle(DataTableValueStyle)
            .WithFunctionBodyStyle(FunctionBodyStyle)
            .WithFunctionParameterStyle(FunctionParameterStyle)
            .WithFunctionArgumentStyle(FunctionArgumentStyle)
            .WithPipeOperatorStyle(PipeOperatorStyle)
            .WithExpressionStyle(ExpressionStyle)
            .WithStatementStyle(StatementStyle)
            .WithSemicolonStyle(SemicolonStyle)
            .WithGeneralSpacing(GeneralSpacing)
            .WithPrefixOperatorSpacing(PrefixOperatorSpacing)
            .WithInfixOperatorSpacing(InfixOperatorSpacing)
            .WithPipeOperatorSpacing(PipeOperatorSpacing)
            .WithCommaSpacing(CommaSpacing)
            .WithColonSpacing(ColonSpacing)
            .WithAssignmentSpacing(AssignmentSpacing)
            .WithRangeOperatorSpacing(RangeOperatorSpacing)
            .WithSemicolonSpacing(SemicolonSpacing)
            .WithExpressionParenSpacing(ParenthesizedExpressionSpacing)
            .WithArgumentListSpacing(ArgumentListSpacing)
            .WithEmptyArgumentListSpacing(EmptyArgumentListSpacing)
            .WithParameterListSpacing(ParameterListSpacing)
            .WithEmptyParameterListSpacing(EmptyParameterListSpacing)
            .WithJsonArraySpacing(JsonArraySpacing)
            .WithEmptyJsonArraySpacing(EmptyJsonArraySpacing)
            .WithJsonObjectSpacing(JsonObjectSpacing)
            .WithEmptyJsonObjectSpacing(EmptyJsonObjectSpacing)
            .WithDataTableValueSpacing(DataTableValueSpacing)
            .WithEmptyDataTableValueSpacing(EmptyDataTableValueSpacing)
            .WithFunctionBodySpacing(FunctionBodySpacing)
            .WithEmptyFunctionBodySpacing(EmptyFunctionBodySpacing)
            .WithBeforeFunctionBodySpacing(BeforeFunctionBodySpacing)
            .WithBeforeParameterListSpacing(BeforeParameterListSpacing)
            .WithBeforeArgumentListSpacing(BeforeArgumentListSpacing)
            .WithBeforeDataTableValueSpacing(BeforeDataTableValueSpacing);

    /// <summary>
    ///     Creates a settings DTO from a real <see cref="FormattingOptions" /> instance.
    /// </summary>
    public static FormattingPreferences FromFormattingOptions(FormattingOptions options) =>
        new()
        {
            IndentationSize = options.IndentationSize,
            InsertMissingTokens = options.InsertMissingTokens,
            BrackettingStyle = options.BrackettingStyle,
            SchemaStyle = options.SchemaStyle,
            DataTableValueStyle = options.DataTableValueStyle,
            FunctionBodyStyle = options.FunctionBodyStyle,
            FunctionParameterStyle = options.FunctionParameterStyle,
            FunctionArgumentStyle = options.FunctionArgumentStyle,
            PipeOperatorStyle = options.PipeOperatorStyle,
            ExpressionStyle = options.ExpressionStyle,
            StatementStyle = options.StatementStyle,
            SemicolonStyle = options.SemicolonStyle,
            GeneralSpacing = options.GeneralSpacing,
            PrefixOperatorSpacing = options.PrefixOperatorSpacing,
            InfixOperatorSpacing = options.InfixOperatorSpacing,
            PipeOperatorSpacing = options.PipeOperatorSpacing,
            CommaSpacing = options.CommaSpacing,
            ColonSpacing = options.ColonSpacing,
            AssignmentSpacing = options.AssignmentSpacing,
            RangeOperatorSpacing = options.RangeOperatorSpacing,
            SemicolonSpacing = options.SemicolonSpacing,
            ParenthesizedExpressionSpacing = options.ParenthesizedExpressionSpacing,
            ArgumentListSpacing = options.ArgumentListSpacing,
            EmptyArgumentListSpacing = options.EmptyArgumentListSpacing,
            ParameterListSpacing = options.ParameterListSpacing,
            EmptyParameterListSpacing = options.EmptyParameterListSpacing,
            JsonArraySpacing = options.JsonArraySpacing,
            EmptyJsonArraySpacing = options.EmptyJsonArraySpacing,
            JsonObjectSpacing = options.JsonObjectSpacing,
            EmptyJsonObjectSpacing = options.EmptyJsonObjectSpacing,
            DataTableValueSpacing = options.DataTableValueSpacing,
            EmptyDataTableValueSpacing = options.EmptyDataTableValueSpacing,
            FunctionBodySpacing = options.FunctionBodySpacing,
            EmptyFunctionBodySpacing = options.EmptyFunctionBodySpacing,
            BeforeFunctionBodySpacing = options.BeforeFunctionBodySpacing,
            BeforeParameterListSpacing = options.BeforeParameterListSpacing,
            BeforeArgumentListSpacing = options.BeforeArgumentListSpacing,
            BeforeDataTableValueSpacing = options.BeforeDataTableValueSpacing
        };
}
