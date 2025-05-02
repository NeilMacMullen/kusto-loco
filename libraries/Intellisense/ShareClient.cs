using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;

namespace Intellisense;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class ShareClient
{
    public static unsafe IEnumerable<ShareInfo> GetNetworkShares(string host = Constants.LocalHost)
    {
        var serverName = new PWSTR(Marshal.StringToHGlobalUni(host));

        var resumeHandle = (uint*)0;
        // Call NetShareEnum
        var status = PInvoke.NetShareEnum(
            serverName,
            1,
            out var bufPtr,
            uint.MaxValue,
            out var entriesRead,
            out _,
            resumeHandle
        );

        if (status is not 0)
        {
            throw new Exception($"NetShareEnum failed with error code {status}");
        }

        var bufferPtr = (SHARE_INFO_1*)bufPtr;

        var results = Enumerable
            .Range(0, (int)entriesRead)
            .Select(x => ShareInfo.Create(bufferPtr[x]))
            .ToArray();


        _ = PInvoke.NetApiBufferFree(bufferPtr);


        return results;
    }
}

internal readonly record struct ShareInfo(string Name, string Type, string Remark)
{
    public static ShareInfo Create(SHARE_INFO_1 info) => new(info.shi1_netname.ToString(),
        info.shi1_type.ToString(),
        info.shi1_remark.ToString()
    );
}
