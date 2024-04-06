#pragma warning disable CS8618
public class VirtualConsoleProgressReporter(IConsole console) : IProgress<string>
{
    public void Report(string value)
    {
        console.WriteLine(value);
    }
}