// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Expressions
{
    internal class BabyKustoDataTableExpression : BabyKustoExpression
    {
        public BabyKustoDataTableExpression(BabyKustoEngine engine, DataTableExpression expression)
            : base(engine, expression)
        {
            throw new NotImplementedException();
        }

        protected override object? EvaluateInternal(object? input)
        {
            throw new NotImplementedException();
        }
    }
}
