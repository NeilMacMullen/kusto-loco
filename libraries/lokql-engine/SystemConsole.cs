namespace Lokql.Engine;

/// <summary>
///     implementation of IConsole that uses the actual system console
/// </summary>
public class SystemConsole : IConsole
{
    public void Write(string s)
    {
        Console.Write(s);
    }

    public void SetForegroundColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }

    public int WindowWidth => Console.WindowWidth;


    public string ReadLine()
    {
        return Console.ReadLine() ?? string.Empty;
    }
}
