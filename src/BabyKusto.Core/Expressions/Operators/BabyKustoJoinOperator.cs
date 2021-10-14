// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions.Operators
{
    internal sealed class BabyKustoJoinOperator : BabyKustoOperator<JoinOperator>
    {
        public BabyKustoJoinOperator(BabyKustoEngine engine, JoinOperator projectOperator)
            : base(engine, projectOperator)
        {
        }

        protected override object? EvaluateTableInputInternal(ITableSource input)
        {
            // TODO: Implement join
            return new EmptyTableSource();
        }
    }
}
