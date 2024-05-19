using System.Text;
using KustoLoco.Core.Console;

namespace Lokql.Engine;

/// <summary>
///     Wraps a TextWriter around an IKustoConsole
/// </summary>
/// <param name="console"></param>
public class ConsoleTextWriter(IKustoConsole console) : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        console.Write(value.ToString());
    }
}
