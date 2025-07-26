

using Kusto.Language.Symbols;
using Kusto.Language.Syntax;
using KustoLoco.Core.DataSource;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;




/*
   WARNING 
   -------
   This file was auto-generated.
   Do not modify by hand - your changes will be lost .
    

   Built:  03:56:33 PM on Saturday, 26 Jul 2025
   Machine: NPM-LENOVO
   User:  neilm

*/ 
 


   
internal class ArgMaxFunctionIntImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfInt(arguments[0].Column, true);
        //the first argument the column used to calculate maxIndex
        //so we normally skip it but if the call was of the form arg_max(A)
        //then we need in include it
        var skip = arguments.Length > 1 ? 1 : 0;
        var o = arguments.Skip(skip).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

  
internal class ArgMinFunctionIntImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfInt(arguments[0].Column, false);
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

   
internal class ArgMaxFunctionLongImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfLong(arguments[0].Column, true);
        //the first argument the column used to calculate maxIndex
        //so we normally skip it but if the call was of the form arg_max(A)
        //then we need in include it
        var skip = arguments.Length > 1 ? 1 : 0;
        var o = arguments.Skip(skip).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

  
internal class ArgMinFunctionLongImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfLong(arguments[0].Column, false);
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

   
internal class ArgMaxFunctionDecimalImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfDecimal(arguments[0].Column, true);
        //the first argument the column used to calculate maxIndex
        //so we normally skip it but if the call was of the form arg_max(A)
        //then we need in include it
        var skip = arguments.Length > 1 ? 1 : 0;
        var o = arguments.Skip(skip).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

  
internal class ArgMinFunctionDecimalImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfDecimal(arguments[0].Column, false);
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

   
internal class ArgMaxFunctionDoubleImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfDouble(arguments[0].Column, true);
        //the first argument the column used to calculate maxIndex
        //so we normally skip it but if the call was of the form arg_max(A)
        //then we need in include it
        var skip = arguments.Length > 1 ? 1 : 0;
        var o = arguments.Skip(skip).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

  
internal class ArgMinFunctionDoubleImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfDouble(arguments[0].Column, false);
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

   
internal class ArgMaxFunctionDateTimeImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfDateTime(arguments[0].Column, true);
        //the first argument the column used to calculate maxIndex
        //so we normally skip it but if the call was of the form arg_max(A)
        //then we need in include it
        var skip = arguments.Length > 1 ? 1 : 0;
        var o = arguments.Skip(skip).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

  
internal class ArgMinFunctionDateTimeImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfDateTime(arguments[0].Column, false);
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

   
internal class ArgMaxFunctionTimeSpanImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfTimeSpan(arguments[0].Column, true);
        //the first argument the column used to calculate maxIndex
        //so we normally skip it but if the call was of the form arg_max(A)
        //then we need in include it
        var skip = arguments.Length > 1 ? 1 : 0;
        var o = arguments.Skip(skip).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

  
internal class ArgMinFunctionTimeSpanImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfTimeSpan(arguments[0].Column, false);
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
        return new RowResult(o);
    }
}

