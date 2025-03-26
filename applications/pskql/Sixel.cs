// See https://aka.ms/new-console-template for more information
using Microsoft.VisualBasic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

public static class Sixel
{
    /// <summary>
    /// Converts an image to a Sixel string.
    /// </summary>
    /// <param name="image">The image to convert.</param>
    /// <param name="cellWidth">The width of the cell in terminal cells.</param>
    /// <param name="maxColors">The Max colors of the image.</param>
    /// <param name="frame">The frame to convert.</param>
    /// <param name="returnCursorToTopLeft">Whether to return the cursor to the top left after rendering the image.</param>
    /// <returns>The Sixel string.</returns>
    public static string ImageToSixel(Image<Rgba32> image, int maxColors, int cellWidth, int frame = 0, bool returnCursorToTopLeft = false)
    {
        // get image size in characters
        var cellSize = Compatibility.GetCellSize();
        // get the image size in console characters
        var imageSize = SizeHelper.GetTerminalImageSize(image.Width, image.Height, cellWidth);

        image.Mutate(ctx =>
        {
            // Some math to get the target size in pixels and reverse it to cell height that it will consume.
            var targetPixelWidth = imageSize.Width * cellSize.PixelWidth;
            var targetPixelHeight = imageSize.Height * cellSize.PixelHeight;

            if (image.Width != targetPixelWidth || image.Height != targetPixelHeight)
            {
                // Resize the image to the target size
                ctx.Resize(new ResizeOptions()
                {
                    // https://en.wikipedia.org/wiki/Bicubic_interpolation
                    // quality goes Bicubic > Bilinear > NearestNeighbor
                    Sampler = KnownResamplers.Bicubic,
                    Size = new(targetPixelWidth, targetPixelHeight),
                    PremultiplyAlpha = false,
                });
            }
            // Sixel supports 256 colors max
            ctx.Quantize(new OctreeQuantizer(new()
            {
                MaxColors = maxColors,
            }));
        });
        var targetFrame = image.Frames[frame];
        return FrameToSixelString(targetFrame);
    }
    internal static string FrameToSixelString(ImageFrame<Rgba32> frame)
    {
        var sixelBuilder = new StringBuilder();
        var sixel = new StringBuilder();
        var palette = new Dictionary<Rgba32, int>();
        var colorCounter = 1;
        sixel.StartSixel(frame.Width, frame.Height);
        frame.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);
                // The value of 1 left-shifted by the remainder of the current row divided by 6 gives the correct sixel character offset from the empty sixel char for each row.
                // See the description of s...s for more detail on the sixel format https://vt100.net/docs/vt3xx-gp/chapter14.html#S14.2.1
                var c = (char)(Constants.SixelTransparent + (1 << (y % 6)));
                var lastColor = -1;
                var repeatCounter = 0;
                foreach (ref var pixel in pixelRow)
                {

                    // The colors can be added to the palette and interleaved with the sixel data so long as the color is defined before it is used.
                    if (!palette.TryGetValue(pixel, out var colorIndex))
                    {
                        colorIndex = colorCounter++;
                        palette[pixel] = colorIndex;
                        sixel.AddColorToPalette(pixel, colorIndex);
                    }

                    // Transparency is a special color index of 0 that exists in our sixel palette.
                    var colorId = pixel.A == 0 ? 0 : colorIndex;

                    // Sixel data will use a repeat entry if the color is the same as the last one.
                    // https://vt100.net/docs/vt3xx-gp/chapter14.html#S14.3.1
                    if (colorId == lastColor || repeatCounter == 0)
                    {
                        // If the color was repeated go to the next loop iteration to check the next pixel.
                        lastColor = colorId;
                        repeatCounter++;
                        continue;
                    }

                    // Every time the color is not repeated the previous color is written to the string.
                    sixelBuilder.AppendSixel(lastColor, repeatCounter, c);

                    // Remember the current color and reset the repeat counter.
                    lastColor = colorId;
                    repeatCounter = 1;
                }

