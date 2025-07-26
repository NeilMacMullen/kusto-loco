using AwesomeAssertions;
using AwesomeAssertions.Collections;
using AwesomeAssertions.Primitives;
using lokqlDxComponents.Services;

namespace lokqlDxComponentsTests;

public static class PathAssertionExtensions
{
    public static AndConstraint<StringAssertions> ShouldBeEquivalentToPath(this string path, string expected) => path
        .Should()
        .BeEquivalentTo(expected, o => o.Using(PathComparer.Instance));

    public static AndConstraint<StringCollectionAssertions<IEnumerable<string>>> ShouldBeEquivalentToPaths(
        this IEnumerable<string> paths,
        params string[] expected
    ) => paths
        .Should()
        .BeEquivalentTo(expected, o => o.Using(PathComparer.Instance));
}
