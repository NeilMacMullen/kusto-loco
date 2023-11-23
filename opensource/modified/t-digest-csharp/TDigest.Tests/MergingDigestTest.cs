using System.IO;
using Xunit.Abstractions;

namespace TDigest.Tests;

public class MergingDigestTest : DigestTest
{
    public MergingDigestTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    protected override IDigestFactory Factory(double compression) => new MergeDigestFactory(compression);

    protected override AbstractTDigest fromBytes(BinaryReader reader) => MergingDigest.FromBytes(reader);

    private class MergeDigestFactory : IDigestFactory
    {
        private readonly double compression;
        public MergeDigestFactory(double compression) => this.compression = compression;

        public Digest Create() => new MergingDigest(compression);
    }
}