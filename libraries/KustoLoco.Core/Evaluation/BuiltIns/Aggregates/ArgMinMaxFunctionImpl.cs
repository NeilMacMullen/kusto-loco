

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
    

   Built:  02:02:24 PM on Saturday, 26 Jul 2025
   Machine: BEAST
   User:  User

*/ 
 


   
internal class ArgMaxFunctionIntImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {

        var maxIndex = IndexFinder.FindIndexOfInt(arguments[0].Column, true);
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
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
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
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
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
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
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
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
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
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
        //the first argument is repeated
        var o = arguments.Skip(1).Select(a => a.Column.GetRawDataValue(maxIndex));
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

