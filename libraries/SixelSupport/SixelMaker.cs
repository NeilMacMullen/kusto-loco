using System.Text;

namespace KustoLoco.Rendering.SixelSupport;
public static class SixelMaker
{
    public static string FrameToSixelString(IPixelSource frame)
    {
        //ensure the frame is quantized
        frame = new QuantizedPixelSource(frame);

        var sixelBuilder = new StringBuilder();
        var sixel = new StringBuilder();
        var palette = new Dictionary<Pixel, int>();
        var colorCounter = 1;
        sixel.StartSixel(frame.Width, frame.Height);

        for (var y = 0; y < frame.Height; y++)
        {
            var lastColor = -1;
            var repeatCounter = 0;
            var subRow = y % 6;

            var c = (char)(Constants.SixelTransparent + (1 << subRow));
            for (var x = 0; x < frame.Width; x++)
            {
                // The value of 1 left-shifted by the remainder of the current row divided by 6 gives the correct sixel character offset from the empty sixel char for each row.
                // See the description of s...s for more detail on the sixel format https://vt100.net/docs/vt3xx-gp/chapter14.html#S14.2.1
                var pixel = frame.GetPixel(x, y);

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
            if (subRow== 5) sixelBuilder.AppendNextLine();
        }

        sixelBuilder.AppendNextLine();
        sixelBuilder.AppendExitSixel();
        return sixel.Append(sixelBuilder).ToString();
    }

    private static void AddColorToPalette(this StringBuilder sixelBuilder, Pixel pixel, int colorIndex)
    {
        //Console.WriteLine($"Adding color {pixel.R} {pixel.G} {pixel.B} {pixel.A}");
        // rgb 0-255 needs to be translated to 0-100 for sixel.
        var (r, g, b) = (
            pixel.R * 100 / 255,
            pixel.G * 100 / 255,
            pixel.B * 100 / 255
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
            // Transparent pixels are a special case and are always 0 in the palette.
            sixel = Constants.SixelTransparent;
        if (repeatCounter <= 1)
            // single entry
            sixelBuilder.Append(Constants.SixelColorStart)
                .Append(colorIndex)
                .Append(sixel);
        else
            // add repeats
            sixelBuilder.Append(Constants.SixelColorStart)
                .Append(colorIndex)
                .Append(Constants.SixelRepeat)
                .Append(repeatCounter)
                .Append(sixel);
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
