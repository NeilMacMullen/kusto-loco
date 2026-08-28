//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitMakeSeriesOperator(IRMakeSeriesOperatorNode node, EvaluationContext context)
    {
        var input = context.Left.Value;
        var resultSchema = (TableSymbol)node.ResultType;

        // Reassemble the input into a single chunk so we can index rows freely.
        var chunk = ChunkHelpers.Reassemble(input.GetData().ToArray());
        var rowCount = chunk.RowCount;
        var chunkContext = context with { Chunk = chunk };

        // Range: from / to / step are constant scalars.
        var fromVal = ((ScalarResult)node.From.Accept(this, chunkContext)).Value;
        var toVal = ((ScalarResult)node.To.Accept(this, chunkContext)).Value;
        var stepVal = ((ScalarResult)node.Step.Accept(this, chunkContext)).Value;

        // KQL make-series bins the axis into [from, to) buckets of width step (bin_at semantics): 'to' is
        // NON-inclusive and the number of points is the count of start + i*step values strictly below 'to'. Temporal
        // axes are computed in exact integer ticks — datetime ticks (~6.3e17) exceed double's 2^53 exact-integer
        // range, so doing the arithmetic in double would drift the axis and mis-bin rows.
        var axisIsDateTime = fromVal is DateTime;
        var axisIsTimeSpan = fromVal is TimeSpan;
        var temporal = axisIsDateTime || axisIsTimeSpan;

        // An inclusive stop (the 'in range(..)' syntax) admits the point at 'to': count = floor(span/step)+1. A
        // non-inclusive end ('from .. to ..') stops strictly below 'to': count = ceil(span/step).
        long fromTicks = 0, stepTicks = 0;
        double fromD = 0, stepD = 0;
        int binCount;
        if (temporal)
        {
            fromTicks = ToTicks(fromVal);
            stepTicks = ToTicks(stepVal);
            var span = ToTicks(toVal) - fromTicks;
            if (stepTicks <= 0) binCount = 0;
            else if (node.StopInclusive) binCount = span >= 0 ? (int)(span / stepTicks) + 1 : 0;
            else binCount = span > 0 ? (int)((span + stepTicks - 1) / stepTicks) : 0;
        }
        else
        {
            fromD = ToDouble(fromVal);
            stepD = ToDouble(stepVal);
            var span = ToDouble(toVal) - fromD;
            // The +/-1e-9 keeps an exact multiple from being mis-rounded by floating-point error.
            if (stepD <= 0) binCount = 0;
            else if (node.StopInclusive) binCount = span >= 0 ? (int)Math.Floor(span / stepD + 1e-9) + 1 : 0;
            else binCount = span > 0 ? (int)Math.Ceiling(span / stepD - 1e-9) : 0;
        }
        if (binCount < 0) binCount = 0;

        // Axis + by-columns evaluated columnar over the whole chunk.
        var axisColumn = ((ColumnarResult)node.Axis.Accept(this, chunkContext)).Column;
        var byColumns = node.ByColumns
            .Select(b => ((ColumnarResult)b.Accept(this, chunkContext)).Column)
            .ToArray();

        // Group rows by the by-values, and within a group index them by bin.
        var groups = new Dictionary<GroupKey, GroupState>();
        var groupOrder = new List<GroupKey>();
        for (var r = 0; r < rowCount; r++)
        {
            var key = new GroupKey(byColumns.Select(c => c.GetRawDataValue(r)).ToArray());
            if (!groups.TryGetValue(key, out var state))
            {
                state = new GroupState(binCount);
                groups[key] = state;
                groupOrder.Add(key);
            }

            int bin;
            if (temporal)
            {
                var t = ToTicks(axisColumn.GetRawDataValue(r));
                bin = stepTicks > 0 && t >= fromTicks ? (int)((t - fromTicks) / stepTicks) : -1;
            }
            else
            {
                var axisD = ToDouble(axisColumn.GetRawDataValue(r));
                bin = stepD > 0 ? (int)Math.Floor((axisD - fromD) / stepD) : -1;
            }
            if (bin >= 0 && bin < binCount) state.BinRows[bin].Add(r);
        }

        var outBuilders = ColumnHelpers.CreateBuildersForTable(resultSchema);
        // Column layout of the result: [by columns..., aggregate series..., axis series].
        var byCount = node.ByColumns.Count;

        foreach (var key in groupOrder)
        {
            var state = groups[key];
            var outCol = 0;

            // by columns
            for (var b = 0; b < byCount; b++) outBuilders[outCol++].Add(key.Values[b]);

            // one series per aggregate: aggregate the rows in each bin, gap-fill empty bins with the default.
            for (var a = 0; a < node.Aggregations.Count; a++)
            {
                // ADX fills empty bins with the per-aggregate default; when no 'default=' clause is written the default
                // is 0 (typed to the aggregate's result), not null. An explicit 'default=<null>' still fills with null.
                var fill = node.DefaultProvided[a] ? node.Defaults[a] : DefaultZero(node.Aggregations[a].ResultType);
                var series = new JsonArray();
                for (var i = 0; i < binCount; i++)
                {
                    var rows = state.BinRows[i];
                    if (rows.Count == 0)
                    {
                        series.Add(ToJson(fill));
                        continue;
                    }

                    var binChunk = new TableChunk(chunk.Table,
                        chunk.Columns.Select(c => ColumnHelpers.MapColumn(c, rows.ToImmutableArray())).ToArray());
                    var aggResult = node.Aggregations[a].Accept(this, context with { Chunk = binChunk });
                    var value = aggResult is ScalarResult sr ? sr.Value : null;
                    series.Add(ToJson(value));
                }
                outBuilders[outCol++].Add(series);
            }

            // axis series: from + i*step for each bin (exact in the axis's native type).
            var axisArray = new JsonArray();
            for (var i = 0; i < binCount; i++)
            {
                if (axisIsDateTime)
                    axisArray.Add(JsonValue.Create(
                        new DateTime(fromTicks + (long)i * stepTicks, DateTimeKind.Utc).ToString("o")));
                else if (axisIsTimeSpan)
                    axisArray.Add(JsonValue.Create(fromTicks + (long)i * stepTicks));
                else
                    axisArray.Add(JsonValue.Create(fromD + i * stepD));
            }
            outBuilders[outCol++].Add(axisArray);
        }

        var outColumns = outBuilders.Select(b => b.ToColumn()).ToArray();
        return TabularResult.CreateUnvisualized(new InMemoryTableSource(resultSchema, outColumns));
    }

    private static long ToTicks(object? v) => v switch
    {
        DateTime dt => dt.Ticks,
        TimeSpan ts => ts.Ticks,
        long l => l,
        int i => i,
        double d => (long)d,
        decimal m => (long)m,
        _ => 0
    };

    private static object DefaultZero(TypeSymbol t) =>
        t == ScalarTypes.Real ? 0.0 :
        t == ScalarTypes.Decimal ? 0m :
        t == ScalarTypes.Int ? 0 :
        0L;

    private static double ToDouble(object? v) => v switch
    {
        DateTime dt => dt.Ticks,
        TimeSpan ts => ts.Ticks,
        long l => l,
        int i => i,
        double d => d,
        decimal m => (double)m,
        _ => 0
    };

    private static JsonNode? ToJson(object? v) => v switch
    {
        null => null,
        long l => JsonValue.Create(l),
        int i => JsonValue.Create(i),
        double d => JsonValue.Create(d),
        decimal m => JsonValue.Create(m),
        bool b => JsonValue.Create(b),
        DateTime dt => JsonValue.Create(dt.ToString("o")),
        string s => JsonValue.Create(s),
        JsonNode j => j.DeepClone(),
        _ => JsonValue.Create(v.ToString())
    };

    private sealed class GroupState
    {
        public GroupState(int binCount)
        {
            BinRows = new List<int>[binCount];
            for (var i = 0; i < binCount; i++) BinRows[i] = new List<int>();
        }

        public List<int>[] BinRows { get; }
    }

    private readonly struct GroupKey : IEquatable<GroupKey>
    {
        public GroupKey(object?[] values) => Values = values;
        public object?[] Values { get; }

        public bool Equals(GroupKey other)
        {
            if (Values.Length != other.Values.Length) return false;
            for (var i = 0; i < Values.Length; i++)
                if (!Equals(Values[i], other.Values[i])) return false;
            return true;
        }

        public override bool Equals(object? obj) => obj is GroupKey g && Equals(g);

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (var v in Values) hash.Add(v);
            return hash.ToHashCode();
        }
    }
}
