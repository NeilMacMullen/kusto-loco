// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.Util;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Other;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;


namespace KustoLoco.Core.InternalRepresentation;



internal partial class IRTranslator : DefaultSyntaxVisitor<IRNode>
{
    private readonly Dictionary<FunctionSymbol, ScalarFunctionInfo> _functions;
    private readonly Dictionary<string, EvaluatedExpressionKind> _inScopeSymbolInfos = new();
    private TableSymbol _rowScope = TableSymbol.Empty;

    public IRTranslator(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions) => _functions = functions;

    private void SetInScopeSymbolInfo(string name, EvaluatedExpressionKind resultKind)
    {
        _inScopeSymbolInfos.Add(name, resultKind);
    }

    protected override IRNode DefaultVisit(SyntaxNode node)
    {
        if (node is QueryOperator queryOp)
        {
            var keyword = queryOp.Kind.ToString().Replace("Operator", "").ToLowerInvariant();
            throw new NotImplementedException($"Query operator '{keyword}' not supported");
        }
        
        throw new NotImplementedException($"Unrecognised token type {node.Kind}");
    }

    public override IRNode VisitQueryBlock(QueryBlock node)
    {
        var irStatements = new List<IRStatementNode>();
        foreach (var se in node.Statements)
        {
            var statement = se.Element;
            var irStatement = (IRStatementNode)statement.Accept(this);
            irStatements.Add(irStatement);
        }

        var listNode = IRListNode.From(irStatements);
        return new IRQueryBlockNode(listNode);
    }
}