                // Write the last color and repeat counter to the string for the current row.
                sixelBuilder.AppendSixel(lastColor, repeatCounter, c);

                // Add a carriage return at the end of each row and a new line every 6 pixel rows.
                sixelBuilder.AppendCarriageReturn();
                if (y % 6 == 5)
                {
                    sixelBuilder.AppendNextLine();
                }
            }
        });
        sixelBuilder.AppendNextLine();
        sixelBuilder.AppendExitSixel();

        return sixel.Append(sixelBuilder).ToString();
    }

    private static void AddColorToPalette(this StringBuilder sixelBuilder, Rgba32 pixel, int colorIndex)
    {
        // rgb 0-255 needs to be translated to 0-100 for sixel.
        var (r, g, b) = (
            (int)pixel.R * 100 / 255,
            (int)pixel.G * 100 / 255,
            (int)pixel.B * 100 / 255
        );

        sixelBuilder.Append(Constants.SixelColorStart)
                    .Append(colorIndex)
                    .Append(Constants.SixelColorParam)
                    .Append(r)
                    .Append(Constants.Divider)
                    .Append(g)
                    .Append(Constants.Divider)
                    .Append(b);
    }
    private static void AppendSixel(this StringBuilder sixelBuilder, int colorIndex, int repeatCounter, char sixel)
    {
        if (colorIndex == 0)
        {
            // Transparent pixels are a special case and are always 0 in the palette.
            sixel = Constants.SixelTransparent;
        }
        if (repeatCounter <= 1)
        {
            // single entry
            sixelBuilder.Append(Constants.SixelColorStart)
                        .Append(colorIndex)
                        .Append(sixel);
        }
        else
        {
            // add repeats
            sixelBuilder.Append(Constants.SixelColorStart)
                        .Append(colorIndex)
                        .Append(Constants.SixelRepeat)
                        .Append(repeatCounter)
                        .Append(sixel);
        }
    }
    private static void AppendCarriageReturn(this StringBuilder sixelBuilder)
    {
        sixelBuilder.Append(Constants.SixelDECGCR);
    }

    private static void AppendNextLine(this StringBuilder sixelBuilder)
    {
        sixelBuilder.Append(Constants.SixelDECGNL);
    }

    private static void AppendExitSixel(this StringBuilder sixelBuilder)
    {
        sixelBuilder.Append(Constants.ST);
    }

    private static void StartSixel(this StringBuilder sixelBuilder, int width, int height)
    {
        sixelBuilder.Append(Constants.SixelStart)
                    .Append(Constants.SixelRaster)
                    .Append(width)
                    .Append(Constants.Divider)
                    .Append(height)
                    .Append(Constants.SixelTransparentColor);
    }
}


public static class Compatibility
{
    /// <summary>
    /// Memory-caches the result of the terminal supporting sixel graphics.
    /// </summary>
    internal static bool? _terminalSupportsSixel;

    /// <summary>
    /// Check if the terminal supports kitty graphics
    /// </summary>
    internal static bool? _terminalSupportsKitty;

    /// <summary>
    /// Memory-caches the result of the terminal cell size.
    /// </summary>
    private static CellSize? _cellSize;

    /// <summary>
    /// get the terminal info
    /// </summary>
    private static TerminalInfo? _terminalInfo;

    /// <summary>
    /// Window size in pixels
    /// </summary>
    // private static WindowSizePixels? _windowSizePixels;

    /// <summary>
    /// Window size in characters
    /// </summary>
    // private static WindowSizeCharacters? _windowSizeCharacters;

