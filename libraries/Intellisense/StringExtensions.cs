namespace Intellisense;

internal static class StringExtensions
{
    public static bool IsLowercase(this string text)
    {
        foreach (var c in text.AsSpan())
        {
            if (!char.IsLower(c))
            {
                return false;
            }
        }

        return true;
    }
}
