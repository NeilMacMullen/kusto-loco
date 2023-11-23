using System;
using System.Collections.Generic;
using System.IO;

namespace TDigest;

public abstract class Digest
{
    protected double Max = double.NegativeInfinity;
    protected double Min = double.PositiveInfinity;
    protected ScaleFunction Scale = new ScaleFunction.K2();

    public abstract double Compression { get; set; }

    /**
     * Creates an {@link MergingDigest}.  This is generally the best known implementation right now.
     *
     * @param compression The compression parameter.  100 is a common value for normal uses.  1000 is extremely large.
     * The number of centroids retained will be a smallish (usually less than 10) multiple of this number.
     * @return the MergingDigest
     */
    public static Digest CreateMergingDigest(double compression) => new MergingDigest(compression);

    /**
     * Creates a TDigest of whichever type is the currently recommended type.  MergingDigest is generally the best
     * known implementation right now.
     *
     * @param compression The compression parameter.  100 is a common value for normal uses.  1000 is extremely large.
     * The number of centroids retained will be a smallish (usually less than 10) multiple of this number.
     * @return the TDigest
     */
    public static Digest CreateDigest(double compression) => CreateMergingDigest(compression);

    /**
    * Adds a sample to a histogram.
    *
    * @param x The value to add.
    * @param w The weight of this point.
    */
    public abstract void Add(double x, int w);

    private void CheckValue(double x)
    {
        if (double.IsNaN(x))
        {
            throw new ArgumentException("Cannot add NaN");
        }
    }

    public abstract void Add(IEnumerable<Digest> others);

    /**
     * Re-examines a t-digest to determine whether some centroids are redundant.  If your data are
     * perversely ordered, this may be a good idea.  Even if not, this may save 20% or so in space.
     *
     * The cost is roughly the same as adding as many data points as there are centroids.  This
     * is typically &lt; 10 * compression, but could be as high as 100 * compression.
     *
     * This is a destructive operation that is not thread-safe.
     */
    public abstract void Compress();

    /**
     * Returns the number of points that have been added to this TDigest.
     *
     * @return The sum of the weights on all centroids.
     */
    public abstract long Size();

    /**
     * Returns the fraction of all points added which are &le; x.
     *
     * @param x The cutoff for the cdf.
     * @return The fraction of all data which is less or equal to x.
     */
    public abstract double Cdf(double x);

    /**
     * Returns an estimate of the cutoff such that a specified fraction of the data
     * added to this TDigest would be less than or equal to the cutoff.
     *
     * @param q The desired fraction
     * @return The value x such that cdf(x) == q
     */
    public abstract double Quantile(double q);

    /**
     * A {@link Collection} that lets you go through the centroids in ascending order by mean.  Centroids
     * returned will not be re-used, but may or may not share storage with this TDigest.
     *
     * @return The centroids in the form of a Collection.
     */
    public abstract IEnumerable<Centroid> Centroids();

    /**
     * Returns the number of bytes required to encode this TDigest using #asBytes().
     *
     * @return The number of bytes required.
     */
    public abstract int ByteSize();

    /**
     * Returns the number of bytes required to encode this TDigest using #asSmallBytes().
     *
     * Note that this is just as expensive as actually compressing the digest. If you don't
     * care about time, but want to never over-allocate, this is fine. If you care about compression
     * and speed, you pretty much just have to overallocate by using allocating #byteSize() bytes.
     *
     * @return The number of bytes required.
     */
    public abstract int SmallByteSize();

    public void SetScaleFunction(ScaleFunction scaleFunction)
    {
        /*
           if (scaleFunction.toString().endsWith("NO_NORM")) {
               throw new ArgumentException(
                       string.Format("Can't use %s as scale with %s", scaleFunction, getClass()));
           }
        */
        Scale = scaleFunction;
    }

    /**
     * Serialize this TDigest into a byte buffer.  Note that the serialization used is
     * very straightforward and is considerably larger than strictly necessary.
     *
     * @param buf The byte buffer into which the TDigest should be serialized.
     */
    public abstract void AsBytes(BinaryWriter buf);

    /**
     * Serialize this TDigest into a byte buffer.  Some simple compression is used
     * such as using variable byte representation to store the centroid weights and
     * using delta-encoding on the centroid means so that floats can be reasonably
     * used to store the centroid means.
     *
     * @param buf The byte buffer into which the TDigest should be serialized.
     */
    public abstract void AsSmallBytes(BinaryWriter buf);

    /**
     * Tell this TDigest to record the original data as much as possible for test
     * purposes.
     *
     * @return This TDigest so that configurations can be done in fluent style.
     */
    public abstract Digest RecordAllData();

    public abstract bool IsRecording();

    /**
     * Add a sample to this TDigest.
     *
     * @param x The data value to add
     */
    public abstract void Add(double x);

    /**
     * Add all of the centroids of another TDigest to this one.
     *
     * @param other The other TDigest
     */
    public abstract void Add(Digest other);

    public abstract int CentroidCount();

    public double GetMin() => Min;

    public double GetMax() => Max;

    /**
     * Over-ride the min and max values for testing purposes
     */
    protected void SetMinMax(double min, double max)
    {
        Min = min;
        Max = max;
    }
}