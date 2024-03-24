namespace KustoLoco.FileFormats;

public class NullProgressReporter : IProgress<string>
{
    public void Report(string value)
    {
       
    }
}