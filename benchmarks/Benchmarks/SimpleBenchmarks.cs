using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CsvSupport;
using KustoSupport;

namespace Benchmarks
{
    public class SimpleBenchmarks
    {
        private readonly KustoQueryContext _context = new();

        [GlobalSetup]
        public void Setup()
        {
            CsvLoader.Load(@"C:\temp\locations.csv", _context, "data");
        }


        [Benchmark]
        public void Count()
        {
            var query = "data | count";
            var res = _context.BenchmarkQuery(query);
        }


        [Benchmark]
        public int Summarize()
        {
            var query = "data | summarize count() by Radius";
            return _context.BenchmarkQuery(query);
        }

        [Benchmark]
        public int Where()
        {
            var query = "data | where  Radius >200 and Name contains 'CENTRAL'";
            return _context.BenchmarkQuery(query);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            /*
            var c = new Class1();
            c.Setup();
            var res = c.Summarize();
            var s = KustoFormatter
                .Tabulate(KustoQueryContext.GetDictionarySet(res as TabularResult));
            Console.WriteLine($"{s}");
            */
            BenchmarkRunner.Run<SimpleBenchmarks>();
        }
    }
}