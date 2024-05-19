using System;

namespace KustoLoco.Core.Console;

/// <summary>
///     implementation of IKustoConsole that uses the actual system console
/// </summary>
public class SystemConsole : IKustoConsole
{
    public void Write(string s)
    {
        System.Console.Write(s);
    }

    public ConsoleColor ForegroundColor
    {
        get => System.Console.ForegroundColor;
        set => System.Console.ForegroundColor = value;
    }
    public int WindowWidth => System.Console.WindowWidth;


    public string ReadLine()
    {
        return System.Console.ReadLine() ?? string.Empty;
    }
}
