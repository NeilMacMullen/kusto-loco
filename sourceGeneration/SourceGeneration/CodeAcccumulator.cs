using System;
using System.Text;

namespace SourceGeneration
{
    public class CodeEmitter
    {
        private readonly StringBuilder dbg = new StringBuilder();
        private int indentLevel;

        public void AppendLine(string line)
        {
            dbg.AppendLine(indent() + line);
        }

        public void Append(string line) => dbg.Append(line);
        private string indent() => "".PadLeft(indentLevel * 4);
        public override string ToString() => dbg.ToString();

        public void EnterCodeBlock()
        {
            AppendLine("{");
            indentLevel++;
        }

        public void ExitCodeBlock()
        {
            indentLevel--;
            AppendLine("}");
        }

        public void AppendStatement(string s)
        {
            AppendLine($"{s};");
        }

        public void EnterCommentBlock()
        {
            AppendLine("/*");
            indentLevel++;
        }

        public void ExitCommentBlock()
        {
            indentLevel--;
            AppendLine("*/");
        }

        public void LogException(Exception exception)
        {
            EnterCommentBlock();
            AppendLine("INTERNAL ERROR");
            AppendLine(exception.Message);
            AppendLine(exception.ToString());
            ExitCommentBlock();
        }
    }
}