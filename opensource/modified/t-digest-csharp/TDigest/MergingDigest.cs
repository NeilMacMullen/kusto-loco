using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TDigest;

/**
 * Maintains a t-digest by collecting new points in a buffer that is then sorted occasionally and merged
 * into a sorted array that contains previously computed centroids.
 * <p>
 *     This can be very fast because the cost of sorting and merging is amortized over several insertion. If
 *     we keep N centroids total and have the input array is k long, then the amortized cost is something like
 *     <p>
 *         N/k + log k
 *         <p>
 *             These costs even out when N/k = log k.  Balancing costs is often a good place to start in optimizing an
 *             algorithm.  For different values of compression factor, the following table shows estimated asymptotic
 *             values of N and suggested values of k:
 *             <table>
 *                 <thead>
 *                     <tr>
 *                         <td>Compression</td><td>N</td><td>k</td>
 *                     </tr>
 *                 </thead>
 *                 <tbody>
 *                     <tr>
 *                         <td>50</td><td>78</td><td>25</td>
 *                     </tr>
 *                     <tr>
 *                         <td>100</td><td>157</td><td>42</td>
 *                     </tr>
 *                     <tr>
 *                         <td>200</td><td>314</td><td>73</td>
 *                     </tr>
 *                 </tbody>
 *                 <caption>Sizing considerations for t-digest</caption>
 *             </table>
 *             <p>
 *                 The virtues of this kind of t-digest implementation include:
 *                 <ul>
 *                     <li>No allocation is required after initialization</li>
 *                     <li>The data structure automatically compresses existing centroids when possible</li>
 *                     <li>No Java object overhead is incurred for centroids since data is kept in primitive arrays</li>
 *                 </ul>
 *                 <p>
 *                     The current implementation takes the liberty of using ping-pong buffers for implementing the merge
 *                     resulting
 *                     in a substantial memory penalty, but the complexity of an in place merge was not considered as
 *                     worthwhile
 *                     since even with the overhead, the memory cost is less than 40 bytes per centroid which is much less
 *                     than half
 *                     what the AVLTreeDigest uses and no dynamic allocation is required at all.
 */
public class MergingDigest : AbstractTDigest
{
    public enum Encoding
    {
        VerboseEncoding = 1,
        SmallEncoding = 2,
    }

    // this forces centroid merging based on size limit rather than
    // based on accumulated k-index. This can be much faster since we
    // scale functions are more expensive than the corresponding
    // weight limits.
    public static bool UseWeightLimit = true;

    // mean of points added to each merged centroid
    private readonly double[] mean;


    // array used for sorting the temp centroids.  This is a field
    // to avoid allocations during operation
    private readonly int[] order;

    private readonly double publicCompression;
    private readonly double[] tempMean;
    private readonly double[] tempWeight;

    // number of points that have been added to each merged centroid
    private readonly double[] weight;

    private double compression;

    // history of all data added to centroids (for testing purposes)
    private List<List<double>> data;

    // points to the first unused centroid
    private int lastUsedCell;
    private int mergeCount;
    private List<List<double>> tempData;

    // this is the index of the next temporary centroid
    // this is a more Java-like convention than lastUsedCell uses
    private int tempUsed;

    // sum_i weight[i]  See also unmergedWeight
    private double totalWeight;

    // sum_i tempWeight[i]
    private double unmergedWeight;

    // if true, alternate upward and downward merge passes
    public bool UseAlternatingSort = true;

    // if true, use higher working value of compression during construction, then reduce on presentation
    public bool UseTwoLevelCompression = true;

    /**
     * Allocates a buffer merging t-digest.  This is the normally used constructor that
     * allocates default sized internal arrays.  Other versions are available, but should
     * only be used for special cases.
     *
     * @param compression The compression factor
     */
    public MergingDigest(double compression) :
        this(compression, -1)
    {
    }

    /**
     * If you know the size of the temporary buffer for incoming points, you can use this entry point.
     *
     * @param compression Compression factor for t-digest.  Same as 1/\delta in the paper.
     * @param bufferSize  How many samples to retain before merging.
     */
    public MergingDigest(double compression, int bufferSize) :
        this(compression, bufferSize, -1)
    {
        // we can guarantee that we only need ceiling(compression).
    }

