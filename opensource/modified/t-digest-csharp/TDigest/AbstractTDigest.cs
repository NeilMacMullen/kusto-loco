using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TDigest {
    public abstract class AbstractTDigest : Digest {
        private readonly Random gen = new Random();
        protected bool IsRecordingAllData;

        /**
         * Same as {@link #weightedAverageSorted(double, double, double, double)} but flips
         * the order of the variables if <code>x2</code> is greater than
         * <code>x1</code>.
         */
        public static double WeightedAverage(double x1, double w1, double x2, double w2) {
            if (x1 <= x2) {
                return WeightedAverageSorted(x1, w1, x2, w2);
            }
            else {
                return WeightedAverageSorted(x2, w2, x1, w1);
            }
        }

        /**
         * Compute the weighted average between <code>x1</code> with a weight of
         * <code>w1</code> and <code>x2</code> with a weight of <code>w2</code>.
         * This expects <code>x1</code> to be less than or equal to <code>x2</code>
         * and is guaranteed to return a number between <code>x1</code> and
         * <code>x2</code>.
         */
        private static double WeightedAverageSorted(double x1, double w1, double x2, double w2) {
            Debug.Assert(x1 <= x2);
            var x = (x1 * w1 + x2 * w2) / (w1 + w2);
            return Math.Max(x1, Math.Min(x, x2));
        }

        static double Interpolate(double x, double x0, double x1) {
            return (x - x0) / (x1 - x0);
        }

        static void Encode(byte[] buf, int n) {
            var k = 0;
            while (n < 0 || n > 0x7f) {
                var b = (byte) (0x80 | (0x7f & n));
                buf[k] = b;
                n >>= 7;
                k++;
                if (k >= 6) {
                    throw new InvalidOperationException("Size is implausibly large");
                }
            }

            buf[k] = (byte) n;
        }

        static int Decode(byte[] buf) {
            var i = 0;
            int v = buf[i++];
            var z = 0x7f & v;
            var shift = 7;
            while ((v & 0x80) != 0) {
                if (shift > 28) {
                    throw new InvalidOperationException("Shift too large in decode");
                }

                v = buf[i++];
                z += (v & 0x7f) << shift;
                shift += 7;
            }

            return z;
        }

        public abstract void Add(double x, int w, Centroid baseCentroid);

        /**
         * Computes an interpolated value of a quantile that is between two centroids.
         *
         * Index is the quantile desired multiplied by the total number of samples - 1.
         *
         * @param index              Denormalized quantile desired
         * @param previousIndex      The denormalized quantile corresponding to the center of the previous centroid.
         * @param nextIndex          The denormalized quantile corresponding to the center of the following centroid.
         * @param previousMean       The mean of the previous centroid.
         * @param nextMean           The mean of the following centroid.
         * @return  The interpolated mean.
         */
        static double Quantile(double index, double previousIndex, double nextIndex, double previousMean,
            double nextMean) {
            var delta = nextIndex - previousIndex;
            var previousWeight = (nextIndex - index) / delta;
            var nextWeight = (index - previousIndex) / delta;
            return previousMean * previousWeight + nextMean * nextWeight;
        }

        /**
         * Sets up so that all centroids will record all data assigned to them.  For testing only, really.
         */
        public override Digest RecordAllData() {
            IsRecordingAllData = true;
            return this;
        }

        public override bool IsRecording() {
            return IsRecordingAllData;
        }

        /**
         * Adds a sample to a histogram.
         *
         * @param x The value to add.
         */
        public override void Add(double x) {
            Add(x, 1);
        }

        public override void Add(Digest other) {
            var tmp = other.Centroids().ToList();

            Shuffle(tmp);

            foreach (var centroid in tmp) {
                Add(centroid.Mean(), centroid.Count, centroid);
            }
        }

        protected Centroid CreateCentroid(double mean, int id) {
            return new Centroid(mean, id, IsRecordingAllData);
        }

        private void Shuffle<T>(IList<T> list) {
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = gen.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}