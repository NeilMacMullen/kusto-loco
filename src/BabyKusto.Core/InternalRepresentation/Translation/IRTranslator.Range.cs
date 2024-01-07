﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using BabyKusto.Core.Evaluation;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator
    {
        public override IRNode VisitRangeOperator(RangeOperator node)
        {
            var irFromExpression = (IRExpressionNode)node.From.Accept(this);
            var irToExpression = (IRExpressionNode)node.To.Accept(this);
            var irStepExpression = (IRExpressionNode)node.Step.Accept(this);
            var columnSymbol = node.Name.ReferencedSymbol ?? NullTypeSymbol.Instance;
            var resultType = node.ResultType;
            //for some reason the parser thinks datetime ranges have 
            //resultkind unknown

            if (irFromExpression.ResultType.Name == ScalarTypes.DateTime.Name)
            {
                var colName = ((TableSymbol)node.ResultType).Columns.First().Name;
                resultType = new TableSymbol("", new ColumnSymbol(colName, ScalarTypes.DateTime));
                // SetInScopeSymbolInfo(colName, EvaluatedExpressionKind.Columnar);
            }

            return new IRRangeOperatorNode(columnSymbol, irFromExpression, irToExpression, irStepExpression,
                resultType);
        }
    }
}