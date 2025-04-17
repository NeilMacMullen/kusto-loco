using System.Reflection;
using NotNullStrings;

namespace Lokql.Engine;

/// <summary>
///     Provides a way of checking whether the current version is out of date
/// </summary>
/// <remarks>
///     We look for new versions on the github page
/// </remarks>
public static class UpgradeManager
{
    public const string VersionSite = "https://raw.githubusercontent.com/wiki/NeilMacMullen/kusto-loco/__version.md";

    public static async Task<bool> UpdateAvailable()
    {
        try
        {
            using var client = new HttpClient();
            var remoteVersion = (await client.GetStringAsync(new Uri(VersionSite))).Trim();
            var thisVersion =Assembly.GetExecutingAssembly().GetName().Version?.ToString().NullToEmpty()!;
            return CompareVersions(remoteVersion, thisVersion) >0;
        }
        catch
        {
            return false;
        }
    }

    public static string GetCurrentVersion()
    {
        var v = Assembly.GetExecutingAssembly().GetName().Version;
        var thisVersion = v?.ToString().NullToEmpty()!;
        var (major,minor, patch) = DecomposeVersion(thisVersion);
        return $"{major}.{minor}.{patch}";
    }

    public static (int major, int minor, int patch) DecomposeVersion(string v)
    {
        try
        {
            var elements = v.Split(".")
                .Select(int.Parse)
                .ToArray();
            return (elements[0], elements[1], elements[2]);
        }
        catch
        {
            return (0, 0, 0);
        }
    }
    /// <summary>
    /// +ve if a >b
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static int CompareVersions(string a, string b)
    {
        var av = DecomposeVersion(a);
        var bv = DecomposeVersion(b);
        var d = av.major - bv.major;
        if (d != 0) return d;

        d = av.minor - bv.minor;
        if (d != 0) return d;
        d = av.patch - bv.patch;
        if (d != 0) return d;

        return 0;
    }
}
