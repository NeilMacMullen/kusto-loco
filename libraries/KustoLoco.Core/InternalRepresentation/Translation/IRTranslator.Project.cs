﻿//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using Kusto.Language.Utils;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator
{
    public override IRNode VisitProjectOperator(ProjectOperator node)
    {
        var columns = new List<IROutputColumnNode>();
        for (var i = 0; i < node.Expressions.Count; i++)
        {
            var symbol = (ColumnSymbol)node.ResultType.Members[i];
            var expression = node.Expressions[i].Element;
            var irExpression = (IRExpressionNode)expression.Accept(this);
            columns.Add(new IROutputColumnNode(symbol, irExpression));
        }

        return new IRProjectOperatorNode(IRListNode.From(columns), node.ResultType, false);
    }

    public override IRNode VisitProjectRenameOperator(ProjectRenameOperator node)
    {
        MyDebug.Assert(_rowScope != TableSymbol.Empty);
        var resultTypeMember = node.ResultType.Members;
        //TODO HERE - we should not be keying on ColumnSymbol, but on the original column name/type
        var renamedColumns = new Dictionary<ColumnSymbol, ColumnSymbol>();
        foreach (var element in node.Expressions)
        {
            var expression = (SimpleNamedExpression)element.Element;

            var newSymbol = (ColumnSymbol)expression.Name.ReferencedSymbol;
            var originalSymbol = (ColumnSymbol)((NameReference)expression.Expression).ReferencedSymbol;
            if (newSymbol == null || originalSymbol == null)
            {
                throw new InvalidOperationException("New or original symbols are null.");
            }

            renamedColumns.Add(newSymbol, originalSymbol);
        }

        var columns = new List<IROutputColumnNode>();
        foreach (var t in resultTypeMember)
        {
            var symbol = (ColumnSymbol)t;
            renamedColumns.TryGetValue(symbol, out var originalSymbol);
            var referencedSymbol = originalSymbol ?? symbol;
            var referencedColumnIndex = _rowScope.Members.IndexOf(referencedSymbol);
            columns.Add(new IROutputColumnNode(symbol,
                new IRRowScopeNameReferenceNode(referencedSymbol, symbol.Type, referencedColumnIndex)));
        }

        return new IRProjectOperatorNode(IRListNode.From(columns), node.ResultType,false);
    }

    public override IRNode VisitProjectReorderOperator(ProjectReorderOperator node) =>
        EasyProjectImpl(node.ResultType);

    public override IRNode VisitProjectAwayOperator(ProjectAwayOperator node) => EasyProjectImpl(node.ResultType);

    public override IRNode VisitProjectKeepOperator(ProjectKeepOperator node) => EasyProjectImpl(node.ResultType);

    /// <summary>
    ///     Takes care of <c>project-reorder</c>, <c>project-away</c>, <c>project-keep</c>
    ///     which are all trivial to implement because the Kusto library has already done the heavy lifting and computed all
    ///     the output columns.
    /// </summary>
    private IRNode EasyProjectImpl(TypeSymbol resultType)
    {
        MyDebug.Assert(_rowScope != TableSymbol.Empty);
        var resultTypeMember = resultType.Members;

        var columns = new List<IROutputColumnNode>();
        foreach (var t in resultTypeMember)
        {
            var symbol = (ColumnSymbol)t;
            var referencedColumnIndex = _rowScope.Members.IndexOf(symbol);
            columns.Add(new IROutputColumnNode(symbol,
                new IRRowScopeNameReferenceNode(symbol, symbol.Type, referencedColumnIndex)));
        }

        return new IRProjectOperatorNode(IRListNode.From(columns), resultType,false);
    }

    public override IRNode VisitExtendOperator(ExtendOperator node) =>
        HandleExtendOperatorInternal(node.Expressions, node.ResultType);

    public override IRNode VisitSerializeOperator(SerializeOperator node)
    {
        return  HandleExtendOperatorInternal(node.Expressions, node.ResultType,true) ;
    }

    private IRNode HandleExtendOperatorInternal(SyntaxList<SeparatedElement<Expression>> expressions,
        TypeSymbol resultType,bool requiresSerialization=false)
    {
        MyDebug.Assert(_rowScope != TableSymbol.Empty);
        var pendingExpressions = new LinkedList<IRExpressionNode>();
        //TODO HERE - we should not be keying on ColumnSymbol, but on the original column name/type
        var namedExtendedColumns = new Dictionary<ColumnSymbol, LinkedListNode<IRExpressionNode>>();
        foreach (var element in expressions)
        {
            var expression = element.Element;
            var irExpression = (IRExpressionNode)expression.Accept(this);
            var linkedListNode = pendingExpressions.AddLast(irExpression);

            if (expression is SimpleNamedExpression simpleNamedExpression)
            {
                var symbol = (ColumnSymbol)simpleNamedExpression.Name.ReferencedSymbol;
                if (symbol == null)
                {
                    throw new InvalidOperationException();
                }

                namedExtendedColumns.Add(symbol, linkedListNode);
            }
        }

        var resultTypeMember = resultType.Members;
        var columns = new List<IROutputColumnNode>();
        for (var i = 0; i < resultTypeMember.Count; i++)
        {
            var column = (ColumnSymbol)resultTypeMember[i];

            IRExpressionNode irExpression;
            if (i >= resultTypeMember.Count - pendingExpressions.Count)
            {
                irExpression = pendingExpressions.First!.Value;
                pendingExpressions.RemoveFirst();
            }
            else if (namedExtendedColumns.TryGetValue(column, out var linkedListNode))
            {
                irExpression = linkedListNode.Value;
                pendingExpressions.Remove(linkedListNode);
            }
            else
            {
                var referencedColumnIndex = _rowScope.Members.IndexOf(column);
                irExpression = new IRRowScopeNameReferenceNode(column, column.Type, referencedColumnIndex);
            }

            columns.Add(new IROutputColumnNode(column, irExpression));
        }

        return new IRProjectOperatorNode(IRListNode.From(columns), resultType,requiresSerialization);
    }
}
