// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitNameReference(IRNameReferenceNode node, EvaluationContext context)
    {
        var lookup = context.Scope.Lookup(node.ReferencedSymbol.Name);
        if (lookup == null)
            throw new InvalidOperationException($"Name {node.ReferencedSymbol.Name} is not in scope.");

        var (symbol, value) = lookup.Value;
        if (symbol != node.ReferencedSymbol)
        {
            //TODO - do we care ?
            //Console.WriteLine($"Name '{node.ReferencedSymbol.Name}' mismatched, but that's expected for now in function calls.");
        }

        return value;
    }

    public override EvaluationResult VisitRowScopeNameReferenceNode(IRRowScopeNameReferenceNode node,
        EvaluationContext context)
    {
        Debug.Assert(context.Chunk != TableChunk.Empty);
        var column = context.Chunk.Columns[node.ReferencedColumnIndex];
        return new ColumnarResult(column);
    }

    private static JsonNode? IndexIntoPossibleJsonArray(object? array, int index)
    {
        if (array is not JsonArray obj)
            return null;

        if (index < 0) index = obj.Count + index;
        if (index < 0) return null;
        return index >= obj.Count
            ? null
            : obj[index];
    }

    public override EvaluationResult VisitMemberAccess(IRArrayAccessNode node, EvaluationContext context)
    {
        if (node.ResultKind == EvaluatedExpressionKind.Columnar)
        {
            var items = (ColumnarResult?)node.Expression.Accept(this, context);
            if (items == null) throw new InvalidOperationException("Expression yielded null result");

            var itemsCol = (TypedBaseColumn<JsonNode?>)items.Column;

            var data = new JsonNode?[itemsCol.RowCount];
            for (var i = 0; i < items.Column.RowCount; i++)
                data[i] = IndexIntoPossibleJsonArray(itemsCol[i], node.Index);

            var column = new InMemoryColumn<JsonNode?>(data);
            return new ColumnarResult(column);
        }

        {
            var item = (ScalarResult?)node.Expression.Accept(this, context);
            if (item == null) throw new InvalidOperationException("Expression yielded null result");

            var value = IndexIntoPossibleJsonArray(item.Value, node.Index);
            return new ScalarResult(ScalarTypes.Dynamic, value);
        }
    }

    public override EvaluationResult VisitMemberAccess(IRMemberAccessNode node, EvaluationContext context)
    {
        if (node.ResultKind == EvaluatedExpressionKind.Columnar)
        {
            var items = (ColumnarResult?)node.Expression.Accept(this, context);
            if (items == null) throw new InvalidOperationException("Expression yielded null result");

            var itemsCol = (TypedBaseColumn<JsonNode?>)items.Column;

            var data = new JsonNode?[itemsCol.RowCount];
            for (var i = 0; i < items.Column.RowCount; i++)
                if (itemsCol[i] is JsonObject obj)
                    if (obj.TryGetPropertyValue(node.MemberName, out var value))
                        data[i] = value;

            var column = new InMemoryColumn<JsonNode?>(data);
            return new ColumnarResult(column);
        }

        {
            var item = (ScalarResult?)node.Expression.Accept(this, context);
            if (item == null) throw new InvalidOperationException("Expression yielded null result");

            if (item.Value is JsonObject obj)
                if (obj.TryGetPropertyValue(node.MemberName, out var value))
                    return new ScalarResult(ScalarTypes.Dynamic, value);

            return new ScalarResult(ScalarTypes.Dynamic, null);
        }
    }

    public override EvaluationResult
        VisitLiteralExpression(IRLiteralExpressionNode node, EvaluationContext context)
    {
        return new ScalarResult(node.ResultType, node.Value);
    }

    public override EvaluationResult VisitPipeExpression(IRPipeExpressionNode node, EvaluationContext context)
    {
        var left = (TabularResult?)node.Expression.Accept(this, context);
        if (left == null) throw new InvalidOperationException("Left expression produced null result");

        return node.Operator.Accept(this, context with { Left = left });
    }

    public override EvaluationResult VisitDataTableExpression(IRDataTableExpression node, EvaluationContext context)
    {
        var tableSymbol = (TableSymbol)node.ResultType;

        var numColumns = tableSymbol.Columns.Count;
        var numRows = node.Data.Length / numColumns;

        var columns = new BaseColumn[numColumns];
        for (var j = 0; j < numColumns; j++)
        {
            var columnData = new object?[numRows];
            for (var i = 0; i < numRows; i++) columnData[i] = node.Data[i * numColumns + j];

            columns[j] = ColumnHelpers.CreateFromObjectArray(columnData, tableSymbol.Columns[j].Type);
        }

        var result = new InMemoryTableSource(tableSymbol, columns);
        return TabularResult.CreateUnvisualized(result);
    }
}
