// See https://aka.ms/new-console-template for more information
using KustoExecutionEngine.Core;

var engine = new KqlEngine();

engine.DumpTree(@"
let d=(10.0 + 1)*todouble(1);
MyTable
| project a = a + b, c = 1
| where a > d");

engine.DumpTree(@"
MyTable
| where a > 1.0");

engine.DumpTree(@"
print 'hello'");

Console.WriteLine("Done");