    /// <summary>
    /// Get the response to a control sequence.
    /// </summary>
    public static string GetControlSequenceResponse(string controlSequence)
    {
        char? c;
        var response = string.Empty;

        // Console.Write($"{Constants.ESC}{controlSequence}{Constants.ST}");
        Console.Write($"{Constants.ESC}{controlSequence}");
        do
        {
            c = Console.ReadKey(true).KeyChar;
            response += c;
        } while (c != 'c' && Console.KeyAvailable);

        return response;
    }

    /// <summary>
    /// Get the cell size of the terminal in pixel-sixel size.
    /// The response to the command will look like [6;20;10t where the 20 is height and 10 is width.
    /// I think the 6 is the terminal class, which is not used here.
    /// </summary>
    /// <returns>The number of pixel sixels that will fit in a single character cell.</returns>
    public static CellSize GetCellSize()
    {
        if (_cellSize != null)
        {
            return _cellSize;
        }

        var response = GetControlSequenceResponse("[16t");

        try
        {
            var parts = response.Split(';', 't');
            _cellSize = new CellSize
            {
                PixelWidth = int.Parse(parts[2]),
                PixelHeight = int.Parse(parts[1])
            };
        }
        catch
        {
            // Return the default Windows Terminal size if we can't get the size from the terminal.
            _cellSize = new CellSize
            {
                PixelWidth = 10,
                PixelHeight = 20
            };
        }
        return _cellSize;
    }

    /// <summary>
    /// Get the window size in pixels
    /// </summary>
    /// <returns>WindowSizePixels</returns>
    // public static WindowSizePixels GetWindowSizePixels()
    // {
    //   // this class should be able to re-run, people can resize the terminal
    //   // so should not cache the result.. hopefully this is not too slow
    //   var response14 = GetControlSequenceResponse("[14t");
    //   try
    //   {
    //     var parts14 = response14.Split(';', 't');
    //     _windowSizePixels = new WindowSizePixels
    //     {
    //       PixelWidth = int.Parse(parts14[2]),
    //       PixelHeight = int.Parse(parts14[1]),
    //     };
    //   }
    //   catch
    //   {
    //     _windowSizePixels = new WindowSizePixels
    //     {
    //       PixelWidth = 0,
    //       PixelHeight = 0
    //     };
    //   }
    //   return _windowSizePixels;
    // }

    /// <summary>
    /// Get the window size in characters
    /// </summary>
    /// <returns>WindowSizeCharacters</returns>
    // public static WindowSizeCharacters GetWindowSizeCharacters()
    // {
    //   // this class should be able to re-run, people can resize the terminal
    //   // so should not cache the result.. hopefully this is not too slow
    //   var response18 = GetControlSequenceResponse("[18t");
    //   try
    //   {
    //     var parts18 = response18.Split(';', 't');
    //     _windowSizeCharacters = new WindowSizeCharacters
    //     {
    //       CharacterWidth = int.Parse(parts18[2]),
    //       CharacterHeight = int.Parse(parts18[1]),
    //     };
    //   }
    //   catch {
    //     _windowSizeCharacters = new WindowSizeCharacters
    //     {
    //       CharacterWidth = 0,
    //       CharacterHeight = 0
    //     };
    //   }
    //   return _windowSizeCharacters;
    // }

    /// <summary>
    /// Check if the terminal supports sixel graphics.
    /// This is done by sending the terminal a Device Attributes request.
    /// If the terminal responds with a response that contains ";4;" then it supports sixel graphics.
    /// https://vt100.net/docs/vt510-rm/DA1.html
    /// </summary>
    /// <returns>True if the terminal supports sixel graphics, false otherwise.</returns>
    public static bool TerminalSupportsSixel()
    {
        if (_terminalSupportsSixel.HasValue)
        {
            return _terminalSupportsSixel.Value;
        }
        _terminalSupportsSixel = GetControlSequenceResponse("[c").Contains(";4;");
        return _terminalSupportsSixel.Value;
    }

