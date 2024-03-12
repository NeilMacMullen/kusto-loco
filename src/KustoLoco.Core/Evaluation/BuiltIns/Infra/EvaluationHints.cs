using System;

namespace KustoLoco.Core.Evaluation.BuiltIns;

[Flags]
public enum EvaluationHints
{
    None = 0,

    /// <summary>
    ///     Some functions like rand or row_number
    ///     appear to the parser to be scalars in that they don't
    ///     take any column value parameters.  But they _need_ to be evaluated
    ///     in columnar context because they are intended to generate a new
    ///     value for each row
    /// </summary>
    ForceColumnarEvaluation = 1 << 1,

    /// <summary>
    ///     Some functions like row_number require the table to be serialised since
    ///     otherwise they would operate incorrect for chunked data
    /// </summary>
    RequiresTableSerialization = 1 << 2
}