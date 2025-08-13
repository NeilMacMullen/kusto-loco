using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class BenchmarkImpl : IScalarFunctionImpl
{
    internal static ScalarOverloadInfo Overload =>
        new(new BenchmarkImpl(),
            TypeMapping.SymbolForType(typeof(long)), TypeMapping.SymbolForType(typeof(string)),TypeMapping.SymbolForType(typeof(long)))
        ;
    public ScalarResult InvokeScalar(ScalarResult[] arguments) => new(ScalarTypes.Long, 0);

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var instrColumn = (GenericTypedBaseColumnOfstring)arguments[0].Column;
        var argColumn = (GenericTypedBaseColumnOflong)arguments[1].Column;

        var rowCount = instrColumn.RowCount;


        var data = NullableSetBuilderOflong.CreateFixed(rowCount);

        var rangePartitioner = SafePartitioner.Create(rowCount);
        var instr = instrColumn[0]!;
        var watch = Stopwatch.StartNew();
        var noFetch = instr.Contains("nofetch");
        var noSvFetch = instr.Contains("nosvfetch");
        var parallel = instr.Contains("parallel");
        var nostore = instr.Contains("nostore");
        var doop = instr.Contains("op");


        if (parallel)
            Parallel.ForEach(rangePartitioner, (range, loopState) =>
            {
                for (var i = range.Item1; i < range.Item2; i++)
                {
                    var d = noFetch ? 0 : argColumn[i];
                    d = noSvFetch ? d : instrColumn[i] !=null ?1:0;
                    if (doop) d += instrColumn[i]!.Contains("nofetch or paralle") ?1:0;
                    if (!nostore)
                        data[i] = d;
                }
            });
        else
            for (var i = 0; i < rowCount; i++)
            {
                var d = noFetch ? 0 : argColumn[i];
                d = noSvFetch ? d : instrColumn[i] != null ? 1 : 0;
                if (doop) d += instrColumn[i]!.Contains("nofetch or paralle") ? 1 : 0;
                if (!nostore)
                    data[i] = d;
            }

        data[0] = watch.ElapsedTicks;
        return new ColumnarResult(ColumnFactory.CreateFromDataSet(data.ToNullableSet()));
    }
}

public partial class BenchmarkFunction
{
    public static readonly FunctionSymbol Func =
        new FunctionSymbol("benchmark",
                ScalarTypes.Long,
                new Parameter("instr", ScalarTypes.String, minOccurring: 1),
                new Parameter("arg",ScalarTypes.Long,minOccurring:1)
            ).ConstantFoldable()
            .WithResultNameKind(ResultNameKind.None);
    public static ScalarFunctionInfo S = new ScalarFunctionInfo(
        BenchmarkImpl.Overload
    );
    public static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> f)
        => f.Add(Func, S);
}
