


using System;
using KustoLoco.Core.DataSource.Columns;

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
 
public static class IndexFinder
{

    public static int FindIndexOfInt(BaseColumn column, bool max)
    {
    
        var valueIndex = 0;
        var valueSoFar = max ? int.MinValue : int.MaxValue;
        var valuesColumn = (TypedBaseColumn<int?>) column;
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (valuesColumn[i] is not { } n) continue;
            if ( max && n <= valueSoFar) continue;
            if (!max && n >= valueSoFar) continue;
            valueSoFar = n;
            valueIndex = i;
        }

        return valueIndex;
    }

    public static int FindIndexOfLong(BaseColumn column, bool max)
    {
    
        var valueIndex = 0;
        var valueSoFar = max ? long.MinValue : long.MaxValue;
        var valuesColumn = (TypedBaseColumn<long?>) column;
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (valuesColumn[i] is not { } n) continue;
            if ( max && n <= valueSoFar) continue;
            if (!max && n >= valueSoFar) continue;
            valueSoFar = n;
            valueIndex = i;
        }

        return valueIndex;
    }

    public static int FindIndexOfDecimal(BaseColumn column, bool max)
    {
    
        var valueIndex = 0;
        var valueSoFar = max ? decimal.MinValue : decimal.MaxValue;
        var valuesColumn = (TypedBaseColumn<decimal?>) column;
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (valuesColumn[i] is not { } n) continue;
            if ( max && n <= valueSoFar) continue;
            if (!max && n >= valueSoFar) continue;
            valueSoFar = n;
            valueIndex = i;
        }

        return valueIndex;
    }

    public static int FindIndexOfDouble(BaseColumn column, bool max)
    {
    
        var valueIndex = 0;
        var valueSoFar = max ? double.MinValue : double.MaxValue;
        var valuesColumn = (TypedBaseColumn<double?>) column;
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (valuesColumn[i] is not { } n) continue;
            if ( max && n <= valueSoFar) continue;
            if (!max && n >= valueSoFar) continue;
            valueSoFar = n;
            valueIndex = i;
        }

        return valueIndex;
    }

    public static int FindIndexOfDateTime(BaseColumn column, bool max)
    {
    
        var valueIndex = 0;
        var valueSoFar = max ? DateTime.MinValue : DateTime.MaxValue;
        var valuesColumn = (TypedBaseColumn<DateTime?>) column;
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (valuesColumn[i] is not { } n) continue;
            if ( max && n <= valueSoFar) continue;
            if (!max && n >= valueSoFar) continue;
            valueSoFar = n;
            valueIndex = i;
        }

        return valueIndex;
    }

    public static int FindIndexOfTimeSpan(BaseColumn column, bool max)
    {
    
        var valueIndex = 0;
        var valueSoFar = max ? TimeSpan.MinValue : TimeSpan.MaxValue;
        var valuesColumn = (TypedBaseColumn<TimeSpan?>) column;
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (valuesColumn[i] is not { } n) continue;
            if ( max && n <= valueSoFar) continue;
            if (!max && n >= valueSoFar) continue;
            valueSoFar = n;
            valueIndex = i;
        }

        return valueIndex;
    }

}