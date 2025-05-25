// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using Kusto.Language.Utils;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.Core.Extensions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.InternalRepresentation.Nodes.Other;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.InternalRepresentation;

internal partial class IRTranslator : DefaultSyntaxVisitor<IRNode>
{
    public override IRNode VisitBetweenExpression(BetweenExpression node)
    {
        var couple = node.Right.Accept(this)
            as IRExpressionCoupleNode;
        var signature = node.ReferencedSignature;

        var leftRange = couple!._left;
        var rightRange = couple._right;

        var parameterExpression = node.Left.Accept(this) as IRExpressionNode;

        //between used after datetime range thinks type is unknown...
        var irArguments = new[] { parameterExpression!, leftRange, rightRange };
        var overloadInfo = BuiltInOperators.GetOverload((OperatorSymbol)signature.Symbol,
            node.ResultType,irArguments);

        //ApplyTypeCoercions(irArguments, overloadInfo);

        return new IRBuiltInScalarFunctionCallNode(signature,
            overloadInfo, new List<Parameter>(), IRListNode.From(irArguments), ScalarTypes.Bool);
    }

    public  IRNode VisitExpressionWithArray(Expression node,Expression left,ExpressionList right)
    {
        var signature = node.ReferencedSignature;
        var parameterExpression = left.Accept(this) as IRExpressionNode;
        var ja = new JsonArray();
        var expresions =right.Expressions
            .OfType<SeparatedElement>()
            .Select(e => e.Element)
            .OfType<LiteralExpression>()
            .ToArray();

        if (parameterExpression!.ResultType.IsStringOrArray())
        {
            var items = expresions
                .Select(expr => expr.LiteralValue as string)
                .ToArray();
            ja = JsonArrayHelper.From(items);
        }
        else if (parameterExpression.ResultType.IsNumeric())
        {
            var items = expresions
                .Select(expr => expr.LiteralValue as long?)
                .ToArray();
            ja = JsonArrayHelper.From(items);
        }
        else
        {
            throw new NotImplementedException("In expressions are only supported for arrays of string/long literals");
        }

        var str1 = new IRPreEvaluatedScalarExpressionNode(ja, DynamicArraySymbol.From("dynamic"));
        var irArguments = new[] { parameterExpression!, str1! };
        var overloadInfo = BuiltInOperators.GetOverload((OperatorSymbol)signature.Symbol,
            node.ResultType,irArguments);
        return new IRBuiltInScalarFunctionCallNode(signature,
            overloadInfo, new List<Parameter>(), IRListNode.From(irArguments), ScalarTypes.Bool);
    }

    public override IRNode VisitInExpression(InExpression node)
    {
        return VisitExpressionWithArray(node, node.Left, node.Right);
    }


    public override IRNode VisitHasAnyExpression(HasAnyExpression node)
    {
        return VisitExpressionWithArray(node, node.Left, node.Right);
    }

    public override IRNode VisitHasAllExpression(HasAllExpression node)
    {
        return VisitExpressionWithArray(node, node.Left, node.Right);
    }

    public override IRNode VisitSimpleNamedExpression(SimpleNamedExpression node) => node.Expression.Accept(this);