    /// <summary>
    /// Check if the terminal supports kitty graphics.
    /// https://sw.kovidgoyal.net/kitty/graphics-protocol/
    /// response: ␛_Gi=31;OK␛\␛[?62;c
    /// </summary>
    /// <returns>True if the terminal supports sixel graphics, false otherwise.</returns>
    public static bool TerminalSupportsKitty()
    {
        if (_terminalSupportsKitty.HasValue)
        {
            return _terminalSupportsKitty.Value;
        }
        string kittyTest = $"_Gi=31,s=1,v=1,a=q,t=d,f=24;AAAA{Constants.ST}{Constants.ESC}[c";
        // string kittyTest = $"_Gi=31,s=1,v=1,a=q,t=d,f=24;AAAA{Constants.ESC}\\";
        _terminalSupportsKitty = GetControlSequenceResponse(kittyTest).Contains(";OK");
        return _terminalSupportsKitty.Value;
    }

    /// <summary>
    /// Get the terminal info
    /// </summary>
    /// <returns>The terminal protocol</returns>
    public static TerminalInfo GetTerminalInfo()
    {
        if (_terminalInfo != null)
        {
            return _terminalInfo;
        }

        _terminalInfo = TerminalChecker.CheckTerminal();
        return _terminalInfo;
    }
}

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

public static class SizeHelper
{
    public static Size GetTerminalImageSize(Image<Rgba32> image, int? maxWidth = null)
    {
        return GetTerminalImageSize(image.Width, image.Height, maxWidth);
    }
    public static Size GetTerminalImageSize(Stream image, int? maxWidth = null)
    {
        using var _image = Image.Load<Rgba32>(image);
        return GetTerminalImageSize(_image.Width, _image.Height, maxWidth);
    }

    public static Size GetTerminalImageSize(int Width, int Height, int? maxWidth = null)
    {
        var cellSize = Compatibility.GetCellSize();

        // calculate natural dimensions in cells (rounded down to ensure fit)
        var naturalCellWidth = (int)Math.Floor((double)Width / cellSize.PixelWidth);
        var naturalCellHeight = (int)Math.Floor((double)Height / cellSize.PixelHeight);

        // If no maxWidth specified, constrain to console width
        if (!maxWidth.HasValue || maxWidth.Value <= 0)
        {
            var targetCellWidth = Math.Min(naturalCellWidth, Console.WindowWidth - 2);
            if (targetCellWidth == naturalCellWidth)
            {
                return new Size(naturalCellWidth, naturalCellHeight);
            }

            // Calculate new height maintaining aspect ratio
            var targetPixelWidth = targetCellWidth * cellSize.PixelWidth;
            var targetPixelHeight = (int)Math.Round((double)Height / Width * targetPixelWidth);

            // round down height to nearest Sixel boundary (6 pixels)
            targetPixelHeight -= targetPixelHeight % 6;

            var targetCellHeight = (int)Math.Floor((double)targetPixelHeight / cellSize.PixelHeight);
            return new Size(targetCellWidth, targetCellHeight);
        }

        // maxWidth specified - honor it regardless of natural size
        var requestedCellWidth = maxWidth.Value;
        var requestedPixelWidth = requestedCellWidth * cellSize.PixelWidth;
        var requestedPixelHeight = (int)Math.Round((double)Height / Width * requestedPixelWidth);

        // round to sixel boundary
        requestedPixelHeight -= requestedPixelHeight % 6;

        var requestedCellHeight = (int)Math.Floor((double)requestedPixelHeight / cellSize.PixelHeight);
        return new Size(requestedCellWidth, requestedCellHeight);
    }