    /**
     * Fully specified constructor.  Normally only used for deserializing a buffer t-digest.
     *
     * @param compression Compression factor
     * @param bufferSize  Number of temporary centroids
     * @param size        Size of main buffer
     */
    public MergingDigest(double compression, int bufferSize, int size)
    {
        // ensure compression >= 10
        // default size = 2 * ceil(compression)
        // default bufferSize = 5 * size
        // scale = max(2, bufferSize / size - 1)
        // compression, publicCompression = sqrt(scale-1)*compression, compression
        // ensure size > 2 * compression + weightLimitFudge
        // ensure bufferSize > 2*size

        // force reasonable value. Anything less than 10 doesn't make much sense because
        // too few centroids are retained
        if (compression < 10)
        {
            compression = 10;
        }

        // the weight limit is too conservative about sizes and can require a bit of extra room
        double sizeFudge = 0;
        if (UseWeightLimit)
        {
            sizeFudge = 10;
            if (compression < 30) sizeFudge += 20;
        }

        // default size
        size = (int)Math.Max(2 * compression + sizeFudge, size);

        // default buffer
        if (bufferSize == -1)
        {
            // TODO update with current numbers
            // having a big buffer is good for speed
            // experiments show bufferSize = 1 gives half the performance of bufferSize=10
            // bufferSize = 2 gives 40% worse performance than 10
            // but bufferSize = 5 only costs about 5-10%
            //
            //   compression factor     time(us)
            //    50          1         0.275799
            //    50          2         0.151368
            //    50          5         0.108856
            //    50         10         0.102530
            //   100          1         0.215121
            //   100          2         0.142743
            //   100          5         0.112278
            //   100         10         0.107753
            //   200          1         0.210972
            //   200          2         0.148613
            //   200          5         0.118220
            //   200         10         0.112970
            //   500          1         0.219469
            //   500          2         0.158364
            //   500          5         0.127552
            //   500         10         0.121505
            bufferSize = 5 * size;
        }

        // ensure enough space in buffer
        if (bufferSize <= 2 * size)
        {
            bufferSize = 2 * size;
        }

        // scale is the ratio of extra buffer to the final size
        // we have to account for the fact that we copy all live centroids into the incoming space
        double scale = Math.Max(1, bufferSize / size - 1);
        //noinspection ConstantConditions
        if (!UseTwoLevelCompression)
        {
            scale = 1;
        }

        // publicCompression is how many centroids the user asked for
        // compression is how many we actually keep
        publicCompression = compression;
        Compression = Math.Sqrt(scale) * publicCompression;

        // changing the compression could cause buffers to be too small, readjust if so
        if (size < compression + sizeFudge)
        {
            size = (int)Math.Ceiling(compression + sizeFudge);
        }

        // ensure enough space in buffer (possibly again)
        if (bufferSize <= 2 * size)
        {
            bufferSize = 2 * size;
        }

        weight = new double[size];
        mean = new double[size];

        tempWeight = new double[bufferSize];
        tempMean = new double[bufferSize];
        order = new int[bufferSize];

        lastUsedCell = 0;
    }

    public override double Compression
    {
        get => publicCompression;
        set => compression = value;
    }

    /**
     * Turns on internal data recording.
     */
    public override Digest RecordAllData()
    {
        base.RecordAllData();
        data = new List<List<double>>();
        tempData = new List<List<double>>();
        return this;
    }

    public override void Add(double x, int w, Centroid baseCentroid)
    {
        Add(x, w, baseCentroid.Data());
    }

    public override void Add(double x, int w)
    {
        Add(x, w, null);
    }

    private void Add(double x, int w, List<double> history)
    {
        if (double.IsNaN(x))
        {
            throw new ArgumentException("Cannot add NaN to t-digest");
        }

        if (tempUsed >= tempWeight.Length - lastUsedCell - 1)
        {
            MergeNewValues();
        }

        var where = tempUsed++;
        tempWeight[where] = w;
        tempMean[where] = x;
        unmergedWeight += w;
        if (x < Min)
        {
            Min = x;
        }

        if (x > Max)
        {
            Max = x;
        }

        if (data != null)
        {
            if (tempData == null)
            {
                tempData = new List<List<double>>();
            }

            while (tempData.Count <= where)
            {
                tempData.Add(new List<double>());
            }

            if (history == null)
            {
                history = new List<double> { x };
            }

            tempData[where].AddRange(history);
        }
    }

