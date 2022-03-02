// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using FluentAssertions;
using Xunit;

namespace KustoExecutionEngine.Core.Tests
{
    public class EndToEndTests
    {
        [Fact]
        public void Print1()
        {
            // Arrange
            string query = @"
print 1
";

            string expected = @"
print_0:long
------------------
1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Print2()
        {
            // Arrange
            string query = @"
print v=1
";

            string expected = @"
v:long
------------------
1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Print3()
        {
            // Arrange
            string query = @"
print a=3, b=1, 1+1
";

            string expected = @"
a:long; b:long; print_2:long
------------------
3; 1; 2
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void SimpleDataTable_Works()
        {
            // Arrange
            string query = @"
datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
    'vm1', 'mem', 5,
    'vm2', 'cpu', 100,
]
";

            string expected = @"
AppMachine:string; CounterName:string; CounterValue:real
------------------
vm0; cpu; 50
vm0; mem; 30
vm1; cpu; 20
vm1; mem; 5
vm2; cpu; 100
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void SimpleDataTableWithVariable_Works()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
];
input
";

            string expected = @"
AppMachine:string; CounterName:string; CounterValue:real
------------------
vm0; cpu; 50
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Project1()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:long)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
    'vm1', 'mem', 5,
    'vm2', 'cpu', 100,
];
input
| project AppMachine, plus1 = CounterValue + 1, CounterValue + 2
";

