


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
    

   Built:  05:33:20 PM on Tuesday, 29 Jul 2025
   Machine: BEAST
   User:  User

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
     var data = NullableSetBuilderOfint.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfint)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfint.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOflong.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOflong)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOflong.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfdecimal.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfdecimal)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfdecimal.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfdouble.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfdouble)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfdouble.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfDateTime.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfDateTime)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfDateTime.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfTimeSpan.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfTimeSpan)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfTimeSpan.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfGuid.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfGuid)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfGuid.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfbool.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfbool)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfbool.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfstring.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfstring)arguments[i].Column;
             var item = column[j];
            if (item is null 
            || item.Length == 0    
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfstring.CreateFromDataSet(data.ToNullableSet()));
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
     var data = NullableSetBuilderOfJsonNode.CreateFixed(numRows);
     for (var j = 0; j < numRows; j++)
     {
         for (var i = 0; i < arguments.Length; i++)
         {
             var column = (GenericTypedBaseColumnOfJsonNode)arguments[i].Column;
             var item = column[j];
            if (item is null 
               
            )  
            continue;
            data[j] = item;
           break;
         }
     }

     return new ColumnarResult(GenericColumnFactoryOfJsonNode.CreateFromDataSet(data.ToNullableSet()));
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
