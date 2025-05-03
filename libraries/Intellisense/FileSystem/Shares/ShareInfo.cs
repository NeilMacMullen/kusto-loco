using System.Diagnostics.CodeAnalysis;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal readonly record struct ShareInfo(string Name, string Type, string Remark)
{
    public static ShareInfo Create(NetApi32.SHARE_INFO_1 info) => new(
        info.shi1_netname,
        info.shi1_type.ToString(),
        info.shi1_remark ?? ""
    );
}