            string expected = @"
AppMachine:string; plus1:long; Column1:long
------------------
vm0; 51; 52
vm0; 31; 32
vm1; 21; 22
vm1; 6; 7
vm2; 101; 102
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Project_ColumnizesScalar()
        {
            // Arrange
            string query = @"
datatable(a:long) [ 1, 2 ]
| project a, b=1
";

            string expected = @"
a:long; b:long
------------------
1; 1
2; 1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Summarize_NoByExpressions1()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| summarize count()
";

            string expected = @"
count_:long
------------------
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Summarize_NoByExpressions2()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 43,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| summarize vAvg=avg(CounterValue), vCount=count(), vSum=sum(CounterValue)
";

            string expected = @"
vAvg:real; vCount:long; vSum:real
------------------
31; 3; 93
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Take_Works()
        {
            // Arrange
            string query = @"
let input = datatable(v:real)
[
    1, 2, 3, 4, 5
];
input
| take 3
";

            string expected = @"
v:real
------------------
1
2
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Count_Works()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| take 2
| count
";

            string expected = @"
Count:long
------------------
2
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void CountAs_Works()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| count as abc
";

            string expected = @"
abc:long
------------------
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Distinct_OneColumn()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| distinct AppMachine
";

            string expected = @"
AppMachine:string
------------------
vm0
vm1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Distinct_TwoColumns()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| distinct AppMachine, CounterName
";

            string expected = @"
AppMachine:string; CounterName:string
------------------
vm0; cpu
vm0; mem
vm1; cpu
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Distinct_Star()
        {
            // Arrange
            string query = @"
let input = datatable(AppMachine:string, CounterName:string)
[
    'vm0', 'cpu',
    'vm1', 'cpu',
    'vm0', 'cpu',
    'vm0', 'mem',
];
input
| distinct *
";

            string expected = @"
AppMachine:string; CounterName:string
------------------
vm0; cpu
vm1; cpu
vm0; mem
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Union_WithLeftExpression()
        {
            // Arrange
            string query = @"
let input = datatable(v:real)
[
    1, 2,
];
input
| union input
";

            string expected = @"
v:real
------------------
1
2
1
2
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Union_NoLeftExpression()
        {
            // Arrange
            string query = @"
let input = datatable(v:real)
[
    1, 2,
];
union input, input
";

            string expected = @"
v:real
------------------
1
2
1
2
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_minof_Scalar()
        {
            // Arrange
            string query = @"
print v=min_of(3,2)";

            string expected = @"
v:long
------------------
2
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_minof_Columnar()
        {
            // Arrange
            string query = @"
datatable(a:long, b:long)
[
   2, 1,
   3, 4,
]
| project v = min_of(a, b)";

            string expected = @"
v:long
------------------
1
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_minof_TypeCoercions()
        {
            // Arrange
            string query = @"
print v=min_of(1.5, 2)
";

            string expected = @"
v:real
------------------
1.5
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact(Skip = "We only support overload with 2 args for now")]
        public void BuiltIns_minof_ManyArgs()
        {
            // Arrange
            string query = @"
print v=min_of(4,3,2,1.0)";

            string expected = @"
v:real
------------------
1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_strcat_Scalar1()
        {
            // Arrange
            string query = @"
print v=strcat('a')
";

            string expected = @"
v:string
------------------
a
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_strcat_Scalar2()
        {
            // Arrange
            string query = @"
print v=strcat('a', 'b')
";

            string expected = @"
v:string
------------------
ab
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_strcat_Scalar3_CoercesToString()
        {
            // Arrange
            string query = @"
print v=strcat('a', '-', 123)
";

            string expected = @"
v:string
------------------
a-123
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_strcat_Columnar()
        {
            // Arrange
            string query = @"
datatable(a:string, b:long)
[
    'a', 123,
    'b', 456,
]
| project v = strcat(a, '-', b)
";

            string expected = @"
v:string
------------------
a-123
b-456
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void UserDefinedFunction1()
        {
            // Arrange
            string query = @"
let f=(a: long) { a + 1 };
datatable(v:long) [ 1, 2, 3 ]
| project v = f(v + 1)
";

            string expected = @"
v:long
------------------
3
4
5
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void UserDefinedFunction2()
        {
            // Arrange
            string query = @"
let f=(t:(v:long)) { t | project v };
f((datatable(v:long) [ 1, 2, 3 ]))
";

            string expected = @"
v:long
------------------
1
2
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void UserDefinedFunction3()
        {
            // Arrange
            string query = @"
let f=(t:(v:long), c:long) { t | project v = v+c };
f((datatable(v:long) [ 1, 2, 3 ]), 1)
";

            string expected = @"
v:long
------------------
2
3
4
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void UserDefinedFunction4()
        {
            // Arrange
            string query = @"
let f=(a:real) { a + 0.5 };
print v=f(1)
";

            string expected = @"
v:real
------------------
1.5
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void UnaryOp_Minus1()
        {
            // Arrange
            string query = @"
print a = -1, b = 1 + -3.0
";

            string expected = @"
a:long; b:real
------------------
-1; -2
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Add1()
        {
            // Arrange
            string query = @"
print a=1+2
";

            string expected = @"
a:long
------------------
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Add2()
        {
            // Arrange
            string query = @"
print a=1+2, b=3+4.0, c=5.0+6, d=7.0+8.0
";

            string expected = @"
a:long; b:real; c:real; d:real
------------------
3; 7; 11; 15
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Add3()
        {
            // Arrange
            string query = @"
let c=1;
datatable(a:long) [ 1, 2, 3 ]
| project v = a + c
";

            string expected = @"
v:long
------------------
2
3
4
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Subtract1()
        {
            // Arrange
            string query = @"
print a=2-1, b=4-3.5, c=6.5-5, d=8.0-7.5
";

            string expected = @"
a:long; b:real; c:real; d:real
------------------
1; 0.5; 1.5; 0.5
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Multiply1()
        {
            // Arrange
            string query = @"
print a=2*1, b=4*3.5, c=6.5*5, d=8.0*7.5
";

            string expected = @"
a:long; b:real; c:real; d:real
------------------
2; 14; 32.5; 60
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Divide1()
        {
            // Arrange
            string query = @"
print a=6/2, b=5/0.5, c=10./5, d=2.5/0.5
";

            string expected = @"
a:long; b:real; c:real; d:real
------------------
3; 10; 2; 5
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Modulo1()
        {
            // Arrange
            string query = @"
datatable(a:long, b:long)
[
    4, 5,
    5, 5,
    6, 5,
    -1, 4,
]
| project v = a % b
";

            string expected = @"
v:long
------------------
4
0
1
-1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_GreaterThan1()
        {
            // Arrange
            string query = @"
datatable(a:long, b:real)
[
    1, 1.5,
    2, 1.5,
]
| project v = a > b, w = b > a
";

            string expected = @"
v:bool; w:bool
------------------
False; True
True; False
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_GreaterThan2()
        {
            // Arrange
            string query = @"
print a = 2 > 1, b = 1 > 2, c = 1.5 > 2, d = 2 > 1.5
";

            string expected = @"
a:bool; b:bool; c:bool; d:bool
------------------
True; False; False; True
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Equal1()
        {
            // Arrange
            string query = @"
datatable(a:long) [ 1, 2, 3 ]
| where a == 2
";

            string expected = @"
a:long
------------------
2
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_Equal2()
        {
            // Arrange
            string query = @"
datatable(v:string) [ 'a', 'b' ]
| where v == 'a'
";

            string expected = @"
v:string
------------------
a
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_NotEqual1()
        {
            // Arrange
            string query = @"
datatable(a:long) [ 1, 2, 3]
| where a != 2
";

            string expected = @"
a:long
------------------
1
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_NotEqual2()
        {
            // Arrange
            string query = @"
datatable(v:string) [ 'a', 'b' ]
| where v != 'a'
";

            string expected = @"
v:string
------------------
b
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void ToScalar_Tabular()
        {
            // Arrange
            string query = @"
print v=toscalar(print a=1,b=2)
";

            string expected = @"
v:long
------------------
1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void ToScalar_Scalar()
        {
            // Arrange
            string query = @"
print v=toscalar(1.5)
";

            string expected = @"
v:real
------------------
1.5
";

            // Act & Assert
            Test(query, expected);
        }

        private static void Test(string query, string expectedOutput)
        {
            var engine = new BabyKustoEngine();
            var result = (TabularResult)engine.Evaluate(query);
            var stringified = result.Value.DumpToString();

            var canonicalOutput = stringified.Trim().Replace("\r\n", "\n");
            var canonicalExpectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");

            canonicalOutput.Should().Be(canonicalExpectedOutput);
        }

#if false
        [Fact]
        public void Union_Works()
        {
            // Arrange
            var engine = new BabyKustoEngine();
            var query = @"
union
  (datatable(a:real) [ 1, 2 ]),
  (datatable(a:real) [ 3, 4 ])
";

            // Act
            var result = engine.Evaluate(query) as ITableSource;

            // Assert
            result.Should().NotBeNull();
            var dumped = result!.DumpToString();
            dumped.Should().Be(
                "a; " + Environment.NewLine +
                "------------------" + Environment.NewLine +
                "1; " + Environment.NewLine +
                "2; " + Environment.NewLine +
                "3; " + Environment.NewLine +
                "4; " + Environment.NewLine);
        }

        [Fact]
        public void Union_Works2()
        {
            // Arrange
            var engine = new BabyKustoEngine();
            var query = @"
union
  (datatable(a:real) [ 1, 2 ]),
  (datatable(a:long) [ 3, 4 ])
";

            // Act
            var result = engine.Evaluate(query) as ITableSource;

            // Assert
            result.Should().NotBeNull();
            var dumped = result!.DumpToString();
            dumped.Should().Be(
                "a_real; a_long; " + Environment.NewLine +
                "------------------" + Environment.NewLine +
                "1; ; " + Environment.NewLine +
                "2; ; " + Environment.NewLine +
                "; 3; " + Environment.NewLine +
                "; 4; " + Environment.NewLine);
        }

        [Fact]
        public void Union_Works3()
        {
            // Arrange
            var engine = new BabyKustoEngine();
            var query = @"
datatable(a:real) [ 1, 2 ]
| union (datatable(a:long) [ 3, 4 ])
";

            // Act
            var result = engine.Evaluate(query) as ITableSource;

            // Assert
            result.Should().NotBeNull();
            var dumped = result!.DumpToString();
            dumped.Should().Be(
                "a_real; a_long; " + Environment.NewLine +
                "------------------" + Environment.NewLine +
                "1; ; " + Environment.NewLine +
                "2; ; " + Environment.NewLine +
                "; 3; " + Environment.NewLine +
                "; 4; " + Environment.NewLine);
        }

        [Fact]
        public void Example1_Works()
        {
            // Arrange
            var engine = new BabyKustoEngine();
            engine.AddGlobalTable("MyTable", GetSampleData());
            var query = @"
let c=100.0;
MyTable
| project frac=CounterValue/c, AppMachine, CounterName
| summarize avg(frac) by CounterName
| project CounterName, avgRoundedPercent=tolong(avg_frac*100)
";

            // Act
            var result = engine.Evaluate(query) as ITableSource;

            // Assert
            result.Should().NotBeNull();
            var dumped = result!.DumpToString();
            dumped.Should().Be(
                "CounterName; avgRoundedPercent; " + Environment.NewLine +
                "------------------" + Environment.NewLine +
                "cpu; 57; " + Environment.NewLine +
                "mem; 18; " + Environment.NewLine);
        }

        private static ITableSource GetSampleData()
        {
            @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
    'vm1', 'mem', 5,
    'vm2', 'cpu', 100,
];
input
"
            return new InMemoryTableSource(
                new TableSchema(
                    new List<ColumnDefinition>()
                    {
                        new ColumnDefinition("AppMachine",   KustoValueKind.String),
                        new ColumnDefinition("CounterName",  KustoValueKind.String),
                        new ColumnDefinition("CounterValue", KustoValueKind.Real),
                    }),
                    new[]
                    {
                        new Column(new object?[] { "vm0", "vm0", "vm1", "vm1", "vm2" }),
                        new Column(new object?[] { "cpu", "mem", "cpu", "mem", "cpu" }),
                        new Column(new object?[] {  50.0,  30.0,  20.0,  5.0,   100.0 }),
                    });
        }
#endif
    }
}