    private void Add(double[] m, double[] w, int count, List<List<double>> data)
    {
        if (m.Length != w.Length)
        {
            throw new ArgumentException("Arrays not same length");
        }

        if (m.Length < count + lastUsedCell)
        {
            // make room to add existing centroids
            var m1 = new double[count + lastUsedCell];
            Array.Copy(m, 0, m1, 0, count);
            m = m1;
            var w1 = new double[count + lastUsedCell];
            Array.Copy(w, 0, w1, 0, count);
            w = w1;
        }

        double total = 0;
        for (var i = 0; i < count; i++)
        {
            total += w[i];
        }

        Merge(m, w, count, data, null, total, false, compression);
    }

    public override void Add(IEnumerable<Digest> others)
    {
        if (!others.Any())
        {
            return;
        }

        var size = 0;
        foreach (var other in others)
        {
            other.Compress();
            size += other.CentroidCount();
        }

        var m = new double[size];
        var w = new double[size];
        List<List<double>> data;
        if (IsRecordingAllData)
        {
            data = new List<List<double>>();
        }
        else
        {
            data = null;
        }

        var offset = 0;
        foreach (var other in others)
        {
            if (other is MergingDigest)
            {
                var md = (MergingDigest)other;
                Array.Copy(md.mean, 0, m, offset, md.lastUsedCell);
                Array.Copy(md.weight, 0, w, offset, md.lastUsedCell);
                if (data != null)
                {
                    foreach (var centroid in other.Centroids())
                    {
                        data.Add(centroid.Data());
                    }
                }

                offset += md.lastUsedCell;
            }
            else
            {
                foreach (var centroid in other.Centroids())
                {
                    m[offset] = centroid.Mean();
                    w[offset] = centroid.Count;
                    if (IsRecordingAllData)
                    {
                        Debug.Assert(data != null);
                        data.Add(centroid.Data());
                    }

                    offset++;
                }
            }
        }

        Add(m, w, size, data);
    }

    private void MergeNewValues()
    {
        MergeNewValues(false, compression);
    }

    private void MergeNewValues(bool force, double compression)
    {
        if (totalWeight == 0 && unmergedWeight == 0)
        {
            // seriously nothing to do
            return;
        }

        if (force || unmergedWeight > 0)
        {
            // note that we run the merge in reverse every other merge to avoid left-to-right bias in merging
            Merge(tempMean, tempWeight, tempUsed, tempData, order, unmergedWeight,
                UseAlternatingSort & (mergeCount % 2 == 1), compression);
            mergeCount++;
            tempUsed = 0;
            unmergedWeight = 0;
            if (data != null)
            {
                tempData = new List<List<double>>();
            }
        }
    }

