using System.Diagnostics.CodeAnalysis;
using Vanara.PInvoke;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace Intellisense.FileSystem;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class ShareClient
{
    public IEnumerable<ShareInfo> GetNetworkShares(string host) =>
        NetApi32
            .NetShareEnum<NetApi32.SHARE_INFO_1>(host)
            .Select(ShareInfo.Create);
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal readonly record struct ShareInfo(string Name, string Type, string Remark)
{
    public static ShareInfo Create(NetApi32.SHARE_INFO_1 info) => new(
        info.shi1_netname,
        info.shi1_type.ToString(),
        info.shi1_remark ?? ""
    );
}