    public static Size GetImageSizeMax(int Width, int Height, int? maxWidth = null)
    {
        // this doesnt allow resize to bigger, will only scale down
        var cellSize = Compatibility.GetCellSize();

        // calculate natural dimensions in cells (rounded down to ensure fit)
        var naturalCellWidth = (int)Math.Floor((double)Width / cellSize.PixelWidth);
        var naturalCellHeight = (int)Math.Floor((double)Height / cellSize.PixelHeight);

        // adjust width if needed by maxWidth
        var targetCellWidth = maxWidth.HasValue && maxWidth.Value > 0
            ? Math.Min(naturalCellWidth, maxWidth.Value)
            : Math.Min(naturalCellWidth, Console.WindowWidth - 2);

        if (targetCellWidth == naturalCellWidth)
        {
            // No resize needed
            return new Size(naturalCellWidth, naturalCellHeight);
        }

        // Calculate new height maintaining aspect ratio
        var targetPixelWidth = targetCellWidth * cellSize.PixelWidth;
        var targetPixelHeight = (int)Math.Round((double)Height / Width * targetPixelWidth);

        // round down height to nearest Sixel boundary (6 pixels)
        targetPixelHeight -= targetPixelHeight % 6;

        var targetCellHeight = (int)Math.Floor((double)targetPixelHeight / cellSize.PixelHeight);

        return new Size(targetCellWidth, targetCellHeight);
    }
}

/// <summary>
/// Represents the size of a cell in pixels for sixel rendering.
/// </summary>
public class CellSize
{
    /// <summary>
    /// Gets the width of a cell in pixels.
    /// </summary>
    public int PixelWidth { get; set; }

    /// <summary>
    /// Gets the height of a cell in pixels.
    /// This isn't used for anything yet but this would be required for something like spectre console that needs to work around the size of the rendered sixel image.
    /// </summary>
    public int PixelHeight { get; set; }
}



public static class TerminalChecker
{
    /// <summary>
    /// Check the terminal for compatibility.
    /// use enviroment variables to try and figure out which terminal is being used.
    /// </summary>
    internal static TerminalInfo CheckTerminal()
    {
        var env = Environment.GetEnvironmentVariables();
        TerminalInfo? envTerminalInfo = null;

        // First check environment variables to identify the terminal
        foreach (DictionaryEntry item in env)
        {
            var key = item.Key?.ToString();
            var value = item.Value?.ToString();

            if (key == "TERM_PROGRAM" && value != null && Helpers.GetTerminal(value) is Terminals terminal)
            {
                if (Helpers.SupportedProtocol.TryGetValue(terminal, out var protocol))
                {
                    envTerminalInfo = new TerminalInfo
                    {
                        Terminal = terminal,
                        Protocol = protocol
                    };
                    break;
                }
            }

            if (key != null && Helpers.GetTerminal(key) is Terminals _terminal)
            {
                if (Helpers.SupportedProtocol.TryGetValue(_terminal, out var protocol))
                {
                    envTerminalInfo = new TerminalInfo
                    {
                        Terminal = _terminal,
                        Protocol = protocol
                    };
                    break;
                }
            }
        }

        // Then check VT capabilities and override protocol if supported
        if (Compatibility._terminalSupportsKitty ?? false)
        {
            return new TerminalInfo
            {
                Terminal = envTerminalInfo?.Terminal ?? Terminals.Kitty,
                Protocol = ImageProtocol.KittyGraphicsProtocol
            };
        }

        if (Compatibility._terminalSupportsSixel ?? false)
        {
            return new TerminalInfo
            {
                Terminal = envTerminalInfo?.Terminal ?? Terminals.MicrosoftTerminal,
                Protocol = ImageProtocol.Sixel
            };
        }

        // Return environment-detected terminal or fallback to unknown
        return envTerminalInfo ?? new TerminalInfo
        {
            Terminal = Terminals.unknown,
            Protocol = ImageProtocol.Blocks
        };
    }
    internal static TerminalInfo CheckTerminal(Terminals terminal)
    {
        if (Helpers.SupportedProtocol.TryGetValue(terminal, out var protocol))
        {
            return new TerminalInfo
            {
                Terminal = terminal,
                Protocol = protocol
            };
        }
        return new TerminalInfo
        {
            Terminal = Terminals.unknown,
            Protocol = ImageProtocol.Blocks
        };
    }
}