    private void Merge(double[] incomingMean, double[] incomingWeight, int incomingCount,
        List<List<double>> incomingData, int[] incomingOrder,
        double unmergedWeight, bool runBackwards, double compression)
    {
        // when our incoming buffer fills up, we combine our existing centroids with the incoming data,
        // and then reduce the centroids by merging if possible
        Array.Copy(mean, 0, incomingMean, incomingCount, lastUsedCell);
        Array.Copy(weight, 0, incomingWeight, incomingCount, lastUsedCell);
        incomingCount += lastUsedCell;

        if (incomingData != null)
        {
            for (var i = 0; i < lastUsedCell; i++)
            {
                Debug.Assert(data != null);
                incomingData.Add(data[i]);
            }

            data = new List<List<double>>();
        }

        if (incomingOrder == null)
        {
            incomingOrder = new int[incomingCount];
        }

        Sorts.Sort(incomingOrder, incomingMean, incomingCount);
        // option to run backwards is to help investigate bias in errors
        if (runBackwards)
        {
            Sorts.Reverse(incomingOrder, 0, incomingCount);
        }

        totalWeight += unmergedWeight;

        lastUsedCell = 0;

        mean[lastUsedCell] = incomingMean[incomingOrder[0]];
        weight[lastUsedCell] = incomingWeight[incomingOrder[0]];
        double wSoFar = 0;
        if (data != null)
        {
            Debug.Assert(incomingData != null);
            data.Add(incomingData[incomingOrder[0]]);
        }

        // weight will contain all zeros after this loop

        var normalizer = Scale.Normalizer(compression, totalWeight);
        var k1 = Scale.K(0, normalizer);
        var wLimit = totalWeight * Scale.Q(k1 + 1, normalizer);
        for (var i = 1; i < incomingCount; i++)
        {
            var ix = incomingOrder[i];
            var proposedWeight = weight[lastUsedCell] + incomingWeight[ix];
            var projectedW = wSoFar + proposedWeight;
            bool addThis;
            if (UseWeightLimit)
            {
                var q0 = wSoFar / totalWeight;
                var q2 = (wSoFar + proposedWeight) / totalWeight;
                addThis = proposedWeight <=
                          totalWeight * Math.Min(Scale.Max(q0, normalizer), Scale.Max(q2, normalizer));
            }
            else
            {
                addThis = projectedW <= wLimit;
            }

            if (addThis)
            {
                // next point will fit
                // so merge into existing centroid
                weight[lastUsedCell] += incomingWeight[ix];
                mean[lastUsedCell] = mean[lastUsedCell] + (incomingMean[ix] - mean[lastUsedCell]) *
                    incomingWeight[ix] / weight[lastUsedCell];
                incomingWeight[ix] = 0;

                if (data != null)
                {
                    while (data.Count <= lastUsedCell)
                    {
                        data.Add(new List<double>());
                    }

                    Debug.Assert(incomingData != null);
                    Debug.Assert(data[lastUsedCell] != incomingData[ix]);
                    data[lastUsedCell].AddRange(incomingData[ix]);
                }
            }
            else
            {
                // didn't fit ... move to next output, copy out first centroid
                wSoFar += weight[lastUsedCell];
                if (!UseWeightLimit)
                {
                    k1 = Scale.K(wSoFar / totalWeight, normalizer);
                    wLimit = totalWeight * Scale.Q(k1 + 1, normalizer);
                }

                lastUsedCell++;
                mean[lastUsedCell] = incomingMean[ix];
                weight[lastUsedCell] = incomingWeight[ix];
                incomingWeight[ix] = 0;

                if (data != null)
                {
                    Debug.Assert(incomingData != null);
                    Debug.Assert(data.Count == lastUsedCell);
                    data.Add(incomingData[ix]);
                }
            }
        }

        // points to next empty cell
        lastUsedCell++;

        // sanity check
        double sum = 0;
        for (var i = 0; i < lastUsedCell; i++)
        {
            sum += weight[i];
        }

        Debug.Assert(sum == totalWeight);
        if (runBackwards)
        {
            Sorts.Reverse(mean, 0, lastUsedCell);
            Sorts.Reverse(weight, 0, lastUsedCell);
            data?.Reverse();
        }

        if (totalWeight > 0)
        {
            Min = Math.Min(Min, mean[0]);
            Max = Math.Max(Max, mean[lastUsedCell - 1]);
        }
    }

    /**
     * Exposed for testing.
     */
    private int CheckWeights() => CheckWeights(weight, totalWeight, lastUsedCell);

    private int CheckWeights(double[] w, double total, int last)
    {
        var badCount = 0;

        var n = last;
        if (w[n] > 0)
        {
            n++;
        }

        var normalizer = Scale.Normalizer(publicCompression, totalWeight);
        var k1 = Scale.K(0, normalizer);
        double q = 0;
        double left = 0;
        var header = "\n";
        for (var i = 0; i < n; i++)
        {
            var dq = w[i] / total;
            var k2 = Scale.K(q + dq, normalizer);
            q += dq / 2;
            if (k2 - k1 > 1 && w[i] != 1)
            {
                Console.WriteLine($"{header}Oversize centroid at " +
                                  "{i}, k0={k0:0.00}, k1={k1:0.00}, dk={(k2 - k1):0.00}, w={w[i]:0.00}," +
                                  " q={q:0.0000}, dq={dq:0.0000}, left={left:0.0}, current={w[i]:0.00} maxw={(totalWeight * scale.max(q, normalizer)):0.00}\n");
                header = string.Empty;
                badCount++;
            }

            if (k2 - k1 > 4 && w[i] != 1)
            {
                throw new InvalidOperationException("Egregiously oversized centroid at " +
                                                    "{i}, k0={k0:0.00}, k1={k1:0.00}, dk={(k2 - k1):0.00}, w={w[i]:0.00}," +
                                                    " q={q:0.0000}, dq={dq:0.0000}, left={left:0.0}, current={w[i]:0.00} maxw={(totalWeight * scale.max(q, normalizer)):0.00}\n");
            }

            q += dq / 2;
            left += w[i];
            k1 = k2;
        }

        return badCount;
    }

