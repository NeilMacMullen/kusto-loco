// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

//#define DEBUG_EXPRESSIONS

using System;
using BabyKusto.Core.Expressions.Operators;
using Kusto.Language.Syntax;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.Expressions
{
    internal abstract class BabyKustoExpression
    {
#if DEBUG_EXPRESSIONS
        static int DebugIndent = 0;
#endif

        protected bool _initialized = true;

        protected readonly BabyKustoEngine _engine;
        protected internal readonly Expression _expression;

        public BabyKustoExpression(BabyKustoEngine engine, Expression expression)
        {
            _engine = engine;
            _expression = expression;
        }

        /// <summary>
        /// Evaluates the expression.
        /// <paramref name="input"/> can be null, an <see cref="IRow"/>, or an <see cref="ITableSource"/>.
        /// </summary>
        public object? Evaluate(object? input)
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

#if DEBUG_EXPRESSIONS
            Console.WriteLine($"{new string(' ', DebugIndent)}Evaluating expression {TypeNameHelper.GetTypeDisplayName(this)} ({_expression.ToString(IncludeTrivia.SingleLine)})");
            DebugIndent++;
            try
            {
#endif
            return EvaluateInternal(input);
#if DEBUG_EXPRESSIONS
            }
            finally
            {
                DebugIndent--;
            }
#endif
        }

        protected virtual void Initialize()
        {
        }

        protected virtual object? EvaluateInternal(object? input)
        {
            return input switch
            {
                ITableSource table => EvaluateTableInputInternal(table),
                IRow row => EvaluateRowInputInternal(row),
                null => EvaluateNullInputInternal(),
                _ => throw new InvalidOperationException($"Unexpected input type to evaluate '{TypeNameHelper.GetTypeDisplayName(input)}'."),
            };
        }

        protected virtual object? EvaluateTableInputInternal(ITableSource table)
        {
            throw new NotImplementedException($"{nameof(EvaluateTableInputInternal)} not implemented for {TypeNameHelper.GetTypeDisplayName(this)}.");
        }

        protected virtual object? EvaluateRowInputInternal(IRow row)
        {
            throw new NotImplementedException($"{nameof(EvaluateRowInputInternal)} not implemented for {TypeNameHelper.GetTypeDisplayName(this)}.");
        }

        protected virtual object? EvaluateNullInputInternal()
        {
            throw new NotImplementedException($"{nameof(EvaluateNullInputInternal)} not implemented for {TypeNameHelper.GetTypeDisplayName(this)}.");
        }

        protected internal static BabyKustoExpression Build(BabyKustoEngine engine, Expression expression)
        {
            return expression.Kind switch
            {
                SyntaxKind.NameReference => new BabyKustoNameReferenceExpression(engine, (NameReference)expression),
                SyntaxKind.PipeExpression => new BabyKustoPipeExpression(engine, (PipeExpression)expression),
                SyntaxKind.SimpleNamedExpression => new BabyKustoSimpleNamedExpression(engine, (SimpleNamedExpression)expression),

                SyntaxKind.AddExpression or
                SyntaxKind.SubtractExpression or
                SyntaxKind.MultiplyExpression or
                SyntaxKind.DivideExpression or
                SyntaxKind.EqualExpression or
                SyntaxKind.NotEqualExpression or
                SyntaxKind.GreaterThanExpression or
                SyntaxKind.GreaterThanOrEqualExpression or
                SyntaxKind.LessThanExpression or
                SyntaxKind.LessThanOrEqualExpression or
                SyntaxKind.AndExpression or
                SyntaxKind.OrExpression => new BabyKustoBinaryExpression(engine, (BinaryExpression)expression),

                SyntaxKind.BooleanLiteralExpression or
                SyntaxKind.IntLiteralExpression or
                SyntaxKind.LongLiteralExpression or
                SyntaxKind.RealLiteralExpression or
                SyntaxKind.DecimalLiteralExpression or
                SyntaxKind.DateTimeLiteralExpression or
                SyntaxKind.TimespanLiteralExpression or
                SyntaxKind.GuidLiteralExpression or
                SyntaxKind.StringLiteralExpression or
                SyntaxKind.NullLiteralExpression => BabyKustoLiteralExpression.Build(engine, (LiteralExpression)expression),

                SyntaxKind.ParenthesizedExpression => new BabyKustoParenthesizedExpression(engine, (ParenthesizedExpression)expression),

                SyntaxKind.DataTableExpression => new BabyKustoDataTableExpression(engine, (DataTableExpression)expression),

                SyntaxKind.FunctionCallExpression => new BabyKustoFunctionCallExpression(engine, (FunctionCallExpression)expression),

                SyntaxKind.FilterOperator => new BabyKustoFilterOperator(engine, (FilterOperator)expression),
                SyntaxKind.SummarizeOperator => new BabyKustoSummarizeOperator(engine, (SummarizeOperator)expression),
                SyntaxKind.ProjectOperator => new BabyKustoProjectOperator(engine, (ProjectOperator)expression),
                SyntaxKind.JoinOperator => new BabyKustoJoinOperator(engine, (JoinOperator)expression),

                _ => throw new InvalidOperationException($"Unsupported expression kind '{expression.Kind}'."),
            };
        }
    }
}
