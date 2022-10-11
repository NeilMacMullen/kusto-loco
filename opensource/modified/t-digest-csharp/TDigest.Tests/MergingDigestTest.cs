using System.IO;
using Xunit.Abstractions;

namespace TDigest.Tests {
    public class MergingDigestTest : DigestTest {
        protected override IDigestFactory Factory(double compression) {
            return new MergeDigestFactory(compression);
        }

        protected override AbstractTDigest fromBytes(BinaryReader reader) {
            return MergingDigest.FromBytes(reader);
        }

        class MergeDigestFactory : IDigestFactory {
            private double compression;
            public MergeDigestFactory(double compression) {
                this.compression = compression;
            }

            public Digest Create() {
                return new MergingDigest(compression);
            }
        }

        public MergingDigestTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
    }
}