    //TODO - this is a nightmare.... there's an assumption that elementary Kusto types are reference equatable but it simply
//isn't true (or else we can't trust it to be so).  Really we want to compare only the important properties.
    public override IRNode VisitNameReference(NameReference node)
    {
        if (_rowScope != TableSymbol.Empty)
        {
            var index = _rowScope.Members.IndexOf(node.ReferencedSymbol);
            if (index >= 0) return new IRRowScopeNameReferenceNode(node.ReferencedSymbol, node.ResultType, index);


            //try column lookup ...
            var m = _rowScope.Members.FirstIndex(t =>
                t is ColumnSymbol cs && cs.Name == node.ReferencedSymbol.Name && cs.Type == node.ResultType);
            if (m >= 0) return new IRRowScopeNameReferenceNode(node.ReferencedSymbol, node.ResultType, m);

            //if the node referenced symbol has type unknown because it may have come
            //from an expression that failed to be evaluated (possibly bug in parser) then 
            //try a more relaxed fit..

            if (node.ResultType == ScalarTypes.Unknown)
            {
                var matchingNames = _rowScope.Members.FirstIndex(t => t.Name == node.ReferencedSymbol.Name);

                if (matchingNames >= 0)
                    return new IRRowScopeNameReferenceNode(node.ReferencedSymbol, node.ResultType, matchingNames);
            }
        }

        // If we know more about this symbol, use this info.
        // This is useful when the NameReference we are visiting is a user-defined-function parameter,
        // in which case we need this additional information to ascertain if the data is e.g. scalar or columnar.
        // TODO: Do this right. Look-up by string from global scope is likely to produce wrong results in many cases.
        // Ideally we'd want to track argument to parameter variable symbols.
        return
            _inScopeSymbolInfos.TryGetValue(node.Name.SimpleName, out var resultKind)
                ? new IRNameReferenceNode(node.ReferencedSymbol, node.ResultType, resultKind)
                : new IRNameReferenceNode(node.ReferencedSymbol, node.ResultType);
    }

    public override IRNode VisitLiteralExpression(LiteralExpression node) =>
        new IRLiteralExpressionNode(node.LiteralValue, node.ResultType);

