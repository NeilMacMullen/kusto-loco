namespace KustoLoco.FileFormats;

public class ConsoleProgressReporter : IProgress<string>
{
    public void Report(string value)
    {
        Console.WriteLine(value);
    }
}