namespace KustoLoco.Rendering.SixelSupport;

/// <summary>
/// Helper class to get current terminal dimensions
/// </summary>
public static class TerminalHelper
{
    /// <summary>
    /// Attempt to get the dimensions (in pixels) of  single character
    /// </summary>
    public static (int width, int height) GetCellDimensions()
    {

        var response = GetControlSequenceResponse("[16t");

        try
        {
            var parts = response.Split(';', 't');
            var pixelWidth = int.Parse(parts[2]);
            var pixelHeight = int.Parse(parts[1]);
            return (pixelWidth, pixelHeight);
        }
        catch
        {
        }
        return (10, 10);

    }

    public static (int width, int height) GetScreenDimension(int linesAtEnd=0)
    {
        var (w,h) = GetCellDimensions();
        return (w*Console.WindowWidth,
            h*Math.Max(1,Console.WindowHeight-linesAtEnd));
    }
    private static string GetControlSequenceResponse(string controlSequence)
    {
        char? c;
        var response = string.Empty;
        Console.Write($"{ESC}{controlSequence}");
        do
        {
            c = Console.ReadKey(true).KeyChar;
            response += c;
        } while (c != 'c' && Console.KeyAvailable);

        return response;
    }

    internal const string ESC = "\u001b";

}
