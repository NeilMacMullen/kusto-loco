#pragma warning disable CS8618, CS8604
/// <summary>
///     Describes operations required for a basic console type display
/// </summary>
public interface IConsole
{
    /// <summary>
    ///     Width of the console window in characters
    /// </summary>
    int WindowWidth { get; }

    void Write(string s);
    void WriteLine(string s) => Write(s + Environment.NewLine);
    void SetForegroundColor(ConsoleColor color);
    void WriteLine() => WriteLine(string.Empty);
    string ReadLine();
}