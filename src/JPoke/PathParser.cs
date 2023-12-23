using System.Collections.Immutable;
using System.Text.RegularExpressions;

public class PathParser
{
    public static JPathElement ParseElement(string element)
    {
        var match = Regex.Match(element, @"(.*)\[\s*(\d*)\s*]");
        var isArray = match.Success;
        var elementRoot = isArray ? match.Groups[1].Value : element;
        var index = 
            int.TryParse(match.Groups[2].Value, out var v) ? v : -1;
        return new JPathElement(elementRoot, isArray, index);
    }

    public static JPath Parse(string path)
    {
        var elements = path.Split(".").ToArray();
        return new JPath(
            elements.Select(ParseElement).ToImmutableArray());
    }

    public readonly record struct JSplitPath(JPath Parent, JPath Child)
    {
        public JSplitPath Create(JPath initial) => new(JPath.Empty, initial);

        public JSplitPath Descend() => new(Parent.Append(Child.Elements.First()), Child.Descend());
    }

    public readonly record struct JPathElement(string Name, bool IsIndex, int Index);

    public readonly record struct JPath(ImmutableArray<JPathElement> Elements)
    {
        public static readonly JPath Empty = new(ImmutableArray<JPathElement>.Empty);
        public bool IsTerminal => Elements.Length == 1;
        public int Length => Elements.Length;

        public JPath Descend() => new(Elements.Slice(1, Elements.Length - 1));

        public JPath Append(JPathElement el) => new(Elements.Append(el).ToImmutableArray());
    }
}