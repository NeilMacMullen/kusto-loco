using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace TDigest.Tests {
    public abstract class DigestTest {
        private Random rng = new Random();
        private readonly ITestOutputHelper testOutputHelper;

        protected DigestTest(ITestOutputHelper testOutputHelper) {
            this.testOutputHelper = testOutputHelper;
        }

        public interface IDigestFactory {
            Digest Create();
        }

        protected abstract IDigestFactory Factory(double compression = 100);

        protected abstract AbstractTDigest fromBytes(BinaryReader reader);

        [Fact]
        public void Simple() {
            var digest = Factory(10).Create();
            for (var i = 1; i <= 1000000; i++) {
                digest.Add(i);
            }

            Assert.Equal(990000, digest.Quantile(.99), 0);
            Assert.Equal(500000, digest.Quantile(.50), 0);
        }

        [Fact]
        public void BigJump() {
            var digest = Factory().Create();
            for (var i = 1; i < 20; i++) {
                digest.Add(i);
            }

            digest.Add(1_000_000);

            Assert.Equal(18, digest.Quantile(0.89999999), 0);
            Assert.Equal(19, digest.Quantile(0.9), 0);
            Assert.Equal(19, digest.Quantile(0.949999999), 0);
            Assert.Equal(1_000_000, digest.Quantile(0.95), 0);

            Assert.Equal(0.925, digest.Cdf(19), 11);
            Assert.Equal(0.95, digest.Cdf(19.0000001), 11);
            Assert.Equal(0.9, digest.Cdf(19 - 0.0000001), 11);

            digest = Factory(80).Create();
            digest.SetScaleFunction(new ScaleFunction.K0());

            for (var j = 0; j < 100; j++) {
                for (var i = 1; i < 20; i++) {
                    digest.Add(i);
                }

                digest.Add(1_000_000);
            }

            Assert.Equal(18.0, digest.Quantile(0.885), 0);
            Assert.Equal(19.0, digest.Quantile(0.915), 0);
            Assert.Equal(19.0, digest.Quantile(0.935), 0);
            Assert.Equal(1_000_000.0, digest.Quantile(0.965), 0);
        }

        [Fact]
        public void TestSmallCountQuantile() {
            var data = new List<double>() {15.0, 20.0, 32.0, 60.0};
            var td = Factory(200).Create();
            foreach (var datum in data) {
                td.Add(datum);
            }

            Assert.Equal(20, td.Quantile(0.4), 10);
            Assert.Equal(20, td.Quantile(0.25), 10);
            Assert.Equal(15, td.Quantile(0.25 - 1e-10), 10);
            Assert.Equal(20, td.Quantile(0.5 - 1e-10), 10);
            Assert.Equal(32, td.Quantile(0.5), 10);
        }

        [Fact]
        public void SingletonQuantiles() {
            var data = new double[20];
            var digest = Factory(100).Create();
            for (var i = 0; i < 20; i++) {
                digest.Add(i);
                data[i] = i;
            }

            for (var x = digest.GetMin() - 0.1; x <= digest.GetMax() + 0.1; x += 1e-3) {
                Assert.Equal(Dist.Cdf(x, data), digest.Cdf(x), 0);
            }

            for (double q = 0; q <= 1; q += 1e-3) {
                Assert.Equal(Dist.Quantile(q, data), digest.Quantile(q), 0);
            }
        }

        [Fact]
        public void SingleSingleRange() {
            var digest = Factory(100).Create();
            digest.Add(1);
            digest.Add(2);
            digest.Add(3);

            // verify the cdf is a step between singletons
            Assert.Equal(0.5 / 3.0, digest.Cdf(1), 0);
            Assert.Equal(1 / 3.0, digest.Cdf(1 + 1e-10), 0);
            Assert.Equal(1 / 3.0, digest.Cdf(2 - 1e-10), 0);
            Assert.Equal(1.5 / 3.0, digest.Cdf(2), 0);
            Assert.Equal(2 / 3.0, digest.Cdf(2 + 1e-10), 0);
            Assert.Equal(2 / 3.0, digest.Cdf(3 - 1e-10), 0);
            Assert.Equal(2.5 / 3.0, digest.Cdf(3), 0);
            Assert.Equal(1.0, digest.Cdf(3 + 1e-10), 0);
        }

        [Fact]
        public void SingletonAtEnd() {
            var digest = new MergingDigest(100);
            digest.Add(1);
            digest.Add(2);
            digest.Add(3);

            Assert.Equal(1, digest.GetMin(), 0);
            Assert.Equal(3, digest.GetMax(), 0);
            Assert.Equal(3, digest.CentroidCount());
            Assert.Equal(0, digest.Cdf(0), 0);
            Assert.Equal(0, digest.Cdf(1 - 1e-9), 0);
            Assert.Equal(0.5 / 3, digest.Cdf(1), 10);
            Assert.Equal(1.0 / 3, digest.Cdf(1 + 1e-10), 10);
            Assert.Equal(2.0 / 3, digest.Cdf(3 - 1e-9), 0);
            Assert.Equal(2.5 / 3, digest.Cdf(3), 0);
            Assert.Equal(1.0, digest.Cdf(3 + 1e-9), 0);

            digest.Add(1);
            Assert.Equal(1.0 / 4, digest.Cdf(1), 0);

            // normally min == mean[0] because weight[0] == 1
            // we can force this not to be true for testing
            digest = new MergingDigest(1);
            digest.SetScaleFunction(new ScaleFunction.K0());
            for (var i = 0; i < 100; i++) {
                digest.Add(1);
                digest.Add(2);
                digest.Add(3);
            }

            // This sample will be added to the first cluster that already exists
            // the effect will be to (slightly) nudge the mean of that cluster
            // but also decrease the min. As such, near q=0, cdf and quantiles
            // should reflect this single sample as a singleton
            digest.Add(0);
            Assert.True(digest.CentroidCount() > 0);
            var first = digest.Centroids().First();
            Assert.True(first.Count > 1);
            Assert.True(first.Mean() > digest.GetMin());
            Assert.Equal(0.0, digest.GetMin(), 0);
            Assert.Equal(0, digest.Cdf(0 - 1e-9), 0);
            Assert.Equal(0.5 / digest.Size(), digest.Cdf(0), 9);
            Assert.Equal(1.0 / digest.Size(), digest.Cdf(1e-9), 9);

            Assert.Equal(0, digest.Quantile(0), 0);
            Assert.Equal(0, digest.Quantile(0.5 / digest.Size()), 0);
            Assert.Equal(0, digest.Quantile(1.0 / digest.Size() - 1e-10), 0);
            Assert.Equal(0, digest.Quantile(1.0 / digest.Size()), 0);
            Assert.Equal(2.0 / first.Count / 100, digest.Quantile(1.01 / digest.Size()), 4);
            Assert.Equal(first.Mean(), digest.Quantile(first.Count / 2.0 / digest.Size()), 5);

            digest.Add(4);
            var last = digest.Centroids().Last();
            Assert.True(last.Count > 1);
            Assert.True(last.Mean() < digest.GetMax());
            Assert.Equal(1.0, digest.Cdf(digest.GetMax() + 1e-9), 0);
            Assert.Equal(1 - 0.5 / digest.Size(), digest.Cdf(digest.GetMax()), 0);
            Assert.Equal(1 - 1.0 / digest.Size(), digest.Cdf((digest.GetMax() - 1e-9)), 10);

            Assert.Equal(4, digest.Quantile(1), 0);
            Assert.Equal(4, digest.Quantile(1 - 0.5 / digest.Size()), 0);
            Assert.Equal(4, digest.Quantile(1 - 1.0 / digest.Size() + 1e-10), 0);
            Assert.Equal(4, digest.Quantile(1 - 1.0 / digest.Size()), 0);
            var slope = 1.0 / (last.Count / 2.0 - 1) * (digest.GetMax() - last.Mean());
            var x = 4 - digest.Quantile(1 - 1.01 / digest.Size());
            Assert.Equal(slope * 0.01, x, 10);
            Assert.Equal(last.Mean(), digest.Quantile(1 - last.Count / 2.0 / digest.Size()), 10);
        }

        [Fact]
        public void SingleMultiRange() {
            var digest = new MergingDigest(10);
            digest.SetScaleFunction(new ScaleFunction.K0());
            for (var i = 0; i < 100; i++) {
                digest.Add(1);
                digest.Add(2);
                digest.Add(3);
            }

            // this check is, of course true, but it also forces merging before we change scale
            Assert.True(digest.CentroidCount() < 300);
            digest.SetScaleFunction(new ScaleFunction.K2());
            digest.Add(0);
            // we now have a digest with a singleton first, then a heavier centroid next
            var ix = digest.Centroids().ToArray();
            var first = ix[0];
            var second = ix[1];
            Assert.Equal(1, first.Count);
            Assert.Equal(0, first.Mean(), 0);
            Assert.True(second.Count > 1);
            Assert.Equal(1.0, second.Mean(), 0);

            Assert.Equal(0.5 / digest.Size(), digest.Cdf(0), 0);
            Assert.Equal(1.0 / digest.Size(), digest.Cdf(1e-10), 10);
            Assert.Equal((1 + second.Count / 8.0) / digest.Size(), digest.Cdf(0.25), 10);
        }

        [Fact]
        public void TestSingleValue() {
            var digest = Factory().Create();
            var value = rng.NextDouble() * 1000;
            digest.Add(value);
            var q = rng.NextDouble();
            foreach (var qValue in new[] {0, q, 1}) {
                Assert.Equal(value, digest.Quantile(qValue), 3);
            }
        }

        [Fact]
        public void TestFewValues() {
            // When there are few values in the tree, quantiles should be exact
            var digest = Factory().Create();
            var length = rng.Next(10);
            var values = new List<double>();
            for (var i = 0; i < length; ++i) {
                double value;
                if (i == 0 || rng.NextDouble() > 0.5) {
                    value = rng.NextDouble() * 100;
                }
                else {
                    // introduce duplicates
                    value = values[i - 1];
                }

                digest.Add(value);
                values.Add(value);
            }

            values.Sort();

            // for this value of the compression, the tree shouldn't have merged any node
            Assert.Equal(digest.Centroids().Count(), values.Count);
            foreach (var q in new double[] {0, 1e-10, rng.NextDouble(), 0.5, 1 - 1e-10, 1}) {
                var q1 = Dist.Quantile(q, values);
                var q2 = digest.Quantile(q);
                Assert.Equal(q1, q2, 4);
            }
        }

        [Fact]
        public void TestEmpty() {
            var digest = Factory().Create();
            var q = rng.NextDouble();
            Assert.True(double.IsNaN(digest.Quantile(q)));
        }

        [Fact]
        public void TestMoreThan2BValues() {
            var digest = Factory().Create();
            for (var i = 0; i < 1000; ++i) {
                var next = rng.NextDouble();
                digest.Add(next);
            }

            for (var i = 0; i < 10; ++i) {
                var next = rng.NextDouble();
                var count = 1 << 28;
                digest.Add(next, count);
            }

            Assert.Equal(1000 + 10L * (1 << 28), digest.Size());
            Assert.True(digest.Size() > int.MaxValue);
            var quantiles = new double[] {0, 0.1, 0.5, 0.9, 1, rng.NextDouble()};
            Array.Sort(quantiles);
            var prev = double.NegativeInfinity;
            foreach (var q in quantiles) {
                var v = digest.Quantile(q);
                Assert.True(v >= prev);
                prev = v;
            }
        }

        [Fact]
        public void TestSorted() {
            var digest = Factory().Create();
            for (var i = 0; i < 10000; ++i) {
                digest.Add(rng.NextDouble(), 1 + rng.Next(10));
            }

            Centroid previous = null;
            foreach (var centroid in digest.Centroids()) {
                if (previous != null) {
                    Assert.True(previous.Mean() <= centroid.Mean());
                }

                previous = centroid;
            }
        }

        [Fact]
        public void TestSingletonInACrowd() {
            const double compression = 100;
            var dist = Factory(compression).Create();
            for (var i = 0; i < 10000; i++) {
                dist.Add(10);
            }

            dist.Add(20);
            dist.Compress();
            Assert.Equal(10.0, dist.Quantile(0), 0);
            Assert.Equal(10.0, dist.Quantile(0.5), 0);
            Assert.Equal(10.0, dist.Quantile(0.8), 0);
            Assert.Equal(10.0, dist.Quantile(0.9), 0);
            Assert.Equal(10.0, dist.Quantile(0.99), 0);
            Assert.Equal(10.0, dist.Quantile(0.999), 0);
            Assert.Equal(20.0, dist.Quantile(1), 0);
        }

        [Fact]
        public void TestThreePointExample() {
            var tdigest = Factory(100).Create();
            var x0 = 0.18615591526031494;
            var x1 = 0.4241943657398224;
            var x2 = 0.8813006281852722;

            tdigest.Add(x0);
            tdigest.Add(x1);
            tdigest.Add(x2);

            var p10 = tdigest.Quantile(0.1);
            var p50 = tdigest.Quantile(0.5);
            var p90 = tdigest.Quantile(0.9);
            var p95 = tdigest.Quantile(0.95);
            var p99 = tdigest.Quantile(0.99);

            Assert.True(p10 <= p50);
            Assert.True(p50 <= p90);
            Assert.True(p90 <= p95);
            Assert.True(p95 <= p99);

            Assert.Equal(x0, p10, 0);
            Assert.Equal(x2, p99, 0);
        }

        [Fact]
        public void TestExtremeQuantiles() {
            // t-digest shouldn't merge extreme nodes, but let's still test how it would
            // answer to extreme quantiles in that case ('extreme' in the sense that the
            // quantile is either before the first node or after the last one)
            var digest = Factory().Create();
            digest.Add(10, 3);
            digest.Add(20, 1);
            digest.Add(40, 5);
            // this group tree is roughly equivalent to the following sorted array:
            // [ ?, 10, ?, 20, ?, ?, 50, ?, ? ]
            // and we expect it to compute approximate missing values:
            // [ 5, 10, 15, 20, 30, 40, 50, 60, 70]
            var values = new List<double>() {5.0, 10.0, 15.0, 20.0, 30.0, 35.0, 40.0, 45.0, 50.0};
            foreach (var q in new[] {1.5 / 9, 3.5 / 9, 6.5 / 9}) {
                Assert.Equal(Dist.Quantile(q, values), digest.Quantile(q), 2);
            }
        }

        [Fact]
        public void TestMonotonicity() {
            var digest = Factory().Create();
            for (var i = 0; i < 100000; i++) {
                digest.Add(rng.NextDouble());
            }

            double lastQuantile = -1;
            double lastX = -1;
            for (double z = 0; z <= 1; z += 1e-5) {
                var x = digest.Quantile(z);
                Assert.True(x >= lastX);
                lastX = x;

                var q = digest.Cdf(z);
                Assert.True(q >= lastQuantile);
                lastQuantile = q;
            }
        }

        [Fact]
        public void TestSerialization() {
            var compression = 20 + rng.NextDouble() * 100;
            var dist = Factory(compression).Create();

            for (var i = 0; i < 100000; i++) {
                var x = rng.NextDouble();
                dist.Add(x);
            }

            dist.Compress();

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            dist.AsBytes(writer);
            Assert.True(writer.BaseStream.Position < 12000);
            Assert.Equal(dist.ByteSize(), writer.BaseStream.Position);

            testOutputHelper.WriteLine($"# big {writer.BaseStream.Position} bytes\n");

            stream.Seek(0, SeekOrigin.Begin);

            using var reader = new BinaryReader(stream);
            var dist2 = fromBytes(reader);
            Assert.Equal(dist.Centroids().Count(), dist2.Centroids().Count());
            Assert.Equal(dist.Compression, dist2.Compression, 4);
            Assert.Equal(dist.CentroidCount(), dist2.CentroidCount());

            for (double q = 0; q < 1; q += 0.01) {
                Assert.Equal(dist.Quantile(q), dist2.Quantile(q), 5);
            }

            using var ix = dist2.Centroids().GetEnumerator();
            foreach (var centroid in dist.Centroids()) {
                Assert.True(ix.MoveNext());
                Assert.Equal(centroid.Count, ix.Current.Count);
            }

            Assert.False(ix.MoveNext());

            stream.Seek(0, SeekOrigin.Begin);
            dist.AsSmallBytes(writer);
            Assert.True(writer.BaseStream.Position < 6000);
            Assert.Equal(dist.SmallByteSize(), writer.BaseStream.Position);
            testOutputHelper.WriteLine($"# small {writer.BaseStream.Position} bytes\n");

            stream.Seek(0, SeekOrigin.Begin);
            dist2 = fromBytes(reader);
            Assert.Equal(dist.Centroids().Count(), dist2.Centroids().Count());
            Assert.Equal(dist.Compression, dist2.Compression, 4);
            Assert.Equal(dist.CentroidCount(), dist2.CentroidCount());

            for (double q = 0; q < 1; q += 0.01) {
                Assert.Equal(dist.Quantile(q), dist2.Quantile(q), 4);
            }

            using var ix2 = dist2.Centroids().GetEnumerator();
            foreach (var centroid in dist.Centroids()) {
                Assert.True(ix2.MoveNext());
                Assert.Equal(centroid.Count, ix2.Current.Count);
            }

            Assert.False(ix2.MoveNext());
        }
    }
}