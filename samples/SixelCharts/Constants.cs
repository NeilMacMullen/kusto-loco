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

    /// <summary>
    /// inline image protocol start
    /// </summary>
    internal const string InlineImageStart = $"{ESC}]";

    /// <summary>
    /// move 1 character row down
    /// </summary>
    internal const string AddEmptyRow = $"{ESC}[1B";

    /// <summary>
    /// inline image protocol end
    /// </summary>
    internal const char InlineImageEnd = (char)7;

    /// <summary>
    /// kitty start sequence
    /// </summary>
    internal const string KittyStart = $"{ESC}_G";

    /// <summary>
    /// Kitty raster
    /// </summary>
    internal const string KittyPos = "a=T,f=100,";

    /// <summary>
    /// Kitty more chunks
    /// </summary>
    internal const string KittyMore = "m=1";

    /// <summary>
    /// Kitty last chunk
    /// </summary>
    internal const string KittyFinish = "m=0";

    /// <summary>
    /// Kitty chunksize
    /// </summary>
    internal const int KittychunkSize = 4096;

    /// <summary>
    /// lower half block character
    /// ▄
    /// this allows you to color the top and bottom of a cell.
    /// foreground colors the lower block and background colors the space above the block in the same cell.
    /// </summary>
    internal const char LowerHalfBlock = '\u2584';

    /// <summary>
    /// upper half block character
    /// ▀
    /// this allows you to color the top and bottom of a cell.
    /// foreground colors the upper block and background colors the space below the block in the same cell.
    /// </summary>
    internal const char UpperHalfBlock = '\u2580';

    /// <summary>
    /// background color escape sequence
    /// </summary>
    internal const string VTBG = "[48;2;";

    /// <summary>
    /// foreground color escape sequence
    /// </summary>
    internal const string VTFG = "[38;2;";

    /// <summary>
    /// hide cursor
    /// </summary>
    internal const string HideCursor = $"{ESC}[?25l";
    /// <summary>
    /// show cursor
    /// </summary>
    /// <summary>
    internal const string ShowCursor = $"{ESC}[?25h";

    /// <summary>
    /// chafa compatibility string
    /// </summary>
    internal const string xtermHideCursor = $"{ESC}[?8452l";
    /// <summary>
    /// Adds VT sequence SM ? 8452 h / RM ? 8452 l for enabling/disabling sixel cursor placement conformance (xterm extension).
    /// </summary>
    internal const string xtermShowCursor = $"{ESC}[?8452h";
    /// <summary>
    /// DECSDM (sixel display mode)
    /// Nota bene: this reference has the sense for DECSDM (sixel display mode) reversed!
    // The actual behaviour of the VT340 is that when DECSDM is reset (the default), sixel scrolling is enabled.
    /// https://github.com/hackerb9/lsix/blob/master/README.md
    /// </summary>
    // internal const string DECSDM = $"{ESC}[?80L";
}