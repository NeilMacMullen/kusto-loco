// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BabyKusto.Core.Evaluation.BuiltIns;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using Kusto.Language.Utils;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.InternalRepresentation
{
    internal partial class IRTranslator : DefaultSyntaxVisitor<IRNode>
    {
        public override IRNode VisitSimpleNamedExpression(SimpleNamedExpression node)
        {
            return node.Expression.Accept(this);
        }

        public override IRNode VisitNameReference(NameReference node)
        {
            if (_rowScope != null)
            {
                int index = _rowScope.Members.IndexOf(node.ReferencedSymbol);
                if (index >= 0)
                {
                    return new IRRowScopeNameReferenceNode(node.ReferencedSymbol, node.ResultType, index);
                }
            }

            // If we know more about this symbol, use this info.
            // This is useful when the NameReference we are visiting is a user-defined-function parameter,
            // in which case we need this additional information to ascertain if the data is e.g. scalar or columnar.
            // TODO: Do this right. Look-up by string from global scope is likely to produce wrong results in many cases.
            // Ideally we'd want to track argument to parameter variable symbols.
            if (_inScopeSymbolInfos.TryGetValue(node.Name.SimpleName, out var resultKind))
            {
                return new IRNameReferenceNode(node.ReferencedSymbol, node.ResultType, resultKind);
            }

            return new IRNameReferenceNode(node.ReferencedSymbol, node.ResultType);
        }

        public override IRNode VisitLiteralExpression(LiteralExpression node)
        {
            return new IRLiteralExpressionNode(node.LiteralValue, node.ResultType);
        }

        public override IRNode VisitCompoundStringLiteralExpression(CompoundStringLiteralExpression node)
        {
            return new IRLiteralExpressionNode(node.LiteralValue, node.ResultType);
        }

        public override IRNode VisitPipeExpression(PipeExpression node)
        {
            var irExpression = (IRExpressionNode)node.Expression.Accept(this);
            var oldRowScope = _rowScope;
            _rowScope = (TableSymbol)irExpression.ResultType;
            try
            {
                var irOperator = (IRQueryOperatorNode)node.Operator.Accept(this);

                return new IRPipeExpressionNode(irExpression, irOperator, node.ResultType);
            }
            finally
            {
                _rowScope = oldRowScope;
            }
        }

        public override IRNode VisitBinaryExpression(BinaryExpression node)
        {
            var signature = node.ReferencedSignature;
            if (signature == null)
            {
                throw new InvalidOperationException($"Unexpected binary expression operator missing referenced signature ({node}).");
            }

            var irLeft = (IRExpressionNode)node.Left.Accept(this);
            var irRight = (IRExpressionNode)node.Right.Accept(this);

            var irArguments = new[] { irLeft, irRight };
            var parameters = signature.GetArgumentParameters(new[] { node.Left, node.Right });
            var overloadInfo = BuiltInOperators.GetOverload((OperatorSymbol)signature.Symbol, irArguments, parameters);

            ApplyTypeCoercions(irArguments, overloadInfo);
            return new IRBinaryExpressionNode(signature, overloadInfo, irArguments[0], irArguments[1], node.ResultType);
        }

        public override IRNode VisitPrefixUnaryExpression(PrefixUnaryExpression node)
        {
            var signature = node.ReferencedSignature;
            if (signature == null)
            {
                throw new InvalidOperationException($"Unexpected unar expression operator missing referenced signature ({node}).");
            }

            var irExpression = (IRExpressionNode)node.Expression.Accept(this);

            var irArguments = new[] { irExpression };
            var parameters = signature.GetArgumentParameters(new[] { node.Expression });
            var overloadInfo = BuiltInOperators.GetOverload((OperatorSymbol)signature.Symbol, irArguments, parameters);

            ApplyTypeCoercions(irArguments, overloadInfo);
            return new IRUnaryExpressionNode(signature, overloadInfo, irExpression, node.ResultType);
        }

        public override IRNode VisitFunctionCallExpression(FunctionCallExpression node)
        {
            var signature = node.ReferencedSignature;
            if (signature == null)
            {
                throw new InvalidOperationException($"signature was null ({node}).");
            }

            var arguments = new Expression[node.ArgumentList.Expressions.Count];
            var irArguments = new IRExpressionNode[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                var argument = node.ArgumentList.Expressions[i].Element;
                arguments[i] = argument;
                irArguments[i] = (IRExpressionNode)argument.Accept(this);
            }

            var parameters = signature.GetArgumentParameters(arguments);

            bool isUserFunction = signature.Declaration != null; // user functions have declarations
            if (isUserFunction)
            {
                var expansion = node.GetCalledFunctionBody();
                if (expansion == null)
                {
                    throw new InvalidOperationException($"Failed to get function expansion for {node.Name.SimpleName}.");
                }

                var paramSymbols = new VariableSymbol[arguments.Length];
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].DeclaredTypes.Count != 1)
                    {
                        throw new InvalidOperationException($"Parameters with more than one declared type is not supported, found {parameters[i].DeclaredTypes.Count}.");
                    }

                    paramSymbols[i] = new VariableSymbol(parameters[i].Name, parameters[i].DeclaredTypes.Single());
                }

                IRFunctionBodyNode irFunctionBody;
                {
                    var nestedTranslator = new IRTranslator();
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        // NOTE: For now we only support type coercions for scalars. Bad things may happen for tabular inputs, oh well...
                        if (irArguments[i].ResultType is ScalarSymbol && paramSymbols[i].Type != irArguments[i].ResultType)
                        {
                            irArguments[i] = new IRCastExpressionNode(irArguments[i], paramSymbols[i].Type);
                        }

                        nestedTranslator.SetInScopeSymbolInfo(paramSymbols[i].Name, irArguments[i].ResultKind);
                    }

                    irFunctionBody = (IRFunctionBodyNode)expansion.Accept(nestedTranslator);
                }

                return new IRUserFunctionCallNode(signature, parameters, paramSymbols, irFunctionBody, IRListNode.From(irArguments), node.ResultType);
            }

            var functionSymbol = (FunctionSymbol)signature.Symbol;
            if (BuiltInScalarFunctions.TryGetOverload(functionSymbol, irArguments, parameters, out var functionOverload))
            {
                Debug.Assert(functionOverload != null);
                ApplyTypeCoercions(irArguments, functionOverload);
                return new IRBuiltInScalarFunctionCallNode(signature, functionOverload, parameters, IRListNode.From(irArguments), node.ResultType);
            }
            else if (BuiltInAggregates.TryGetOverload(functionSymbol, irArguments, parameters, out var aggregateOverload))
            {
                Debug.Assert(aggregateOverload != null);
                ApplyTypeCoercions(irArguments, aggregateOverload);
                return new IRAggregateCallNode(signature, aggregateOverload, parameters, IRListNode.From(irArguments), node.ResultType);
            }
            else if (BuiltInWindowFunctions.TryGetOverload(functionSymbol, irArguments, parameters, out var windowFunctionOverload))
            {
                Debug.Assert(windowFunctionOverload != null);
                ApplyTypeCoercions(irArguments, windowFunctionOverload);
                return new IRBuiltInWindowFunctionCallNode(signature, windowFunctionOverload, parameters, IRListNode.From(irArguments), node.ResultType);
            }
            else
            {
                throw new InvalidOperationException($"Function {functionSymbol.Display} is not implemented.");
            }
        }

        private static void ApplyTypeCoercions(IRExpressionNode[] irArguments, OverloadInfoBase overloadInfo)
        {
            for (int i = 0; i < irArguments.Length; i++)
            {
                if (overloadInfo.ParameterTypes[i] != irArguments[i].ResultType)
                {
                    irArguments[i] = new IRCastExpressionNode(irArguments[i], overloadInfo.ParameterTypes[i]);
                }
            }
        }

        public override IRNode VisitMaterializeExpression(MaterializeExpression node)
        {
            var irExpression = (IRExpressionNode)node.Expression.Accept(this);
            return new IRMaterializeExpressionNode(irExpression, node.ResultType);
        }

        public override IRNode VisitParenthesizedExpression(ParenthesizedExpression node)
        {
            return node.Expression.Accept(this);
        }

        public override IRNode VisitDataTableExpression(DataTableExpression node)
        {
            int numColumns = node.Schema.Columns.Count;

            if (node.Values.Count % numColumns != 0)
            {
                throw new InvalidOperationException($"Expected number of values ({node.Values.Count}) to be a multiple of the number of columns ({numColumns}).");
            }

            var data = new object?[node.Values.Count];
            for (int i = 0; i < node.Values.Count; i++)
            {
                var expression = node.Values[i].Element;
                if (expression is not LiteralExpression literalExpression)
                {
                    throw new InvalidOperationException($"Expected literal expression in datatable values, found {TypeNameHelper.GetTypeDisplayName(expression)}.");
                }

                data[i] = literalExpression.LiteralValue;
            }

            return new IRDataTableExpression(data, node.ResultType);
        }

        public override IRNode VisitToScalarExpression(ToScalarExpression node)
        {
            var irExpression = (IRExpressionNode)node.Expression.Accept(this);
            return new IRToScalarExpressionNode(irExpression, node.ResultType);
        }
    }
}
