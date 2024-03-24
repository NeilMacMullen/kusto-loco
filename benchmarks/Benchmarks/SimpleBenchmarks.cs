using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using KustoLoco.FileFormats;
using KustoLoco.Core;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class SimpleBenchmarks
    {
        private KustoQueryContext _context ;

        [GlobalSetup]
        public void Setup()
        {
            _context = new KustoQueryContext();
            CsvLoader.Load(@"C:\temp\locations.csv",  "data");
        }


        [Benchmark]
        public void Count()
        {
            var query = "data | count";
            var res = _context.BenchmarkQuery(query);
        }

         [Benchmark]
        public int SummarizeCount()
        {
            var query = "data | summarize count() by LocationSetTagString | count ";
            return _context.BenchmarkQuery(query);
        }

        [Benchmark]
        public int SummarizeSimpleCount()
        {
            var query = "data | summarize by LocationSetTagString | count";
            return _context.BenchmarkQuery(query);
        }

        [Benchmark]
        public int SummarizeSimpleCountInt()
        {
            var query = "data | summarize by Radius | count";
            return _context.BenchmarkQuery(query);
        }


        [Benchmark]
        public int Summarize()
        {
            var query = "data | summarize count() by LocationSetTagString";
            return _context.BenchmarkQuery(query);
        }

        [Benchmark]
        public int Where()
        {
            var query = "data | where Name contains 'CENTRAL' | count";
            return _context.BenchmarkQuery(query);
        }

        [Benchmark]
        public int Distance()
        {
            var query = "data | where geo_distance_2points(Longitude,Latitude, 0.19686015028158654,52.26279460357216) < 10000 | count";
            return _context.BenchmarkQuery(query);
        }
		
		[Benchmark]
        public int Extend()
        {
            var query = "data | extend d=Latitude+Longitude | count";
            return _context.BenchmarkQuery(query);
        }

        [Benchmark]
        public int ExtendDistance()
        {
            var query = "data | extend d=geo_distance_2points(Longitude,Latitude, 0.19686015028158654,52.26279460357216) | where d < 10000 | count";
            return _context.BenchmarkQuery(query);
        }

    }

    public class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<SimpleBenchmarks>();
        }
    }
}