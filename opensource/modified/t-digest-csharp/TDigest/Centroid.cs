using System;
using System.Collections.Generic;
using System.Threading;

namespace TDigest {
    /**
     * A single centroid which represents a number of data points.
     */
    public class Centroid : IComparable<Centroid> {
        private static int _uniqueCount;
        public int Count { get; private set; } = 0;

        private List<double> actualData;
        private int id;
        private double centroid = 0;

        private Centroid(bool record) {
            id = Interlocked.Increment(ref _uniqueCount);
            if (record) {
                actualData = new List<double>();
            }
        }

        public Centroid(double x) : this(false) {
            Start(x, 1, Interlocked.Increment(ref _uniqueCount));
        }

        public Centroid(double x, int w) : this(false) {
            Start(x, w, Interlocked.Increment(ref _uniqueCount));
        }

        public Centroid(double x, int w, int id) : this(false) {
            Start(x, w, id);
        }

        public Centroid(double x, int id, bool record) : this(record) {
            Start(x, 1, id);
        }

        public Centroid(double x, int w, List<double> data) : this(x, w) {
            actualData = data;
        }

        private void Start(double x, int w, int id) {
            this.id = id;
            Add(x, w);
        }

        public void Add(double x, int w) {
            actualData?.Add(x);
            Count += w;
            centroid += w * (x - centroid) / Count;
        }

        public double Mean() {
            return centroid;
        }

        public override string ToString() {
            return "Centroid{" +
                   "centroid=" + centroid +
                   ", count=" + Count +
                   '}';
        }

        public override int GetHashCode() {
            return id;
        }

        public int CompareTo(Centroid other) {
            if (other == null) return -1;
            var r = centroid.CompareTo(other.centroid);
            if (r == 0) {
                r = id - other.id;
            }

            return r;
        }

        public List<double> Data() {
            return actualData;
        }

        public void InsertData(double x) {
            if (actualData == null) {
                actualData = new List<double>();
            }

            actualData.Add(x);
        }

        public static Centroid CreateWeighted(double x, int w, IEnumerable<double> data) {
            var r = new Centroid(data != null);
            r.Add(x, w, data);
            return r;
        }

        public void Add(double x, int w, IEnumerable<double> data) {
            if (actualData != null) {
                if (data != null) {
                    foreach (var old in data) {
                        actualData.Add(old);
                    }
                }
                else {
                    actualData.Add(x);
                }
            }

            centroid = AbstractTDigest.WeightedAverage(centroid, Count, x, w);
            Count += w;
        }
    }
}