    /**
     * Merges any pending inputs and compresses the data down to the public setting.
     * Note that this typically loses a bit of precision and thus isn't a thing to
     * be doing all the time. It is best done only when we want to show results to
     * the outside world.
     */
    public override void Compress()
    {
        MergeNewValues(true, publicCompression);
    }

    public override long Size() => (long)(totalWeight + unmergedWeight);

    public override double Cdf(double x)
    {
        if (double.IsNaN(x) || double.IsInfinity(x))
        {
            throw new ArgumentException("Invalid value: " + x);
        }

        MergeNewValues();

        if (lastUsedCell == 0)
        {
            // no data to examine
            return double.NaN;
        }

        if (lastUsedCell == 1)
        {
            // exactly one centroid, should have max==min
            var width = Max - Min;
            if (x < Min)
            {
                return 0;
            }

            if (x > Max)
            {
                return 1;
            }

            if (x - Min <= width)
            {
                // min and max are too close together to do any viable interpolation
                return 0.5;
            }

            // interpolate if somehow we have weight > 0 and max != min
            return (x - Min) / (Max - Min);
        }

        var n = lastUsedCell;
        if (x < Min)
        {
            return 0;
        }

        if (x > Max)
        {
            return 1;
        }

        // check for the left tail
        if (x < mean[0])
        {
            // note that this is different than mean[0] > min
            // ... this guarantees we divide by non-zero number and interpolation works
            if (mean[0] - Min > 0)
            {
                // must be a sample exactly at min
                if (x == Min)
                {
                    return 0.5 / totalWeight;
                }

                return (1 + (x - Min) / (mean[0] - Min) * (weight[0] / 2 - 1)) / totalWeight;
            }

            // this should be redundant with the check x < min
            return 0;
        }

        Debug.Assert(x >= mean[0]);

        // and the right tail
        if (x > mean[n - 1])
        {
            if (Max - mean[n - 1] > 0)
            {
                if (x == Max)
                {
                    return 1 - 0.5 / totalWeight;
                }

                // there has to be a single sample exactly at max
                var dq = (1 + (Max - x) / (Max - mean[n - 1]) * (weight[n - 1] / 2 - 1)) / totalWeight;
                return 1 - dq;
            }

            return 1;
        }

        // we know that there are at least two centroids and mean[0] < x < mean[n-1]
        // that means that there are either one or more consecutive centroids all at exactly x
        // or there are consecutive centroids, c0 < x < c1
        double weightSoFar = 0;
        for (var it = 0; it < n - 1; it++)
        {
            // weightSoFar does not include weight[it] yet
            if (mean[it] == x)
            {
                // we have one or more centroids == x, treat them as one
                // dw will accumulate the weight of all of the centroids at x
                double dw = 0;
                while (it < n && mean[it] == x)
                {
                    dw += weight[it];
                    it++;
                }

                return (weightSoFar + dw / 2) / totalWeight;
            }

            if (mean[it] <= x && x < mean[it + 1])
            {
                // landed between centroids ... check for floating point madness
                if (mean[it + 1] - mean[it] > 0)
                {
                    // note how we handle singleton centroids here
                    // the point is that for singleton centroids, we know that their entire
                    // weight is exactly at the centroid and thus shouldn't be involved in
                    // interpolation
                    double leftExcludedW = 0;
                    double rightExcludedW = 0;
                    if (weight[it] == 1)
                    {
                        if (weight[it + 1] == 1)
                        {
                            // two singletons means no interpolation
                            // left singleton is in, right is out
                            return (weightSoFar + 1) / totalWeight;
                        }

                        leftExcludedW = 0.5;
                    }
                    else if (weight[it + 1] == 1)
                    {
                        rightExcludedW = 0.5;
                    }

                    var dw = (weight[it] + weight[it + 1]) / 2;

                    // can't have double singleton (handled that earlier)
                    Debug.Assert(dw > 1);
                    Debug.Assert((leftExcludedW + rightExcludedW) <= 0.5);

                    // adjust endpoints for any singleton
                    var left = mean[it];
                    var right = mean[it + 1];

                    var dwNoSingleton = dw - leftExcludedW - rightExcludedW;

                    // adjustments have only limited effect on endpoints
                    Debug.Assert(dwNoSingleton > dw / 2);
                    Debug.Assert(right - left > 0);
                    var baseD = weightSoFar + weight[it] / 2 + leftExcludedW;
                    return (baseD + dwNoSingleton * (x - left) / (right - left)) / totalWeight;
                }
                else
                {
                    // this is simply caution against floating point madness
                    // it is conceivable that the centroids will be different
                    // but too near to allow safe interpolation
                    var dw = (weight[it] + weight[it + 1]) / 2;
                    return (weightSoFar + dw) / totalWeight;
                }
            }

            weightSoFar += weight[it];
        }

        if (x == mean[n - 1])
        {
            return 1 - 0.5 / totalWeight;
        }

        throw new InvalidOperationException("Can't happen ... loop fell through");
    }

