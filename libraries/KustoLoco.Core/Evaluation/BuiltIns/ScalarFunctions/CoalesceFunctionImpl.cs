


using System;
using System.Diagnostics;
using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Kusto.Language;
using KustoLoco.Core.DataSource.Columns;

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
 



internal sealed class CoalesceFunctionIntImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     int? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (int?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.Int, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new int?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<int?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionLongImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     long? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (long?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.Long, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new long?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<long?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionDecimalImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     decimal? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (decimal?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.Decimal, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new decimal?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<decimal?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionDoubleImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     double? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (double?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.Real, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new double?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<double?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionDateTimeImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     DateTime? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (DateTime?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.DateTime, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new DateTime?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<DateTime?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionTimeSpanImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     TimeSpan? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (TimeSpan?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.TimeSpan, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new TimeSpan?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<TimeSpan?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionGuidImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     Guid? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (Guid?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.Guid, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new Guid?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<Guid?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionBoolImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     bool? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (bool?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.Bool, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new bool?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<bool?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionStringImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     string? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (string?)arguments[i].Value;
         if (item is null 
         || item.Length == 0    
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.String, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new string?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<string?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
            || item.Length == 0    
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  

internal sealed class CoalesceFunctionJsonNodeImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
 {
     JsonNode? result = null;
     for (var i = 0; i < arguments.Length; i++)
     {
         var item = (JsonNode?)arguments[i].Value;
         if (item is null 
            
         ) 
         continue;
         result = item;
         break;
     }
     return new ScalarResult(ScalarTypes.Dynamic, result);
 }

 public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
 {
     Debug.Assert(arguments.Length > 0);

     var numRows = arguments[0].Column.RowCount;
     var data = new JsonNode?[numRows];
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (TypedBaseColumn<JsonNode?>)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(ColumnFactory.Create(data));
 }
}  



internal static class Coalesce
{

    internal static void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> functions)
    {
        var overloads = new ScalarFunctionInfo(

            new ScalarOverloadInfo(new CoalesceFunctionIntImpl(),true, ScalarTypes.Int, ScalarTypes.Int)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionLongImpl(),true, ScalarTypes.Long, ScalarTypes.Long)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionDecimalImpl(),true, ScalarTypes.Decimal, ScalarTypes.Decimal)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionDoubleImpl(),true, ScalarTypes.Real, ScalarTypes.Real)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionDateTimeImpl(),true, ScalarTypes.DateTime, ScalarTypes.DateTime)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionTimeSpanImpl(),true, ScalarTypes.TimeSpan, ScalarTypes.TimeSpan)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionGuidImpl(),true, ScalarTypes.Guid, ScalarTypes.Guid)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionBoolImpl(),true, ScalarTypes.Bool, ScalarTypes.Bool)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionStringImpl(),true, ScalarTypes.String, ScalarTypes.String)
            ,

            new ScalarOverloadInfo(new CoalesceFunctionJsonNodeImpl(),true, ScalarTypes.Dynamic, ScalarTypes.Dynamic)


            );
        functions.Add(Kusto.Language.Functions.Coalesce, overloads);
    }
}