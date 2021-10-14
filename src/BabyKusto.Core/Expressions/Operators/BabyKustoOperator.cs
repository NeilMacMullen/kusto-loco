// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions.Operators
{
    internal abstract class BabyKustoOperator<T> : BabyKustoExpression
        where T : QueryOperator
    {
        protected BabyKustoOperator(BabyKustoEngine engine, T @operator)
            : base(engine, @operator)
        {
        }
    }
}
