using KustoLoco.Core;

namespace Lokql.Engine.Commands;

/// <summary>
/// Use a KustoQueryContext to parse a time range string such as "7d" or "datetime(2021-01-01)" into a DateTime
/// </summary>
public static class TimeRangeProcessor
{

    public static bool ParseTime(string time, out DateTime t)
    {
        t= DateTime.UtcNow;
        //try wrapping in "datetime()" if the raw value is not recognised
        return RawParseTime(time,out t) ||
               RawParseTime($"datetime({time})", out t);
    }

    public static bool RawParseTime(string time,out DateTime t)
    {
        t = DateTime.UtcNow;
        var context = new KustoQueryContext();
        var result = context.RunQueryWithoutDemandBasedTableLoading(time);
        if (result.RowCount == 0)
            return false;
        if(result.ColumnCount != 1)
            return false;

        if (result.Get(0, 0) is DateTime dt)
        {
            t = dt;
            return true;
        }
        if (result.Get(0, 0) is TimeSpan ts)
        {
            t -= ts ;
            return true;
        }

        return false;
    }
}
