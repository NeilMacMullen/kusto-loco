using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TDigest;

/**
 * Reference implementations for cdf and quantile if we have all data.
 */
public class Dist
{
    public static double Cdf(double x, double[] data)
    {
        var n1 = 0;
        var n2 = 0;
        foreach (var v in data)
        {
            n1 += (v < x) ? 1 : 0;
            n2 += (v == x) ? 1 : 0;
        }

        return (n1 + n2 / 2.0) / data.Length;
    }

    public static double Cdf(double x, Collection<double> data)
    {
        var n1 = 0;
        var n2 = 0;
        foreach (var v in data)
        {
            n1 += (v < x) ? 1 : 0;
            n2 += (v == x) ? 1 : 0;
        }

        return (n1 + n2 / 2.0) / data.Count;
    }

    public static double Quantile(double q, double[] data)
    {
        var n = data.Length;
        if (n == 0)
        {
            return double.NaN;
        }

        var index = q * n;
        if (index < 0)
        {
            index = 0;
        }

        if (index > n - 1)
        {
            index = n - 1;
        }

        return data[(int)Math.Floor(index)];
    }

    public static double Quantile(double q, List<double> data)
    {
        var n = data.Count;
        if (n == 0)
        {
            return double.NaN;
        }

        var index = q * n;
        if (index < 0)
        {
            index = 0;
        }

        if (index > n - 1)
        {
            index = n - 1;
        }

        return data[(int)Math.Floor(index)];
    }
}