    public override IRNode VisitDynamicExpression(DynamicExpression node)
    {
        var literalValue = (string)node.LiteralValue;
        try
        {
            var parsedValue = JsonNode.Parse(literalValue);
            return new IRLiteralExpressionNode(parsedValue, node.ResultType);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                $"A literal dynamic expression was provided that is valid in Kusto but doesn't conform to the stricter JSON parser used by KustoLoco. Please rewrite the literal expression to be properly formatted JSON (invalid input: \"{literalValue}\")");
        }
    }

    public override IRNode VisitPathExpression(PathExpression node)
    {
        var irExpression = (IRExpressionNode)node.Expression.Accept(this);

        return node.Selector is NameReference selectorName
            ? (IRNode)new IRMemberAccessNode(irExpression, selectorName.SimpleName, node.ResultType)
            : throw new InvalidOperationException(
                $"Expected path selector to be {TypeNameHelper.GetTypeDisplayName(typeof(NameReference))}, but found {TypeNameHelper.GetTypeDisplayName(node.Selector)}");
    }

    public override IRNode VisitElementExpression(ElementExpression node)
    {
        var irExpression = (IRExpressionNode)node.Expression.Accept(this);

        if (node.Selector is not BracketedExpression selector)
            throw new InvalidOperationException(
                $"Expected element selector to be {TypeNameHelper.GetTypeDisplayName(typeof(NameReference))}, but found {TypeNameHelper.GetTypeDisplayName(node.Selector)}");

        var selectorExpression = selector.Expression;

        if (selectorExpression is PrefixUnaryExpression pfxExpression)
        {
            //TODO HERE - this is a horrible hack to allow for negative array indices
            //surely there has to be a better way....
            var pvalue = pfxExpression.Accept(this) as IRUnaryExpressionNode;
            if (pvalue!.Expression is IRLiteralExpressionNode lit)
            {
                var v = (long)lit.Value!;
                var op = pvalue.Signature.Symbol.Name;
                if (op == Operators.UnaryMinus.Name) v = -v;
                return new IRArrayAccessNode(irExpression, (int)v, node.ResultType);
            }
        }

        if (selectorExpression is not LiteralExpression literalExpressionSelector)
            throw new InvalidOperationException(
                $"Expected element selector expression to be {TypeNameHelper.GetTypeDisplayName(typeof(LiteralExpression))}, but found {TypeNameHelper.GetTypeDisplayName(selector.Expression)}");

        var value = selectorExpression.Accept(this) as IRLiteralExpressionNode;
        if (value == null)
            throw new InvalidOperationException(
                $"Expected element selector expression to evaluate to {TypeNameHelper.GetTypeDisplayName(typeof(IRLiteralExpressionNode))}, but found {TypeNameHelper.GetTypeDisplayName(value)}");


        //support array access for dynamic objects
        if (value.Value is long index) return new IRArrayAccessNode(irExpression, (int)index, node.ResultType);


        return value.Value is not string stringValue
            ? throw new InvalidOperationException(
                $"Element selector expression evaluated to null or to an unexpected data type ({TypeNameHelper.GetTypeDisplayName(value.Value)})")
            : new IRMemberAccessNode(irExpression, stringValue, node.ResultType);
    }

    public override IRNode VisitCompoundStringLiteralExpression(CompoundStringLiteralExpression node) =>
        new IRLiteralExpressionNode(node.LiteralValue, node.ResultType);

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

        var irLeft = (IRExpressionNode)node.Left.Accept(this);
        var irRight = (IRExpressionNode)node.Right.Accept(this);

        var irArguments = new[] { irLeft, irRight };
        var overloadInfo = BuiltInOperators.GetOverload((OperatorSymbol)signature.Symbol,
            node.ResultType,irArguments);

        ApplyTypeCoercions(irArguments, overloadInfo);
        return new IRBinaryExpressionNode(signature, overloadInfo, irArguments[0], irArguments[1], node.ResultType);
    }

    public override IRNode VisitExpressionCouple(ExpressionCouple node)
    {
        var irLeft = (IRExpressionNode)node.First.Accept(this);
        var irRight = (IRExpressionNode)node.Second.Accept(this);

        return new IRExpressionCoupleNode(irLeft, irRight);
    }

    public override IRNode VisitPrefixUnaryExpression(PrefixUnaryExpression node)
    {
        var signature = node.ReferencedSignature;

        var irExpression = (IRExpressionNode)node.Expression.Accept(this);


        var irArguments = new[] { irExpression };
        var overloadInfo = BuiltInOperators.GetOverload((OperatorSymbol)signature.Symbol, node.ResultType, irArguments);

        ApplyTypeCoercions(irArguments, overloadInfo);
        return new IRUnaryExpressionNode(signature, overloadInfo, irExpression, node.ResultType);
    }

    public override IRNode VisitFunctionCallExpression(FunctionCallExpression node)
    {
        var signature = node.ReferencedSignature;
        var returnType = node.ResultType;

        var arguments = new Expression[node.ArgumentList.Expressions.Count];
        var irArguments = new IRExpressionNode[arguments.Length];
        for (var i = 0; i < arguments.Length; i++)
        {
            var argument = node.ArgumentList.Expressions[i].Element;
            arguments[i] = argument;
            irArguments[i] = (IRExpressionNode)argument.Accept(this);
        }

        var parameters = signature.GetArgumentParameters(arguments);

        var isUserFunction = signature.Declaration != null; // user functions have declarations
        if (isUserFunction)
        {
            var expansion = node.GetCalledFunctionBody();
            if (expansion == null)
                throw new InvalidOperationException(
                    $"Failed to get function expansion for {node.Name.SimpleName}.");

            var paramSymbols = new VariableSymbol[arguments.Length];
            for (var i = 0; i < parameters.Count; i++)
            {
                if (parameters[i].DeclaredTypes.Count != 1)
                    throw new InvalidOperationException(
                        $"Parameters with more than one declared type is not supported, found {parameters[i].DeclaredTypes.Count}.");

                paramSymbols[i] = new VariableSymbol(parameters[i].Name, parameters[i].DeclaredTypes.Single());
            }

            IRFunctionBodyNode irFunctionBody;
            {
                var nestedTranslator = new IRTranslator(_functions);
                for (var i = 0; i < arguments.Length; i++)
                {
                    // NOTE: For now we only support type coercions for scalars. Bad things may happen for tabular inputs, oh well...
                    if (irArguments[i].ResultType is ScalarSymbol &&
                        paramSymbols[i].Type.Simplify() != irArguments[i].ResultType.Simplify())
                        irArguments[i] = new IRCastExpressionNode(irArguments[i], paramSymbols[i].Type);

                    nestedTranslator.SetInScopeSymbolInfo(paramSymbols[i].Name, irArguments[i].ResultKind);
                }

                irFunctionBody = (IRFunctionBodyNode)expansion.Accept(nestedTranslator);
            }

            return new IRUserFunctionCallNode(signature, parameters, paramSymbols, irFunctionBody,
                IRListNode.From(irArguments), node.ResultType);
        }

        var functionSymbol = (FunctionSymbol)signature.Symbol;
        if (FunctionFinder.TryGetOverload(_functions, functionSymbol, returnType, irArguments, parameters,
                out var functionOverload))
        {
            Debug.Assert(functionOverload != null);
            ApplyTypeCoercions(irArguments, functionOverload);
            return new IRBuiltInScalarFunctionCallNode(signature, functionOverload, parameters,
                IRListNode.From(irArguments), node.ResultType);
        }

        if (BuiltInAggregates.TryGetOverload(functionSymbol,returnType, irArguments, parameters, out var aggregateOverload))
        {
            Debug.Assert(aggregateOverload != null);
            ApplyTypeCoercions(irArguments, aggregateOverload);
            return new IRAggregateCallNode(signature, aggregateOverload, parameters, IRListNode.From(irArguments),
                node.ResultType);
        }

        if (BuiltInWindowFunctions.TryGetOverload(functionSymbol, returnType, irArguments, parameters,
                out var windowFunctionOverload))
        {
            Debug.Assert(windowFunctionOverload != null);
            ApplyTypeCoercions(irArguments, windowFunctionOverload);
            return new IRBuiltInWindowFunctionCallNode(signature, windowFunctionOverload, parameters,
                IRListNode.From(irArguments), node.ResultType);
        }

        throw new InvalidOperationException(
            $"Function {functionSymbol.Name}{SchemaDisplay.GetText(functionSymbol)} is not implemented.");
    }

    private static void ApplyTypeCoercions(IRExpressionNode[] irArguments, OverloadInfoBase overloadInfo)
    {
        for (var i = 0; i < irArguments.Length; i++)
            if (overloadInfo.ParameterTypes[i].Simplify() != irArguments[i].ResultType.Simplify())
            {
                //ideally we would never need to do type coercions since we'd generate
                //all possible overload variants
                irArguments[i] = new IRCastExpressionNode(irArguments[i], overloadInfo.ParameterTypes[i]);
            }
    }

    public override IRNode VisitMaterializeExpression(MaterializeExpression node)
    {
        var irExpression = (IRExpressionNode)node.Expression.Accept(this);
        return new IRMaterializeExpressionNode(irExpression, node.ResultType);
    }

    public override IRNode VisitParenthesizedExpression(ParenthesizedExpression node) => node.Expression.Accept(this);

    public override IRNode VisitDataTableExpression(DataTableExpression node)
    {
        var numColumns = node.Schema.Columns.Count;

        if (node.Values.Count % numColumns != 0)
            throw new InvalidOperationException(
                $"Expected number of values ({node.Values.Count}) to be a multiple of the number of columns ({numColumns}).");

        var data = new object?[node.Values.Count];
        for (var i = 0; i < node.Values.Count; i++)
        {
            var irLiteralExpression = node.Values[i].Element.Accept(this);
            if (irLiteralExpression is not IRLiteralExpressionNode literalExpression)
                throw new InvalidOperationException(
                    $"Expected literal expression in datatable values, found {TypeNameHelper.GetTypeDisplayName(irLiteralExpression)}.");

            data[i] = literalExpression.Value;
        }

        return new IRDataTableExpression(data, node.ResultType);
    }

    public override IRNode VisitToScalarExpression(ToScalarExpression node)
    {
        var irExpression = (IRExpressionNode)node.Expression.Accept(this);
        return new IRToScalarExpressionNode(irExpression, node.ResultType);
    }
}
