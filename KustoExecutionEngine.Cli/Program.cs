// See https://aka.ms/new-console-template for more information
using KustoExecutionEngine.Core;

#if false

var playground = new ParserPlayground();

playground.DumpTree(@"
print 'hello'");

playground.DumpTree(@"
let d=(10.0 + 1)*todouble(1);
MyTable
| project a = a + b, c = 1
| where a > d");

playground.DumpTree(@"
MyTable
| where a > 1+1");

playground.DumpTree(@"
MyTable
| where a > toscalar(MyTable|take 1|project a)");

#else

var engine = new SterlingEngine();
engine.Evaluate("MyTable | where a > 1");

#endif


Console.WriteLine("Done");
