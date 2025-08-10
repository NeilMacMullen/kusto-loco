


using System;
using System.Linq;
using System.Text;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/*
   WARNING 
   -------
   This file was auto-generated.
   Do not modify by hand - your changes will be lost .
    

   Built:  05:33:20 PM on Tuesday, 29 Jul 2025
   Machine: BEAST
   User:  User

*/ 




internal class MinOfFunctionIntImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        int? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MinOfInt(ret, (int?)t.Value);
        return new ScalarResult(ScalarTypes.Int, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfint>().ToArray();

        var data = NullableSetBuilderOfint.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            int? ret = null;
            foreach (var t in columns) ret = TypeComparison.MinOfInt(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfint.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MinOfFunctionLongImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        long? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MinOfLong(ret, (long?)t.Value);
        return new ScalarResult(ScalarTypes.Long, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOflong>().ToArray();

        var data = NullableSetBuilderOflong.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            long? ret = null;
            foreach (var t in columns) ret = TypeComparison.MinOfLong(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOflong.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MinOfFunctionDecimalImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        decimal? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MinOfDecimal(ret, (decimal?)t.Value);
        return new ScalarResult(ScalarTypes.Decimal, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfdecimal>().ToArray();

        var data = NullableSetBuilderOfdecimal.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            decimal? ret = null;
            foreach (var t in columns) ret = TypeComparison.MinOfDecimal(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfdecimal.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MinOfFunctionDoubleImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        double? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MinOfDouble(ret, (double?)t.Value);
        return new ScalarResult(ScalarTypes.Real, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfdouble>().ToArray();

        var data = NullableSetBuilderOfdouble.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            double? ret = null;
            foreach (var t in columns) ret = TypeComparison.MinOfDouble(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfdouble.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MinOfFunctionDateTimeImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        DateTime? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MinOfDateTime(ret, (DateTime?)t.Value);
        return new ScalarResult(ScalarTypes.DateTime, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfDateTime>().ToArray();

        var data = NullableSetBuilderOfDateTime.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            DateTime? ret = null;
            foreach (var t in columns) ret = TypeComparison.MinOfDateTime(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfDateTime.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MinOfFunctionTimeSpanImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        TimeSpan? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MinOfTimeSpan(ret, (TimeSpan?)t.Value);
        return new ScalarResult(ScalarTypes.TimeSpan, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfTimeSpan>().ToArray();

        var data = NullableSetBuilderOfTimeSpan.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            TimeSpan? ret = null;
            foreach (var t in columns) ret = TypeComparison.MinOfTimeSpan(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfTimeSpan.CreateFromDataSet(data.ToNullableSet()));
    }
}



internal class MaxOfFunctionIntImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        int? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MaxOfInt(ret, (int?)t.Value);
        return new ScalarResult(ScalarTypes.Int, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfint>().ToArray();

        var data = NullableSetBuilderOfint.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            int? ret = null;
            foreach (var t in columns) ret = TypeComparison.MaxOfInt(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfint.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MaxOfFunctionLongImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        long? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MaxOfLong(ret, (long?)t.Value);
        return new ScalarResult(ScalarTypes.Long, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOflong>().ToArray();

        var data = NullableSetBuilderOflong.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            long? ret = null;
            foreach (var t in columns) ret = TypeComparison.MaxOfLong(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOflong.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MaxOfFunctionDecimalImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        decimal? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MaxOfDecimal(ret, (decimal?)t.Value);
        return new ScalarResult(ScalarTypes.Decimal, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfdecimal>().ToArray();

        var data = NullableSetBuilderOfdecimal.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            decimal? ret = null;
            foreach (var t in columns) ret = TypeComparison.MaxOfDecimal(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfdecimal.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MaxOfFunctionDoubleImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        double? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MaxOfDouble(ret, (double?)t.Value);
        return new ScalarResult(ScalarTypes.Real, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfdouble>().ToArray();

        var data = NullableSetBuilderOfdouble.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            double? ret = null;
            foreach (var t in columns) ret = TypeComparison.MaxOfDouble(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfdouble.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MaxOfFunctionDateTimeImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        DateTime? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MaxOfDateTime(ret, (DateTime?)t.Value);
        return new ScalarResult(ScalarTypes.DateTime, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfDateTime>().ToArray();

        var data = NullableSetBuilderOfDateTime.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            DateTime? ret = null;
            foreach (var t in columns) ret = TypeComparison.MaxOfDateTime(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfDateTime.CreateFromDataSet(data.ToNullableSet()));
    }
}

internal class MaxOfFunctionTimeSpanImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        TimeSpan? ret = null;
        foreach (var t in arguments) ret = TypeComparison.MaxOfTimeSpan(ret, (TimeSpan?)t.Value);
        return new ScalarResult(ScalarTypes.TimeSpan, ret);
    }
    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        var columns = arguments
            .Select(a => a.Column)
            .Cast<GenericTypedBaseColumnOfTimeSpan>().ToArray();

        var data = NullableSetBuilderOfTimeSpan.CreateFixed(columns[0].RowCount);
        var builder = new StringBuilder();
        for (var row = 0; row < columns[0].RowCount; row++)
        {
            TimeSpan? ret = null;
            foreach (var t in columns) ret = TypeComparison.MaxOfTimeSpan(ret, t[row]);
            data[row] = ret;
        }
        return new ColumnarResult(GenericColumnFactoryOfTimeSpan.CreateFromDataSet(data.ToNullableSet()));
    }
}





public static class MinOfRegister
{
  public static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions)
  {
      functions.Add(Kusto.Language.Functions.MinOf,
         new ScalarFunctionInfo(



            new ScalarOverloadInfo(new MinOfFunctionIntImpl(),true, ScalarTypes.Int, ScalarTypes.Int)
           ,

            new ScalarOverloadInfo(new MinOfFunctionLongImpl(),true, ScalarTypes.Long, ScalarTypes.Long)
           ,

            new ScalarOverloadInfo(new MinOfFunctionDecimalImpl(),true, ScalarTypes.Decimal, ScalarTypes.Decimal)
           ,

            new ScalarOverloadInfo(new MinOfFunctionDoubleImpl(),true, ScalarTypes.Real, ScalarTypes.Real)
           ,

            new ScalarOverloadInfo(new MinOfFunctionDateTimeImpl(),true, ScalarTypes.DateTime, ScalarTypes.DateTime)
           ,

            new ScalarOverloadInfo(new MinOfFunctionTimeSpanImpl(),true, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan)


         )
      );
  }
}


public static class MaxOfRegister
{
  public static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions)
  {
      functions.Add(Kusto.Language.Functions.MaxOf,
         new ScalarFunctionInfo(



            new ScalarOverloadInfo(new MaxOfFunctionIntImpl(),true, ScalarTypes.Int, ScalarTypes.Int)
           ,

            new ScalarOverloadInfo(new MaxOfFunctionLongImpl(),true, ScalarTypes.Long, ScalarTypes.Long)
           ,

            new ScalarOverloadInfo(new MaxOfFunctionDecimalImpl(),true, ScalarTypes.Decimal, ScalarTypes.Decimal)
           ,

            new ScalarOverloadInfo(new MaxOfFunctionDoubleImpl(),true, ScalarTypes.Real, ScalarTypes.Real)
           ,

            new ScalarOverloadInfo(new MaxOfFunctionDateTimeImpl(),true, ScalarTypes.DateTime, ScalarTypes.DateTime)
           ,

            new ScalarOverloadInfo(new MaxOfFunctionTimeSpanImpl(),true, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan)


         )
      );
  }
}
