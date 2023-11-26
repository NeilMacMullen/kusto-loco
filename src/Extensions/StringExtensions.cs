namespace Extensions;

public static class StringExtensions
{
    /// <summary>
    ///     Joins an enumerable into a string by calling ToString on each of then
    /// </summary>
    /// <remarks> The default separator is a comma</remarks>
    public static string JoinString<T>(this IEnumerable<T> items, string separator = ",")
        => string.Join(separator, items);


    /// <summary>
    ///     Joins an enumerable into a string by calling function  on each of then
    /// </summary>
    public static string JoinString<T>(this IEnumerable<T> items, string separator, Func<T, string> converter)
        => string.Join(separator, items.Select(converter));

    /// <summary>
    ///     Joins a set of things into a single string by calling ToString and separating with newlines
    /// </summary>
    public static string JoinAsLines<T>(this IEnumerable<T> items) => items.JoinString(Environment.NewLine);

    /// <summary>
    ///     Splits string into array of non-empty tokens separated by any of the characters in separationChars
    /// </summary>
    public static string[] Tokenise(this string input, string separationCharacters)
    {
        if (input == null)
            return Array.Empty<string>();
        return input.Split(separationCharacters.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => t.Length != 0)
            .ToArray();
    }

    /// <summary>
    ///     Tokenises all strings in input into a single array by splitting at any of separationCharacters
    /// </summary>
    public static string[] Tokenise(this IEnumerable<string> input, string separationCharacters)
        => input.SelectMany(s => s.Tokenise(separationCharacters)).ToArray();

    /// <summary>
    ///     Splits string into array of non-empty tokens separated by space
    /// </summary>
    public static string[] Tokenise(this string input) => input.Tokenise(" \t");

    public static bool IsBlank(this string s) => string.IsNullOrWhiteSpace(s);
    public static bool IsNotBlank(this string s) => !s.IsBlank();
}