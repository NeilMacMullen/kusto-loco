using System;
using System.ComponentModel;
using System.Text;
using Kusto.Language;
using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.Console;
using KustoLoco.Core.InternalRepresentation;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.InternalRepresentation.Nodes.Statements;
using NotNullStrings;

namespace KustoLoco.Core.Diagnostics;

internal class IrNodeVisualizer(IKustoConsole console)
{
    public void DumpKustoTree(KustoCode code,bool show)
    {
        if (!show)
            return ;
        var sb = new StringBuilder();
        var indent = 0;
        SyntaxElement.WalkNodes(
            code.Syntax,
            node =>
            {
                sb.Append(new string(' ', indent));
                sb.AppendLine(
                    $"{node.Kind}: {node.ToString(IncludeTrivia.SingleLine)}: {SchemaDisplay.GetText((node as Expression)?.ResultType)}");
                indent++;
            },
            node => { indent--; });

        sb.AppendLine();
        sb.AppendLine();
        var s=sb.ToString();
        console.WriteLine(s);
    }

    public static bool ContainsForbidden(KustoCode code)
    {
        var result = false;
        SyntaxElement.WalkNodes(
            code.Syntax,
            node =>
            {
                if (node is QueryOperator op)
                {
                    var n = op.Kind.ToString()
                        .ToLowerInvariant()
                        .Replace("operator", "")
                        ;
                    if ("extend join summarize project".Contains(n))
                        result = true;
                }
            
            });
        return result;
    }

    public void DumpIRTree(IRNode node,bool show)
    {
        if(!show)
            return ;
        DumpTreeInternal(node, "");

        console.WriteLine();
        console.WriteLine();
    }

    private void DumpTreeInternal(IRNode node, string indent, bool isLast = true)
    {
        var oldColor = console.ForegroundColor;
        try
        {
            console.ForegroundColor = ConsoleColor.DarkGray;

            console.Write(indent);
            console.Write(isLast ? " └─" : " ├─");

            var c = node switch
            {
                IRListNode => ConsoleColor.DarkGray,
                IRStatementNode => ConsoleColor.White,
                IRQueryOperatorNode => ConsoleColor.DarkBlue,
                IRLiteralExpressionNode => ConsoleColor.Magenta,
                IRNameReferenceNode => ConsoleColor.Green,
                IRExpressionNode => ConsoleColor.Cyan,
                _ => ConsoleColor.Gray
            };
            console.ForegroundColor = c;
            console.WriteLine(node?.ToString().NullToEmpty()!);
        }
        finally
        {
            console.ForegroundColor = oldColor;
        }

        indent += isLast ? "   " : " | ";

        for (var i = 0; i < node!.ChildCount; i++)
        {
            var child = node.GetChild(i);
            DumpTreeInternal(child, indent, i == node.ChildCount - 1);
        }
    }
}
