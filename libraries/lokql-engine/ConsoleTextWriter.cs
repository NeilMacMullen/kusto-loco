using System.Text;

#pragma warning disable CS8618, CS8604
/// <summary>
///     Wraps a TextWriter around an IConsole
/// </summary>
/// <param name="console"></param>
public class ConsoleTextWriter(IConsole console) : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        console.Write(value.ToString());
    }
}