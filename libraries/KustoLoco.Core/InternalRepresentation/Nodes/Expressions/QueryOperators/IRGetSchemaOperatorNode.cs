﻿using Kusto.Language.Symbols;
using NLog;

namespace KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;

internal class IRGetSchemaOperatorNode : IRQueryOperatorNode
{
    private static Logger Logger = LogManager.GetCurrentClassLogger();

    public IRGetSchemaOperatorNode()
        : base(ScalarTypes.Null)
    {
    }


    public override int ChildCount => 0;


    public override TResult Accept<TResult, TContext>(IRNodeVisitor<TResult, TContext> visitor, TContext context) =>
        visitor.VisitGetSchemaOperator(this, context);

    public override string ToString() => "GetSchemaOperator";
}