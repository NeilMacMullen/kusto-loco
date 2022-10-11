# T-Digest C#
A .NET Core implementation of T-Digest. This is a nearly line-for-line port of the original Java reference implementation by Ted Dunning. 

The AVL Tree version and histogram classes have not been ported as I do not need them for my own project, but I will welcome any PRs to add missing classes or keep this repo up to date. This version includes the MergingDigest class and most of the relevant unit tests.

## Installation
```
dotnet add package T-Digest --version 1.0.0
```

## What is T-Digest?

A new data structure for accurate on-line accumulation of rank-based statistics such as quantiles and trimmed means. The t-digest algorithm is also very friendly to parallel programs making it useful in map-reduce and parallel streaming applications implemented using, say, Apache Spark.

The t-digest construction algorithm uses a variant of 1-dimensional k-means clustering to produce a very compact data structure that allows accurate estimation of quantiles. This t-digest data structure can be used to estimate quantiles, compute other rank statistics or even to estimate related measures like trimmed means. The advantage of the t-digest over previous digests for this purpose is that the t-digest handles data with full floating point resolution. With small changes, the t-digest can handle values from any ordered set for which we can compute something akin to a mean. The accuracy of quantile estimates produced by t-digests can be orders of magnitude more accurate than those produced by previous digest algorithms in spite of the fact that t-digests are much more compact when stored on disk.

In summary, the particularly interesting characteristics of the t-digest are that it

has smaller summaries when serialized
works on double precision floating point as well as integers.
provides part per million accuracy for extreme quantiles and typically <1000 ppm accuracy for middle quantiles
is very fast (~ 140 ns per add)
is very simple (~ 5000 lines of code total, <1000 for the most advanced implementation alone)
has a reference implementation that has > 90% test coverage
can be used with map-reduce very easily because digests can be merged
requires no dynamic allocation after initial creation (MergingDigest only)
has no runtime dependencies

[Continue Reading...](https://github.com/tdunning/t-digest)

## Usage
```
var digest = new MergingDigest(100);
for (var i = 0; i < 1000; i++) {
  digest.Add(i);
}
digest.Cdf(244); // To get the percentile of any value.
digest.Quantile(99); // To get the value of a specific percentile.
```
