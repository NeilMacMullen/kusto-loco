using System;
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

internal class IrNodeVisualizer
{
    private readonly IKustoConsole _console = new SystemConsole();

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
        _console.WriteLine(s);
    }

    public void DumpIRTree(IRNode node,bool show)
    {
        if(!show)
            return ;
        DumpTreeInternal(node, "");

        _console.WriteLine();
        _console.WriteLine();
    }

    private void DumpTreeInternal(IRNode node, string indent, bool isLast = true)
    {
        var oldColor = _console.ForegroundColor;
        try
        {
            _console.ForegroundColor = ConsoleColor.DarkGray;

            _console.Write(indent);
            _console.Write(isLast ? " └─" : " ├─");

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
            _console.ForegroundColor = c;
            _console.WriteLine(node?.ToString().NullToEmpty()!);
        }
        finally
        {
            _console.ForegroundColor = oldColor;
        }

        indent += isLast ? "   " : " | ";

        for (var i = 0; i < node!.ChildCount; i++)
        {
            var child = node.GetChild(i);
            DumpTreeInternal(child, indent, i == node.ChildCount - 1);
        }
    }
}
