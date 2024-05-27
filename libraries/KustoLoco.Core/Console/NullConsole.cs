using System;

namespace KustoLoco.Core.Console;

/// <summary>
/// An implementation of IKustoConsole that does nothing 
/// </summary>
public class NullConsole : IKustoConsole
{
    public void Write(string s)
    {
    }

    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
    public int WindowWidth => 80;

    public string ReadLine()
    {
        return string.Empty;
    }
}