    public override double Quantile(double q)
    {
        if (q < 0 || q > 1)
        {
            throw new ArgumentException("q should be in [0,1], got " + q);
        }

        MergeNewValues();

        if (lastUsedCell == 0)
        {
            // no centroids means no data, no way to get a quantile
            return double.NaN;
        }

        if (lastUsedCell == 1)
        {
            // with one data point, all quantiles lead to Rome
            return mean[0];
        }

        // we know that there are at least two centroids now
        var n = lastUsedCell;

        // if values were stored in a sorted array, index would be the offset we are interested in
        var index = q * totalWeight;

        // beyond the boundaries, we return min or max
        // usually, the first centroid will have unit weight so this will make it moot
        if (index < 1)
        {
            return Min;
        }

        // if the left centroid has more than one sample, we still know
        // that one sample occurred at min so we can do some interpolation
        if (weight[0] > 1 && index < weight[0] / 2)
        {
            // there is a single sample at min so we interpolate with less weight
            return Min + (index - 1) / (weight[0] / 2 - 1) * (mean[0] - Min);
        }

        // usually the last centroid will have unit weight so this test will make it moot
        if (index > totalWeight - 1)
        {
            return Max;
        }

        // if the right-most centroid has more than one sample, we still know
        // that one sample occurred at max so we can do some interpolation
        if (weight[n - 1] > 1 && totalWeight - index <= weight[n - 1] / 2)
        {
            return Max - (totalWeight - index - 1) / (weight[n - 1] / 2 - 1) * (Max - mean[n - 1]);
        }

        double z1, z2;

        // in between extremes we interpolate between centroids
        var weightSoFar = weight[0] / 2;
        for (var i = 0; i < n - 1; i++)
        {
            var dw = (weight[i] + weight[i + 1]) / 2;
            if (weightSoFar + dw > index)
            {
                // centroids i and i+1 bracket our current point

                // check for unit weight
                double leftUnit = 0;
                if (weight[i] == 1)
                {
                    if (index - weightSoFar < 0.5)
                    {
                        // within the singleton's sphere
                        return mean[i];
                    }

                    leftUnit = 0.5;
                }

                double rightUnit = 0;
                if (weight[i + 1] == 1)
                {
                    if (weightSoFar + dw - index <= 0.5)
                    {
                        // no interpolation needed near singleton
                        return mean[i + 1];
                    }

                    rightUnit = 0.5;
                }

                z1 = index - weightSoFar - leftUnit;
                z2 = weightSoFar + dw - index - rightUnit;
                return WeightedAverage(mean[i], z2, mean[i + 1], z1);
            }

            weightSoFar += dw;
        }

        // we handled singleton at end up above
        Debug.Assert(weight[n - 1] > 1);
        Debug.Assert(index <= totalWeight);
        Debug.Assert(index >= totalWeight - weight[n - 1] / 2);

        // weightSoFar = totalWeight - weight[n-1]/2 (very nearly)
        // so we interpolate out to max value ever seen
        z1 = index - totalWeight - weight[n - 1] / 2.0;
        z2 = weight[n - 1] / 2 - z1;
        return WeightedAverage(mean[n - 1], z1, Max, z2);
    }

