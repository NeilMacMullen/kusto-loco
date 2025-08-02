


using System;
using System.Runtime.CompilerServices;

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
 

public static class TypeComparison
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? MaxOfInt(int? a, int? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value > b.Value
                ? a
                : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int? MinOfInt(int? a, int? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value < b.Value
                ? a
                : b;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long? MaxOfLong(long? a, long? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value > b.Value
                ? a
                : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long? MinOfLong(long? a, long? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value < b.Value
                ? a
                : b;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal? MaxOfDecimal(decimal? a, decimal? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value > b.Value
                ? a
                : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static decimal? MinOfDecimal(decimal? a, decimal? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value < b.Value
                ? a
                : b;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? MaxOfDouble(double? a, double? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value > b.Value
                ? a
                : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double? MinOfDouble(double? a, double? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value < b.Value
                ? a
                : b;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? MaxOfDateTime(DateTime? a, DateTime? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value > b.Value
                ? a
                : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime? MinOfDateTime(DateTime? a, DateTime? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value < b.Value
                ? a
                : b;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan? MaxOfTimeSpan(TimeSpan? a, TimeSpan? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value > b.Value
                ? a
                : b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan? MinOfTimeSpan(TimeSpan? a, TimeSpan? b)
    {
        if (b is not { } bValue) return a;

        return a == null
            ? b
            : a.Value < b.Value
                ? a
                : b;
    }



}