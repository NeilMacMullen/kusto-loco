using System.Text;

#pragma warning disable CS8618, CS8604
public class SystemConsole : IConsole
{

    public class ConsoleWriter : TextWriter
    {
        public override Encoding Encoding => Console.OutputEncoding;
        public override void Write(char c)
        {
           Console.Write(c);
        }
    }
    private readonly TextWriter _writer;

    public SystemConsole()
    {
        _writer = new ConsoleWriter();
    }
    public void Write(string s)
    {
        Console.Write(s);
    }

    public void SetForegroundColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }

    public int WindowWidth => Console.WindowWidth;

    public TextWriter Writer => _writer;

    public string ReadLine()
    {
        return Console.ReadLine()??string.Empty ;
    }
}