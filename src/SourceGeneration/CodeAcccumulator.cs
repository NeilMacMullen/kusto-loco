using System.Text;

namespace SourceGeneration
{
    public class CodeAcccumulator
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
    }
}