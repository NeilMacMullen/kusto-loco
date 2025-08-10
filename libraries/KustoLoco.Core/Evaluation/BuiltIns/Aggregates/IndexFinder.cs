


using System;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;


/*
   WARNING 
   -------
   This file was auto-generated.
   Do not modify by hand - your changes will be lost .
    

   Built:  05:33:19 PM on Tuesday, 29 Jul 2025
   Machine: BEAST
   User:  User

*/ 
 
public static class IndexFinder
{

    public static int FindIndexOfInt(BaseColumn column, bool max)
    {
    
        var valueIndex = 0;
        var valueSoFar = max ? int.MinValue : int.MaxValue;
        var valuesColumn = (GenericTypedBaseColumnOfint) column;
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
        var valuesColumn = (GenericTypedBaseColumnOflong) column;
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
        var valuesColumn = (GenericTypedBaseColumnOfdecimal) column;
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
        var valuesColumn = (GenericTypedBaseColumnOfdouble) column;
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
        var valuesColumn = (GenericTypedBaseColumnOfDateTime) column;
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
        var valuesColumn = (GenericTypedBaseColumnOfTimeSpan) column;
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