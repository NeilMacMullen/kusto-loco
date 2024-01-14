// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using BabyKusto.Core.Evaluation.BuiltIns;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.InternalRepresentation;

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

    protected override IRNode DefaultVisit(SyntaxNode node) =>
        throw new NotImplementedException(TypeNameHelper.GetTypeDisplayName(node));

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