using System;

namespace TDigest;

public abstract class ScaleFunction
{
    /**
     * Converts a quantile to the k-scale. The total number of points is also provided so that a normalizing function
     * can be computed if necessary.
     *
     * @param q           The quantile
     * @param compression Also known as delta in literature on the t-digest
     * @param n           The total number of samples
     * @return The corresponding value of k
     */
    public abstract double K(double q, double compression, double n);

    /**
     * Converts  a quantile to the k-scale. The normalizer value depends on compression and (possibly) number of points
     * in the digest. #normalizer(double, double)
     *
     * @param q          The quantile
     * @param normalizer The normalizer value which depends on compression and (possibly) number of points in the
     * digest.
     * @return The corresponding value of k
     */
    public abstract double K(double q, double normalizer);

    /**
     * Computes q as a function of k. This is often faster than finding k as a function of q for some scales.
     *
     * @param k           The index value to convert into q scale.
     * @param compression The compression factor (often written as &delta;)
     * @param n           The number of samples already in the digest.
     * @return The value of q that corresponds to k
     */
    public abstract double Q(double k, double compression, double n);

    /**
     * Computes q as a function of k. This is often faster than finding k as a function of q for some scales.
     *
     * @param k          The index value to convert into q scale.
     * @param normalizer The normalizer value which depends on compression and (possibly) number of points in the
     * digest.
     * @return The value of q that corresponds to k
     */
    public abstract double Q(double k, double normalizer);

    /**
     * Computes the maximum relative size a cluster can have at quantile q. Note that exactly where within the range
     * spanned by a cluster that q should be isn't clear. That means that this function usually has to be taken at
     * multiple points and the smallest value used.
     * <p>
     *     Note that this is the relative size of a cluster. To get the max number of samples in the cluster, multiply this
     *     value times the total number of samples in the digest.
     *     @param q           The quantile
     *     @param compression The compression factor, typically delta in the literature
     *     @param n           The number of samples seen so far in the digest
     *     @return The maximum number of samples that can be in the cluster
     */
    public abstract double Max(double q, double compression, double n);

    /**
     * Computes the maximum relative size a cluster can have at quantile q. Note that exactly where within the range
     * spanned by a cluster that q should be isn't clear. That means that this function usually has to be taken at
     * multiple points and the smallest value used.
     * <p>
     *     Note that this is the relative size of a cluster. To get the max number of samples in the cluster, multiply this
     *     value times the total number of samples in the digest.
     *     @param q          The quantile
     *     @param normalizer The normalizer value which depends on compression and (possibly) number of points in the
     *     digest.
     *     @return The maximum number of samples that can be in the cluster
     */
    public abstract double Max(double q, double normalizer);

    /**
     * Computes the normalizer given compression and number of points.
     */
    public abstract double Normalizer(double compression, double n);

    /**
     * Approximates asin to within about 1e-6. This approximation works by breaking the range from 0 to 1 into 5 regions
     * for all but the region nearest 1, rational polynomial models get us a very good approximation of asin and by
     * interpolating as we move from region to region, we can guarantee continuity and we happen to get monotonicity as
     * well.  for the values near 1, we just use Math.asin as our region "approximation".
     *
     * @param x sin(theta)
     * @return theta
     */
    public static double FastAsin(double x)
    {
        if (x < 0)
        {
            return -FastAsin(-x);
        }

        if (x > 1)
        {
            return double.NaN;
        }

        // Cutoffs for models. Note that the ranges overlap. In the
        // overlap we do linear interpolation to guarantee the overall
        // result is "nice"
        var c0High = 0.1;
        var c1High = 0.55;
        var c2Low = 0.5;
        var c2High = 0.8;
        var c3Low = 0.75;
        var c3High = 0.9;
        var c4Low = 0.87;
        if (x > c3High)
        {
            return Math.Asin(x);
        }

        // the models
        double[] m0 = { 0.2955302411, 1.2221903614, 0.1488583743, 0.2422015816, -0.3688700895, 0.0733398445 };
        double[] m1 =
            { -0.0430991920, 0.9594035750, -0.0362312299, 0.1204623351, 0.0457029620, -0.0026025285 };
        double[] m2 =
        {
            -0.034873933724, 1.054796752703, -0.194127063385, 0.283963735636, 0.023800124916,
            -0.000872727381
        };
        double[] m3 =
            { -0.37588391875, 2.61991859025, -2.48835406886, 1.48605387425, 0.00857627492, -0.00015802871 };
        // the parameters for all of the models
        double[] vars = { 1, x, x * x, x * x * x, 1 / (1 - x), 1 / (1 - x) / (1 - x) };
        // raw grist for interpolation coefficients
        var x0 = Bound((c0High - x) / c0High);
        var x1 = Bound((c1High - x) / (c1High - c2Low));
        var x2 = Bound((c2High - x) / (c2High - c3Low));
        var x3 = Bound((c3High - x) / (c3High - c4Low));
        // interpolation coefficients
        var mix0 = x0;
        var mix1 = (1 - x0) * x1;
        var mix2 = (1 - x1) * x2;
        var mix3 = (1 - x2) * x3;
        var mix4 = 1 - x3;
        // now mix all the results together, avoiding extra evaluations
        double r = 0;
        if (mix0 > 0)
        {
            r += mix0 * Eval(m0, vars);
        }

        if (mix1 > 0)
        {
            r += mix1 * Eval(m1, vars);
        }

        if (mix2 > 0)
        {
            r += mix2 * Eval(m2, vars);
        }

        if (mix3 > 0)
        {
            r += mix3 * Eval(m3, vars);
        }

        if (mix4 > 0)
        {
            // model 4 is just the real deal
            r += mix4 * Math.Asin(x);
        }

        return r;
    }

