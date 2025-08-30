﻿using lokqlDxComponents.Services.Assets;

namespace LokqlDx;

internal static class ResourceHelper
{
    /// <summary>
    ///     Gets a resource name independent of namespace
    /// </summary>
    /// <remarks>
    ///     For some reason dotnet publish decides to lower-case the
    ///     namespace in the resource name. In any case, we really don't want to trust
    ///     that the namespace won't change so do a match against the filename
    /// </remarks>
    public static Stream SafeGetResourceStream(string substring) => App.Resolve<IAssetService>().Open(substring);
}
