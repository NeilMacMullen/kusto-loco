
namespace KustoLoco.Rendering.SixelSupport;
/// <summary>
/// Sixel terminal compatibility helpers.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The character to use when entering a terminal escape code sequence.
    /// </summary>
    internal const string ESC = "\u001b";

    /// <summary>
    /// The character to indicate the start of a sixel color palette entry or to switch to a new color.
    /// </summary>
    internal const char SixelColorStart = '#';

    /// <summary>
    /// The character to use when a sixel is empty/transparent.
    /// </summary>
    internal const char SixelTransparent = '?';

    /// <summary>
    /// The character to use when entering a repeated sequence of a color in a sixel.
    /// </summary>
    internal const char SixelRepeat = '!';

    /// <summary>
    /// The character to use when moving to the next line in a sixel.
    /// </summary>
    internal const char SixelDECGNL = '-';

    /// <summary>
    /// The character to use when going back to the start of the current line in a sixel to write more data over it.
    /// </summary>
    internal const char SixelDECGCR = '$';

    /// <summary>
    /// divider
    /// </summary>
    internal const char Divider = ';';

    /// <summary>
    /// The start of a sixel sequence.
    /// evaluate $"{ESC}P0;1;0q"; which chafa uses
    /// </summary>
    internal const string SixelStart = $"{ESC}P0;1q";

    /// <summary>
    /// The raster settings for setting the sixel pixel ratio to 1:1 so images are square when rendered instead of the 2:1 double height default.
    /// </summary>
    internal const string SixelRaster = "\"1;1;";

    /// <summary>
    /// The end of a sixel sequence.
    /// ST is the string terminator for sixel.
    /// </summary>
    internal const string ST = $"{ESC}\\";

    /// <summary>
    /// color parameter
    /// </summary>
    internal const string SixelColorParam = ";2;";

    /// <summary>
    /// The transparent color for the sixel, this is red but the sixel should be transparent so this is not visible.
    /// </summary>
    internal const string SixelTransparentColor = "#0;2;0;0;0";

}