public class TerminalInfo
{
    public Terminals Terminal { get; set; }
    public ImageProtocol Protocol { get; set; }
}
[Flags]
public enum ImageProtocol
{
    Blocks = 0,
    Sixel = 1,
    InlineImageProtocol = 2,
    iTerm2 = 4, // Alias for InlineImageProtocol
    KittyGraphicsProtocol = 8
};


/// <summary>
/// known terminals
/// not all terminals are supported.
/// </summary>
public enum Terminals
{
    MicrosoftTerminal,
    MicrosoftConhost,
    Kitty,
    Iterm2,
    WezTerm,
    Ghostty,
    VSCode,
    Mintty,
    Alacritty,
    xterm,
    mlterm,
    unknown
};


internal partial class Helpers
{
    /// <summary>
    /// mapping of environment variables to terminal.
    /// used for detecting the terminal.
    /// </summary>
    private static readonly Dictionary<Terminals, string> _lookup;
    private static readonly Dictionary<string, Terminals> _reverseLookup;
    static Helpers()
    {
        _lookup = new Dictionary<Terminals, string>
        {
            { Terminals.MicrosoftTerminal, "WT_SESSION" },
            // { Terminals.MicrosoftConhost, "SESSIONNAME" },
            { Terminals.Kitty, "KITTY_WINDOW_ID" },
            { Terminals.Iterm2, "ITERM_SESSION_ID" },
            { Terminals.WezTerm, "WEZTERM_CONFIG_FILE" },
            { Terminals.Ghostty, "GHOSTTY_RESOURCES_DIR" },
            { Terminals.VSCode, "TERM_PROGRAM" },
            { Terminals.Mintty, "MINTTY" },
            { Terminals.Alacritty, "ALACRITTY_LOG" }
        };
        _reverseLookup = new Dictionary<string, Terminals>(StringComparer.OrdinalIgnoreCase);
        foreach (var (terminal, envVar) in _lookup)
        {
            if (!_reverseLookup.ContainsKey(envVar))
            {
                _reverseLookup[envVar] = terminal;
            }
        }
    }
    internal static Terminals? GetTerminal(string str)
    {
        Terminals _terminal;
        if (_reverseLookup.TryGetValue(str, out _terminal))
        {
            return _terminal;
        }
        if (Enum.TryParse<Terminals>(str, true, out _terminal))
        {
            return _terminal;
        }
        return null;
    }
    internal static string? GetEnvironmentVariable(Terminals terminal)
    {
        if (_lookup.TryGetValue(terminal, out var _envVar))
        {
            return _envVar;
        }
        return null;
    }
}

internal partial class Helpers
{
    /// <summary>
    ///  mapping of terminals to the image protocol they support.
    /// </summary>
    internal static readonly Dictionary<Terminals, ImageProtocol> SupportedProtocol = new Dictionary<Terminals, ImageProtocol>()
    {
        { Terminals.MicrosoftTerminal, ImageProtocol.Sixel },
        { Terminals.MicrosoftConhost, ImageProtocol.Sixel },
        { Terminals.Kitty, ImageProtocol.KittyGraphicsProtocol },
        { Terminals.Iterm2, ImageProtocol.InlineImageProtocol },
        { Terminals.WezTerm, ImageProtocol.InlineImageProtocol },
        { Terminals.Ghostty, ImageProtocol.KittyGraphicsProtocol },
        { Terminals.VSCode, ImageProtocol.InlineImageProtocol },
        { Terminals.Mintty, ImageProtocol.InlineImageProtocol },
        { Terminals.Alacritty, ImageProtocol.Blocks },
        { Terminals.xterm, ImageProtocol.InlineImageProtocol },
        { Terminals.mlterm, ImageProtocol.Sixel },
        { Terminals.unknown, ImageProtocol.Blocks }
    };
}