    private static double Eval(double[] model, double[] vars)
    {
        double r = 0;
        for (var i = 0; i < model.Length; i++)
        {
            r += model[i] * vars[i];
        }

        return r;
    }

    private static double Bound(double v)
    {
        if (v <= 0)
        {
            return 0;
        }

        if (v >= 1)
        {
            return 1;
        }

        return v;
    }

    /**
     * Generates uniform cluster sizes. Used for comparison only.
     */
    public class K0 : ScaleFunction
    {
        public override double K(double q, double compression, double n) => compression * q / 2;

        public override double K(double q, double normalizer) => normalizer * q;

        public override double Q(double k, double compression, double n) => 2 * k / compression;

        public override double Q(double k, double normalizer) => k / normalizer;

        public override double Max(double q, double compression, double n) => 2 / compression;

        public override double Max(double q, double normalizer) => 1 / normalizer;

        public override double Normalizer(double compression, double n) => compression / 2;
    }

    /**
     * Generates cluster sizes proportional to sqrt(q*(1-q)). This gives constant relative accuracy if accuracy is
     * proportional to squared cluster size. It is expected that K_2 and K_3 will give better practical results.
     */
    public class K1 : ScaleFunction
    {
        public override double K(double q, double compression, double n) =>
            compression * Math.Asin(2 * q - 1) / (2 * Math.PI);

        public override double K(double q, double normalizer) => normalizer * Math.Asin(2 * q - 1);

        public override double Q(double k, double compression, double n) =>
            (Math.Sin(k * (2 * Math.PI / compression)) + 1) / 2;

        public override double Q(double k, double normalizer) => (Math.Sin(k / normalizer) + 1) / 2;

        public override double Max(double q, double compression, double n)
        {
            if (q <= 0)
            {
                return 0;
            }

            if (q >= 1)
            {
                return 0;
            }

            return 2 * Math.Sin(Math.PI / compression) * Math.Sqrt(q * (1 - q));
        }

        public override double Max(double q, double normalizer)
        {
            if (q <= 0)
            {
                return 0;
            }

            if (q >= 1)
            {
                return 0;
            }

            return 2 * Math.Sin(0.5 / normalizer) * Math.Sqrt(q * (1 - q));
        }

        public override double Normalizer(double compression, double n) => compression / (2 * Math.PI);
    }

    public class K1Fast : ScaleFunction
    {
        public override double K(double q, double compression, double n) =>
            compression * FastAsin(2 * q - 1) / (2 * Math.PI);

        public override double K(double q, double normalizer) => normalizer * FastAsin(2 * q - 1);

        public override double Q(double k, double compression, double n) =>
            (Math.Sin(k * (2 * Math.PI / compression)) + 1) / 2;

        public override double Q(double k, double normalizer) => (Math.Sin(k / normalizer) + 1) / 2;

        public override double Max(double q, double compression, double n)
        {
            if (q <= 0)
            {
                return 0;
            }

            if (q >= 1)
            {
                return 0;
            }

            return 2 * Math.Sin(Math.PI / compression) * Math.Sqrt(q * (1 - q));
        }

        public override double Max(double q, double normalizer)
        {
            if (q <= 0)
            {
                return 0;
            }

            if (q >= 1)
            {
                return 0;
            }

            return 2 * Math.Sin(0.5 / normalizer) * Math.Sqrt(q * (1 - q));
        }

        public override double Normalizer(double compression, double n) => compression / (2 * Math.PI);
    }

    public class K2 : ScaleFunction
    {
        public override double K(double q, double compression, double n)
        {
            if (n <= 1)
            {
                if (q <= 0)
                {
                    return -10;
                }

                if (q >= 1)
                {
                    return 10;
                }

                return 0;
            }

            if (q == 0)
            {
                return 2 * K(1 / n, compression, n);
            }

            if (q == 1)
            {
                return 2 * K((n - 1) / n, compression, n);
            }

            return compression * Math.Log(q / (1 - q)) / Z(compression, n);
        }

        public override double K(double q, double normalizer)
        {
            if (q < 1e-15)
            {
                // this will return something more extreme than q = 1/n
                return 2 * K(1e-15, normalizer);
            }

            if (q > 1 - 1e-15)
            {
                // this will return something more extreme than q = (n-1)/n
                return 2 * K(1 - 1e-15, normalizer);
            }

            return Math.Log(q / (1 - q)) * normalizer;
        }

        public override double Q(double k, double compression, double n)
        {
            var w = Math.Exp(k * Z(compression, n) / compression);
            return w / (1 + w);
        }

        public override double Q(double k, double normalizer)
        {
            var w = Math.Exp(k / normalizer);
            return w / (1 + w);
        }

        public override double Max(double q, double compression, double n) =>
            Z(compression, n) * q * (1 - q) / compression;

        public override double Max(double q, double normalizer) => q * (1 - q) / normalizer;

        public override double Normalizer(double compression, double n) => compression / Z(compression, n);

        private double Z(double compression, double n) => 4 * Math.Log(n / compression) + 24;
    }
}