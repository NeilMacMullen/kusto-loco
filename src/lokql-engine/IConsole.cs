#pragma warning disable CS8618, CS8604
public interface IConsole
{
    void Write(string s);
    void WriteLine(string s) => Write(s + Environment.NewLine);
    void SetForegroundColor(ConsoleColor color);
    void WriteLine() => WriteLine(string.Empty);
    int WindowWidth { get; }
    TextWriter Writer { get; }
    string ReadLine();
}

