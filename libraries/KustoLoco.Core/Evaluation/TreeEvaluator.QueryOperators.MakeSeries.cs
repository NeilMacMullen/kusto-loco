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
        var from = ToDouble(fromVal);
        var step = ToDouble(stepVal);
        var binCount = step != 0 ? (int)Math.Floor((ToDouble(toVal) - from) / step) : 0;
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

            var axisD = ToDouble(axisColumn.GetRawDataValue(r));
            if (step == 0) continue;
            var bin = (int)Math.Floor((axisD - from) / step);
            if (bin >= 0 && bin < binCount) state.BinRows[bin].Add(r);
        }

        var outBuilders = ColumnHelpers.CreateBuildersForTable(resultSchema);
        // Column layout of the result: [by columns..., axis series, aggregate series...].
        var byCount = node.ByColumns.Count;
        var axisIsDateTime = fromVal is DateTime;

        foreach (var key in groupOrder)
        {
            var state = groups[key];
            var outCol = 0;

            // by columns
            for (var b = 0; b < byCount; b++) outBuilders[outCol++].Add(key.Values[b]);

            // axis series: from + i*step for each bin
            var axisArray = new JsonArray();
            for (var i = 0; i < binCount; i++)
            {
                var pointTicks = from + i * step;
                axisArray.Add(axisIsDateTime
                    ? JsonValue.Create(new DateTime((long)pointTicks, DateTimeKind.Utc).ToString("o"))
                    : JsonValue.Create(pointTicks));
            }
            outBuilders[outCol++].Add(axisArray);

            // one series per aggregate: aggregate the rows in each bin, gap-fill empties with the default.
            for (var a = 0; a < node.Aggregations.Count; a++)
            {
                var series = new JsonArray();
                for (var i = 0; i < binCount; i++)
                {
                    var rows = state.BinRows[i];
                    if (rows.Count == 0)
                    {
                        series.Add(ToJson(node.Defaults[a]));
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
        }

        var outColumns = outBuilders.Select(b => b.ToColumn()).ToArray();
        return TabularResult.CreateUnvisualized(new InMemoryTableSource(resultSchema, outColumns));
    }

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
