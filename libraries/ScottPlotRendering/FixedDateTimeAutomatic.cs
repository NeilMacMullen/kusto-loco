using ScottPlot;
using ScottPlot.TickGenerators;
using SkiaSharp;
using TimeUnits = ScottPlot.TickGenerators.TimeUnits;

namespace KustoLoco.Rendering.ScottPlot;

/// <summary>
///     TODO - temporary implementation until fixed in ScottPlot
/// </summary>
public class FixedDateTimeAutomatic : IDateTimeTickGenerator
{
    private static readonly List<ITimeUnit> TheseTimeUnits =
    [
        new TimeUnits.Millisecond(),
        new TimeUnits.Centisecond(),
        new TimeUnits.Decisecond(),
        new TimeUnits.Second(),
        new TimeUnits.Minute(),
        new TimeUnits.Hour(),
        new TimeUnits.Day(),
        new TimeUnits.Month(),
        new TimeUnits.Year()
    ];

    /// <summary>
    ///     If assigned, this function will be used to create tick labels
    /// </summary>
    public Func<DateTime, string>? LabelFormatter { get; set; } = null;

    public ITimeUnit? TimeUnit { get; private set; }

    public Tick[] Ticks { get; set; } = [];

    public int MaxTickCount { get; set; } = 10_000;

    public void Regenerate(CoordinateRange range, Edge edge, PixelLength size, SKPaint paint, LabelStyle labelStyle)
    {
        if (range.Span >= TimeSpan.MaxValue.Days || double.IsNaN(range.Span) || double.IsInfinity(range.Span) ||
            size.Length <= 0)
        {
            // cases of extreme zoom (10,000 years)
            Ticks = [];
            return;
        }

        var span = TimeSpan.FromDays(range.Span);
        var timeUnit = GetAppropriateTimeUnit(span);

        // estimate the size of the largest tick label for this unit this unit
        var maxExpectedTickLabelWidth = (int)Math.Max(16, span.TotalDays / MaxTickCount);
        var tickLabelHeight = 12;
        PixelSize tickLabelBounds = new(maxExpectedTickLabelWidth, tickLabelHeight);
        var coordinatesPerPixel = range.Span / size.Length;

        while (true)
        {
            // determine the ideal spacing to use between ticks
            var increment = coordinatesPerPixel * tickLabelBounds.Width / timeUnit.MinSize.TotalDays;
            var niceIncrement = LeastMemberGreaterThan(increment, timeUnit.Divisors);
            if (niceIncrement is null)
            {
                timeUnit = TheseTimeUnits.FirstOrDefault(t => t.MinSize > timeUnit.MinSize);
                if (timeUnit is not null)
                    continue;
                timeUnit = TheseTimeUnits.Last();
                niceIncrement = (int)Math.Ceiling(increment);
            }

            TimeUnit = timeUnit;

            // attempt to generate the ticks given these conditions
            var (ticks, largestTickLabelSize) =
                GenerateTicks(range, timeUnit, niceIncrement.Value, tickLabelBounds, paint, labelStyle);

            // if ticks were returned, use them
            if (ticks is not null)
            {
                Ticks = [.. ticks];
                return;
            }

            // if no ticks were returned it means the conditions were too dense and tick labels
            // overlapped, so expand the tick label bounds and try again.
            if (largestTickLabelSize is not null)
            {
                tickLabelBounds = tickLabelBounds.Max(largestTickLabelSize.Value);
                tickLabelBounds = new PixelSize(tickLabelBounds.Width + 10, tickLabelBounds.Height + 10);
                continue;
            }

            throw new InvalidOperationException($"{nameof(ticks)} and {nameof(largestTickLabelSize)} are both null");
        }
    }

    public IEnumerable<double> ConvertToCoordinateSpace(IEnumerable<DateTime> dates) =>
        dates.Select(NumericConversion.ToNumber);

    private ITimeUnit GetAppropriateTimeUnit(TimeSpan timeSpan, int targetTickCount = 10)
    {
        foreach (var timeUnit in TheseTimeUnits)
        {
            var estimatedUnitTicks = timeSpan.Ticks / timeUnit.MinSize.Ticks;
            foreach (var increment in timeUnit.Divisors)
            {
                var estimatedTicks = estimatedUnitTicks / increment;
                if (estimatedTicks > targetTickCount / 3 && estimatedTicks < targetTickCount * 3)
                    return timeUnit;
            }
        }

        return TheseTimeUnits.Last();
    }

    private ITimeUnit GetLargerTimeUnit(ITimeUnit timeUnit)
    {
        for (var i = 0; i < TheseTimeUnits.Count - 1; i++)
            if (timeUnit.GetType() == TheseTimeUnits[i].GetType())
                return TheseTimeUnits[i + 1];

        return TheseTimeUnits.Last();
    }

    private int? LeastMemberGreaterThan(double value, IReadOnlyList<int> list)
    {
        for (var i = 0; i < list.Count; i++)
            if (list[i] > value)
                return list[i];
        return null;
    }

    /// <summary>
    ///     This method attempts to find an ideal set of ticks.
    ///     If all labels fit within the bounds, the list of ticks is returned.
    ///     If a label doesn't fit in the bounds, the list is null and the size of the large tick label is returned.
    /// </summary>
    private (List<Tick>? Positions, PixelSize? PixelSize) GenerateTicks(CoordinateRange range, ITimeUnit unit,
        int increment, PixelSize tickLabelBounds, SKPaint paint, LabelStyle labelStyle)
    {
        var rangeMin = NumericConversion.ToDateTime(range.Min);
        var rangeMax = NumericConversion.ToDateTime(range.Max);

        // range.Min could be anything, but when calculating start it must be "snapped" to the best tick
        var start = GetLargerTimeUnit(unit).Snap(rangeMin);

        start = unit.Next(start, -increment);

        List<Tick> ticks = [];

        // if the increment is 0 or negative, something has gone wrong further up but bail out now.
        // Also check that unit.Next is actually going to move us forward... otherwise we could loop forever
        if (increment <= 0 || unit.Next(start, increment) <= start)
            return (ticks, null);

        const int maxTickCount = 1000;
        for (var dt = start; dt <= rangeMax; dt = unit.Next(dt, increment))
        {
            //this test is dangerous because it can cause an infinite loop if dt is not
            //advancing.  It might be better to initialize dt to Max(start,rangeMin) but I'm guessing the#
            //intentions of having this here is to allow the "preroll" to advance so that the first
            //tick is beyond rangeMin ?
            if (dt < rangeMin)
                continue;

            var tickLabel = LabelFormatter is null
                ? dt.ToString(unit.GetDateTimeFormatString())
                : LabelFormatter(dt);

            var tickLabelSize = labelStyle.Measure(tickLabel, paint).Size;

            var tickLabelIsTooLarge = !tickLabelBounds.Contains(tickLabelSize);
            if (tickLabelIsTooLarge)
                return (null, tickLabelSize);

            var tickPosition = NumericConversion.ToNumber(dt);
            Tick tick = new(tickPosition, tickLabel, true);
            ticks.Add(tick);

            // this prevents infinite loops with weird axis limits or small delta (e.g., DateTime)
            if (ticks.Count >= maxTickCount)
                break;
        }

        return (ticks, null);
    }
}
