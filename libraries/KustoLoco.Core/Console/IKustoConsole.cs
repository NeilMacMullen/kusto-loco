using System;

namespace KustoLoco.Core.Console;

/// <summary>
///     Describes operations required for a basic console type display
/// </summary>
/// <remarks>
///     This interface allows us to render content in a few predefined styles to whatever
///     display we have.  It's intended to be used in place of IProgress in order
///     to give us more flexibility
/// </remarks>
public interface IKustoConsole
{
    /// <summary>
    ///     Width of the console window in characters
    /// </summary>
    int WindowWidth { get; }

    ConsoleColor ForegroundColor { get; set; }

    void Write(string s);

    void WriteLine(string s)
    {
        Write(s + Environment.NewLine);
    }

    void WriteLine()
    {
        WriteLine(string.Empty);
    }

    string ReadLine();

    public void WriteLine(ConsoleColor color, string s)
    {
        ForegroundColor = color;
        WriteLine(s);
    }

    public void Info(string s)
    {
        WriteLine(ConsoleColor.Green, s);
    }


    public void Error(string s)
    {
        WriteLine(ConsoleColor.Red, s);
    }

    public void Warn(string s)
    {
        WriteLine(ConsoleColor.Yellow, s);
    }


    public void Progress(string s)
    {
        WriteLine(ConsoleColor.Gray, s);
    }

    void ShowProgress(string s)
    {
        Progress(s);
    }

    void CompleteProgress(string s)
    {
        Progress(s);
    }
}
