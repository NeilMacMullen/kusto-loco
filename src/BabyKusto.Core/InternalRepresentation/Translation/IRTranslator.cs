// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator : DefaultSyntaxVisitor<IRNode>
    {
        private readonly Dictionary<string, EvaluatedExpressionKind> _inScopeSymbolInfos = new();
        private TableSymbol? _rowScope;

        internal IRTranslator()
        {
        }

        private void SetInScopeSymbolInfo(string name, EvaluatedExpressionKind resultKind)
        {
            _inScopeSymbolInfos.Add(name, resultKind);
        }

        protected override IRNode DefaultVisit(SyntaxNode node)
        {
            throw new NotImplementedException(TypeNameHelper.GetTypeDisplayName(node));
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
}
