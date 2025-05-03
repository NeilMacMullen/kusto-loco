using System.Diagnostics.CodeAnalysis;
using Vanara.PInvoke;

// ReSharper disable NotAccessedPositionalProperty.Global

namespace Intellisense.FileSystem.Shares;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class ShareClient
{
    public IEnumerable<ShareInfo> GetNetworkShares(string host) =>
        NetApi32
            .NetShareEnum<NetApi32.SHARE_INFO_1>(host)
            .Select(ShareInfo.Create);
}