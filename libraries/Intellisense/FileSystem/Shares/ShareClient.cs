using System.Diagnostics.CodeAnalysis;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

internal interface IShareClient
{
    IEnumerable<ShareInfo> GetNetworkShares(string host);
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class ShareClient : IShareClient
{
    public IEnumerable<ShareInfo> GetNetworkShares(string host) =>
        NetApi32
            .NetShareEnum<NetApi32.SHARE_INFO_1>(host)
            .Select(CreateShareInfo);

    private static ShareInfo CreateShareInfo(NetApi32.SHARE_INFO_1 info) => new()
    {
        Name = info.shi1_netname,
        Type = info.shi1_type.ToString(),
        Remark = info.shi1_remark ?? string.Empty
    };
}
