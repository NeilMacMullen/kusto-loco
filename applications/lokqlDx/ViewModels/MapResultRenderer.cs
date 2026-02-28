using System.Globalization;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Mapsui.Styles;
using NotNullStrings;
using Color = Mapsui.Styles.Color;

namespace LokqlDx.ViewModels;

public class MapResultRenderer
{
    private readonly MapArtist _mapArtist;
    private readonly KustoSettingsProvider _settings;
    private readonly TooltipManager _tooltipManager;

    public MapResultRenderer(KustoSettingsProvider settings, MapArtist mapArtist, TooltipManager tooltipManager)
    {
        _settings = settings;
        _mapArtist = mapArtist;
        _tooltipManager = tooltipManager;
    }

    private Color ColorFromHex(string s, Color fallback)
    {
        if (s.StartsWith("#"))
            s = s.Substring(1);
        try
        {
            var elements = s.Chunk(2).ToArray();
            if (elements.Length < 3 || elements.Length > 4)
                return fallback;

            int Get(int index)
            {
                return int.Parse(elements[index], NumberStyles.AllowHexSpecifier);
            }

            var color = (elements.Length == 4)
                ? Color.FromArgb(Get(0), Get(1), Get(2), Get(3))
                : Color.FromArgb(0xff, Get(0), Get(1), Get(2));
            return color;
        }
        catch
        {
            return fallback;
        }
    }

    private Brush BrushFromHex(string s, Color fallback)
    
        => new Brush(ColorFromHex(s, fallback));

    public void Populate(KustoQueryResult result)
    {
        var latCol = FindColumn(result, "latitude,lat");
        var lonCol = FindColumn(result, "longitude,lon,lng");
        var sizeCol = FindColumn(result, "size,radius");
        var scaleCol = FindColumn(result, "pinScale");
        var pinFillCol = FindColumn(result, "pinFill");
        var seriesCol = FindColumn(result, "series");
        var symbolCol = FindColumn(result, "pinSymbol");
        var indexCol = FindColumn(result, "index,timestamp");
        var tooltipCol = FindColumn(result, "tooltip");
        var labelCol = FindColumn(result, "label");

        var defaultScale = _settings.GetDoubleOr("map.pin.scale", 0.3);

        var defaultSymbol =
            Enum.TryParse<SymbolType>(
                _settings.GetOr("map.pin.symbol", ""), true, out var sym)
                ? sym
                : SymbolType.Ellipse;
        var defaultPinFill = BrushFromHex(_settings.GetOr("map.pin.fill", String.Empty),
            Color.Red);

        var defaultPenColor = ColorFromHex(_settings.GetOr("map.line.pen.color", String.Empty), Color.Blue);

        //we need at least latitude and longitude
        if (!IsValid(latCol) || !IsValid(lonCol))
            return;

        var points = result.EnumerateRows()
            .Select(row => new
            {
                Geo = GeoPoint.Maybe(ValOr(row,latCol,double.NaN),ValOr(row,lonCol,double.NaN)),
                Radius = ValOr(row, sizeCol, 0.0),
                Tooltip = ValOr(row, tooltipCol, string.Empty),
                Label = ValOr(row, labelCol, string.Empty),
                Series = ValOr(row, seriesCol, string.Empty),
                Scale = ValOr(row, scaleCol, defaultScale),
                Symbol = ValOrSymbol(row, symbolCol, defaultSymbol),
                PinFill = ValOrBrush(row, pinFillCol, defaultPinFill),
                LinePen = defaultPenColor
            })
            .Where(g => g.Geo.Valid)
            .ToArray();

        foreach (var p in points)
        {
            var pin = _mapArtist.AddPin(p.Geo, p.Scale, p.Symbol, p.PinFill, p.Label);
            _tooltipManager.Add(pin, p.Tooltip);
        }

        if (IsValid(indexCol))
        {
            if (IsValid(seriesCol))
            {
                var groups = points.GroupBy(p => p.Series).ToArray();
                foreach (var d in groups)
                    _mapArtist.DrawLine(d.Select(p => p.Geo).ToArray()
                    ,defaultPenColor);
            }
            else
            {
                _mapArtist.DrawLine(points.Select(p => p.Geo).ToArray(),
                    defaultPenColor);
            }
        }
    }


    private static ColumnResult FindColumn(KustoQueryResult result, string names)
    {
        foreach (var name in names.Tokenize(","))
        {
            var matches = result.ColumnDefinitions()
                .Where(d => d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (matches.Any())
                return matches.First();
        }

        return new ColumnResult(string.Empty, -1, typeof(object));
    }

    protected static double ValOr(object?[] row, ColumnResult seriesCol, double fallback)
    {
        if (!IsValid(seriesCol))
            return fallback;
        var obj = row[seriesCol.Index];
        if (obj is null)
            return fallback;
        //these are the only types supported by kql
        if (seriesCol.UnderlyingType == typeof(double)
            || seriesCol.UnderlyingType == typeof(float)
            || seriesCol.UnderlyingType == typeof(long)
            || seriesCol.UnderlyingType == typeof(int)
            || seriesCol.UnderlyingType == typeof(decimal)
           )
            return Convert.ToDouble(obj);

        if (seriesCol.UnderlyingType == typeof(string)
            && double.TryParse((string)obj, out var d))
            return d;

        return fallback;
    }


    private static string ValOr(object?[] row, ColumnResult seriesCol, string fallback)
    {
        if (!IsValid(seriesCol))
            return fallback;
        return row[seriesCol.Index]?.ToString() ?? fallback;
    }

    private Brush ValOrBrush(object?[] row, ColumnResult column, Brush fallback)
    {
        if (!IsValid(column))
            return fallback;
        var str = row[column.Index]?.ToString();
        if (str == null)
            return
                fallback;
        var fallbackColor = fallback.Color!.Value;
        return BrushFromHex(str, fallbackColor);
    }


    private static SymbolType ValOrSymbol(object?[] row, ColumnResult seriesCol, SymbolType fallback)
    {
        if (!IsValid(seriesCol))
            return fallback;
        var val = row[seriesCol.Index]?.ToString();
        if (val == null) return fallback;
        return Enum.TryParse<SymbolType>(val, true, out var result)
            ? result
            : fallback;
    }

    private static bool IsValid(ColumnResult res)
        => res.Index >= 0;
}
