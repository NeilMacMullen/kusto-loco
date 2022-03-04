// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    /// <summary>
    /// A scalar function in Kusto is defined as follows (<see href="https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/functions/user-defined-functions"/>):
    ///   - Has zero input arguments, or all its input arguments are scalar values
    ///   - Produces a single scalar value
    ///   - Can be used wherever a scalar expression is allowed
    ///   - May only use the row context in which it is defined
    ///   - Can only refer to tables (and views) that are in the accessible schema
    /// </summary>
    internal interface IScalarFunctionImpl
    {
        ScalarResult InvokeScalar(ScalarResult[] arguments);

        /// <summary>
        /// Executes the scalar function on columnar arguments.
        /// The results are the element-wise evaluation of the function for each row.
        /// </summary>
        ColumnarResult InvokeColumnar(ColumnarResult[] arguments);
    }
}