    public override int CentroidCount()
    {
        MergeNewValues();
        return lastUsedCell;
    }

    public override IEnumerable<Centroid> Centroids()
    {
        // we don't actually keep centroid structures around so we have to fake it
        Compress();

        var i = 0;
        while (i < lastUsedCell)
        {
            var rc = new Centroid(mean[i], (int)weight[i], data?[i]);
            i++;
            yield return rc;
        }
    }


    public override int ByteSize()
    {
        Compress();
        // format code, compression(float), buffer-size(int), temp-size(int), #centroids-1(int),
        // then two doubles per centroid
        return lastUsedCell * 16 + 32;
    }

    public override int SmallByteSize()
    {
        Compress();
        // format code(int), compression(float), buffer-size(short), temp-size(short), #centroids-1(short),
        // then two floats per centroid
        return lastUsedCell * 8 + 30;
    }

    public ScaleFunction GetScaleFunction() => Scale;

    public override void AsBytes(BinaryWriter buf)
    {
        Compress();
        buf.Write((int)Encoding.VerboseEncoding);
        buf.Write(Min);
        buf.Write(Max);
        buf.Write(publicCompression);
        buf.Write(lastUsedCell);
        for (var i = 0; i < lastUsedCell; i++)
        {
            buf.Write(weight[i]);
            buf.Write(mean[i]);
        }
    }

    public override void AsSmallBytes(BinaryWriter buf)
    {
        Compress();
        buf.Write((int)Encoding.SmallEncoding); // 4
        buf.Write(Min); // + 8
        buf.Write(Max); // + 8
        buf.Write((float)publicCompression); // + 4
        buf.Write((short)mean.Length); // + 2
        buf.Write((short)tempMean.Length); // + 2
        buf.Write((short)lastUsedCell); // + 2 = 30
        for (var i = 0; i < lastUsedCell; i++)
        {
            buf.Write((float)weight[i]);
            buf.Write((float)mean[i]);
        }
    }

    public static MergingDigest FromBytes(BinaryReader buf)
    {
        var encoding = buf.ReadInt32();
        if (encoding == (int)Encoding.VerboseEncoding)
        {
            var min = buf.ReadDouble();
            var max = buf.ReadDouble();
            var compression = buf.ReadDouble();
            var n = buf.ReadInt32();
            var r = new MergingDigest(compression);
            r.SetMinMax(min, max);
            r.lastUsedCell = n;
            for (var i = 0; i < n; i++)
            {
                r.weight[i] = buf.ReadDouble();
                r.mean[i] = buf.ReadDouble();

                r.totalWeight += r.weight[i];
            }

            return r;
        }

        if (encoding == (int)Encoding.SmallEncoding)
        {
            var min = buf.ReadDouble();
            var max = buf.ReadDouble();
            double compression = buf.ReadSingle();
            int n = buf.ReadInt16();
            int bufferSize = buf.ReadInt16();
            var r = new MergingDigest(compression, bufferSize, n);
            r.SetMinMax(min, max);
            r.lastUsedCell = buf.ReadInt16();
            for (var i = 0; i < r.lastUsedCell; i++)
            {
                r.weight[i] = buf.ReadSingle();
                r.mean[i] = buf.ReadSingle();

                r.totalWeight += r.weight[i];
            }

            return r;
        }

        throw new InvalidOperationException("Invalid format for serialized histogram");
    }

    public override string ToString() =>
        "MergingDigest"
        + "-" + GetScaleFunction()
        + "-" + (UseWeightLimit ? "weight" : "kSize")
        + "-" + (UseAlternatingSort ? "alternating" : "stable")
        + "-" + (UseTwoLevelCompression ? "twoLevel" : "oneLevel");
}