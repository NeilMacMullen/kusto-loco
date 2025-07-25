


using System.Diagnostics;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using System.Collections.Generic;
using Kusto.Language;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;


/*
   WARNING 
   -------
   This file was auto-generated.
   Do not modify by hand - your changes will be lost .
    

   Built:  03:56:34 PM on Saturday, 26 Jul 2025
   Machine: NPM-LENOVO
   User:  neilm

*/ 
 

internal sealed class TakeAnyFunctionImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments) => new RowResult(arguments.Select(a => a.Column.GetRawDataValue(0)));
}


internal sealed class TakeAnyIntImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.Int,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyLongImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.Long,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyDecimalImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.Decimal,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyDoubleImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.Real,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyDateTimeImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.DateTime,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyTimeSpanImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.TimeSpan,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyGuidImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.Guid,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyBoolImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.Bool,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyStringImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.String,arguments[0].Column.GetRawDataValue(0));
}
  

internal sealed class TakeAnyJsonNodeImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        => new ScalarResult(ScalarTypes.Dynamic,arguments[0].Column.GetRawDataValue(0));
}
  



internal static class TakeAny
{

    internal static void Register(Dictionary<FunctionSymbol, AggregateInfo> aggregates)
    {
        
        var takeAnyOverloads = new AggregateInfo(


            new AggregateOverloadInfo(new TakeAnyIntImpl(), ScalarTypes.Int, ScalarTypes.Int),



            new AggregateOverloadInfo(new TakeAnyLongImpl(), ScalarTypes.Long, ScalarTypes.Long),



            new AggregateOverloadInfo(new TakeAnyDecimalImpl(), ScalarTypes.Decimal, ScalarTypes.Decimal),



            new AggregateOverloadInfo(new TakeAnyDoubleImpl(), ScalarTypes.Real, ScalarTypes.Real),



            new AggregateOverloadInfo(new TakeAnyDateTimeImpl(), ScalarTypes.DateTime, ScalarTypes.DateTime),



            new AggregateOverloadInfo(new TakeAnyTimeSpanImpl(), ScalarTypes.TimeSpan, ScalarTypes.TimeSpan),



            new AggregateOverloadInfo(new TakeAnyGuidImpl(), ScalarTypes.Guid, ScalarTypes.Guid),



            new AggregateOverloadInfo(new TakeAnyBoolImpl(), ScalarTypes.Bool, ScalarTypes.Bool),



            new AggregateOverloadInfo(new TakeAnyStringImpl(), ScalarTypes.String, ScalarTypes.String),



            new AggregateOverloadInfo(new TakeAnyJsonNodeImpl(), ScalarTypes.Dynamic, ScalarTypes.Dynamic),


            new AggregateOverloadInfo(new TakeAnyFunctionImpl(), ScalarTypes.Dynamic,0)

            );
        aggregates.Add(Aggregates.TakeAny, takeAnyOverloads);
        aggregates.Add(Aggregates.Any, takeAnyOverloads);
    }

}