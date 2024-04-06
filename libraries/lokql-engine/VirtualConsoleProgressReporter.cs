

namespace Lokql.Engine;


public class VirtualConsoleProgressReporter(IConsole console) : IProgress<string>
{
    public void Report(string value)
    {
        console.WriteLine(value);
    }
}