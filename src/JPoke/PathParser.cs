using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace JPoke;

public class PathParser
{
    public static JPathElement ParseElement(string element)
    {
        var match = Regex.Match(element, @"(.*)\[\s*(\S*)\s*]");
        var isArray = match.Success;
        var elementRoot = isArray ? match.Groups[1].Value : element;
        if (!isArray)
            return new JPathElement(elementRoot, JPathIndex.None);
        var indexer = match.Groups[2].Value;
        //the syntax '[]' is equivalent to '[-1]'
        return indexer.Length == 0
            ? new JPathElement(elementRoot, JPathIndex.CreateRelative(-1))
            : "+-".Contains(indexer[0])
                ? new JPathElement(elementRoot, JPathIndex.CreateRelative(
                    int.Parse(indexer[1..])))
                : new JPathElement(elementRoot, JPathIndex.CreateAbsolute(
                    int.Parse(indexer)));
    }

    public static JPath Parse(string path)
    {
        if (path.Trim().Length == 0)
            return JPath.Empty;
        var elements = path.Split(".").ToArray();
        return new JPath(
            elements.Select(ParseElement).ToImmutableArray());
    }
}