// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
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
        public void Summarize_1()
        {
            // Arrange
            string query = @"
let input = datatable(a:long) [ 1, 2, 3 ];
input
| summarize count() by bin(a, 2)
";

            string expected = @"
a:long; count_:long
------------------
0; 1
2; 2
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
        public void Sort_Desc()
        {
            // Arrange
            string query = @"
datatable(a: long, b: int)
[
    3, 9,
    2, 8,
    1, 7,
    long(null), 42,
    4, 6,
]
| order by a
";

            string expected = @"
a:long; b:int
------------------
4; 6
3; 9
2; 8
1; 7
(null); 42
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Sort_DescNullsFirst()
        {
            // Arrange
            string query = @"
datatable(a: long, b: int)
[
    3, 9,
    2, 8,
    1, 7,
    long(null), 42,
    4, 6,
]
| order by a nulls first
";

            string expected = @"
a:long; b:int
------------------
(null); 42
4; 6
3; 9
2; 8
1; 7
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Sort_AscNullsFirst()
        {
            // Arrange
            string query = @"
datatable(a: double) [ 1.5, 1, double(null), 3 ]
| order by a asc
";

            string expected = @"
a:real
------------------
(null)
1
1.5
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Sort_AscNullsLast()
        {
            // Arrange
            string query = @"
datatable(a: double) [ 1.5, 1, double(null), 3 ]
| order by a asc nulls last
";

            string expected = @"
a:real
------------------
1
1.5
3
(null)
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltInAggregates_countif()
        {
            // Arrange
            string query = @"
datatable(a: bool)
[
    true, true, false, true
]
| summarize v=countif(a)
";

            string expected = @"
v:long
------------------
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltInAggregates_sumif_Long()
        {
            // Arrange
            string query = @"
datatable(v:long, include: bool)
[
    1, true,
    2, false,
    4, true,
    8, true,
]
| summarize v=sumif(v, include)
";

            string expected = @"
v:long
------------------
13
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltInAggregates_any_String()
        {
            // Arrange
            string query = @"
datatable(x:long, val:string)
[
    0, 'first',
    1, 'second',
    2, 'third',
    3, 'fourth',
]
| summarize any(val) by bin(x, 2)
";

            string expected = @"
x:long; any_val:string
------------------
0; first
2; third
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
        public void Union_DifferentAndNonMatchingSchemas1()
        {
            // Arrange
            string query = @"
union
    (datatable(v1:real) [ 1, 2 ]),
    (datatable(v2:real) [ 3, 4 ])
";

            string expected = @"
v1:real; v2:real
------------------
1; (null)
2; (null)
(null); 3
(null); 4
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact(Skip = "Not supported yet, waiting for https://github.com/microsoft/Kusto-Query-Language/issues/65")]
        public void Union_DifferentAndNonMatchingSchemas2()
        {
            // Arrange
            string query = @"
union
    (datatable(v:real) [ 1, 2 ]),
    (datatable(v:long) [ 3, 4 ])
";

            string expected = @"
v_real:real; v_long:real
------------------
1; (null)
2; (null)
(null); 3
(null); 4
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Union_DifferentButMatchingSchemas()
        {
            // Arrange
            string query = @"
union
    (datatable(v:real) [ 1, 2 ]),
    (datatable(v:real) [ 3, 4 ])
";

            string expected = @"
v:real
------------------
1
2
3
4
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
        public void BuiltIns_replace_string_Scalar1()
        {
            // Arrange
            string query = @"
print v=replace_string('abcb', 'b', '1')
";

            string expected = @"
v:string
------------------
a1c1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_replace_string_Columnar()
        {
            // Arrange
            string query = @"
datatable(a:string) [ 'abc', 'abcb', 'def' ]
| project v = replace_string(a, 'b', '1')
";

            string expected = @"
v:string
------------------
a1c
a1c1
def
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_strlen_Scalar()
        {
            // Arrange
            string query = @"
print v=strlen('abc')
";

            string expected = @"
v:long
------------------
3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_strlen_Columnar()
        {
            // Arrange
            string query = @"
datatable(a:string)
[
    'a',
    'abc',
]
| project v = strlen(a)
";

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
        public void BuiltIns_substring_Scalar()
        {
            // Arrange
            string query = @"
print abc1 = substring('abc', 0, 3),
      abc2 = substring('abc', -1, 20),
      bc1  = substring('abc', 1, 2),
      bc2  = substring('abc', 1, 20),
      b1   = substring('abc', 1, 1),
      n1   = substring('abc', 2, 0),
      n2   = substring('abc', 10, 1)
";

            string expected = @"
abc1:string; abc2:string; bc1:string; bc2:string; b1:string; n1:string; n2:string
------------------
abc; abc; bc; bc; b; ;
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_substring_Columnar()
        {
            // Arrange
            string query = @"
datatable(a:string)
[
    '0',
    '01',
    '012',
    '0123',
]
| project v = substring(a,1,2)
";

            string expected = @"
v:string
------------------

1
12
12
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_url_encode_component_Scalar1()
        {
            // Arrange
            string query = @"
print v=url_encode_component('hello world')
";

            string expected = @"
v:string
------------------
hello%20world
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_url_encode_component_Columnar()
        {
            // Arrange
            string query = @"
datatable(a:string) [ 'hello world', 'https://example.com?a=b' ]
| project v = url_encode_component(a)
";

            string expected = @"
v:string
------------------
hello%20world
https%3A%2F%2Fexample.com%3Fa%3Db
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_url_decode_Scalar1()
        {
            // Arrange
            string query = @"
print v=url_decode('hello%20world')
";

            string expected = @"
v:string
------------------
hello world
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_url_decode_Columnar()
        {
            // Arrange
            string query = @"
datatable(a:string) [ 'hello%20world', 'https%3A%2F%2Fexample.com%3Fa%3Db' ]
| project v = url_decode(a)
";

            string expected = @"
v:string
------------------
hello world
https://example.com?a=b
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_bin_Long()
        {
            // Arrange
            string query = @"
datatable(a:long, b:long)
[
  -1, 3,
   0, 3,
   1, 3,
   2, 3,
   3, 3,
   4, 3,
]
| project v1 = bin(a, b), v2 = floor(a, b)";

            string expected = @"
v1:long; v2:long
------------------
-3; -3
0; 0
0; 0
0; 0
3; 3
3; 3
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_bin_Real()
        {
            // Arrange
            string query = @"
datatable(a:real, b:real)
[
  -1, 3,
   0, 3,
   1, 3,
   2, 3,
   3, 3,
   4, 3,
   0.3, 0.5,
   0.5, 0.5,
   0.9, 0.5,
   1.0, 0.5,
   1.1, 0.5,
   -0.1, 0.5,
   -0.5, 0.5,
   -0.6, 0.5,
]
| project v1 = bin(a, b), v2 = floor(a, b)";

            string expected = @"
v1:real; v2:real
------------------
-3; -3
0; 0
0; 0
0; 0
3; 3
3; 3
0; 0
0.5; 0.5
0.5; 0.5
1; 1
1; 1
-0.5; -0.5
-0.5; -0.5
-1; -1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_bin_DateTime()
        {
            // Arrange
            string query = @"
print v=bin(datetime(2022-03-02 23:04), 1h)";

            string expected = @"
v:datetime
------------------
2022-03-02T23:00:00.0000000
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BuiltIns_bin_Narrowing()
        {
            // Arrange
            string query = @"
datatable(a:int) [ 9, 10, 11 ]
| project v = bin(a, 10)";

            string expected = @"
v:long
------------------
0
10
10
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
print a=2-1, b=4-3.5, c=6.5-5, d=8.0-7.5, e=10s-1s, f=datetime(2022-03-06T20:00)-5m
";

            string expected = @"
a:long; b:real; c:real; d:real; e:timespan; f:datetime
------------------
1; 0.5; 1.5; 0.5; 00:00:09; 2022-03-06T19:55:00.0000000
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
print a=6/2, b=5/0.5, c=10./5, d=2.5/0.5, e=15ms/10ms
";

            string expected = @"
a:long; b:real; c:real; d:real; e:real
------------------
3; 10; 2; 5; 1.5
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
        public void BinOp_LogicalAnd()
        {
            // Arrange
            string query = @"
datatable(a:bool, b:bool)
[
    false, false,
    false, true,
    true, false,
    true, true,
]
| project v = a and b
";

            string expected = @"
v:bool
------------------
False
False
False
True
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_LogicalOr()
        {
            // Arrange
            string query = @"
datatable(a:bool, b:bool)
[
    false, false,
    false, true,
    true, false,
    true, true,
]
| project v = a or b
";

            string expected = @"
v:bool
------------------
False
True
True
True
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_LogicalAnd_NullHandling()
        {
            // Arrange
            string query = @"
let nil=bool(null);
print a = nil and nil, b = nil and true, c = nil and false
";

            string expected = @"
a:bool; b:bool; c:bool
------------------
(null); (null); False
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_LogicalOr_NullHandling()
        {
            // Arrange
            string query = @"
let nil=bool(null);
print a = nil or nil, b = nil or true, c = nil or false
";

            string expected = @"
a:bool; b:bool; c:bool
------------------
(null); True; (null)
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_StringContains()
        {
            // Arrange
            string query = @"
datatable(v:string)
[
    'a',
    'ac',
    'bc',
    'BC',
]
| project v = 'abcd' contains v, notV = 'abcd' !contains v
";

            string expected = @"
v:bool; notV:bool
------------------
True; False
False; True
True; False
True; False
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_StringContainsCs()
        {
            // Arrange
            string query = @"
datatable(v:string)
[
    'a',
    'ac',
    'bc',
    'BC',
]
| project v = 'abcd' contains_cs v, notV = 'abcd' !contains_cs v
";

            string expected = @"
v:bool; notV:bool
------------------
True; False
False; True
True; False
False; True
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_StringStartsWith()
        {
            // Arrange
            string query = @"
datatable(v:string)
[
    'a',
    'ab',
    'ABC',
    'bc',
]
| project v = 'abcd' startswith v, notV = 'abcd' !startswith v
";

            string expected = @"
v:bool; notV:bool
------------------
True; False
True; False
True; False
False; True
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_StringStartsWithCs()
        {
            // Arrange
            string query = @"
datatable(v:string)
[
    'a',
    'ab',
    'ABC',
    'bc',
]
| project v = 'abcd' startswith_cs v, notV = 'abcd' !startswith_cs v
";

            string expected = @"
v:bool; notV:bool
------------------
True; False
True; False
False; True
False; True
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_StringEndsWith()
        {
            // Arrange
            string query = @"
datatable(v:string)
[
    'd',
    'cd',
    'BCD',
    'bc',
]
| project v = 'abcd' endswith v, notV = 'abcd' !endswith v
";

            string expected = @"
v:bool; notV:bool
------------------
True; False
True; False
True; False
False; True
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void BinOp_StringEndsWithCs()
        {
            // Arrange
            string query = @"
datatable(v:string)
[
    'd',
    'cd',
    'BCD',
    'bc',
]
| project v = 'abcd' endswith_cs v, notV = 'abcd' !endswith_cs v
";

            string expected = @"
v:bool; notV:bool
------------------
True; False
True; False
False; True
False; True
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

        [Fact]
        public void AggregateFunctionResultKind()
        {
            // Arrange
            string query = @"
datatable(a:long) [ 1, 2, 3 ]
| summarize v=100 * count()
";

            string expected = @"
v:long
------------------
300
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToInt_String_Scalar()
        {
            // Arrange
            string query = @"
print a=toint(''), b=toint('123')
";

            string expected = @"
a:int; b:int
------------------
(null); 123
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToInt_String_Columnar()
        {
            // Arrange
            string query = @"
datatable(v:string) [ '', '123', 'nan' ]
| project a=toint(v)
";

            string expected = @"
a:int
------------------
(null)
123
(null)
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToLong_String_Scalar()
        {
            // Arrange
            string query = @"
print a=tolong(''), b=tolong('123')
";

            string expected = @"
a:long; b:long
------------------
(null); 123
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToLong_String_Columnar()
        {
            // Arrange
            string query = @"
datatable(v:string) [ '', '123', 'nan' ]
| project a=tolong(v)
";

            string expected = @"
a:long
------------------
(null)
123
(null)
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToLong_Real_Scalar()
        {
            // Arrange
            string query = @"
print a=tolong(123.5)
";

            string expected = @"
a:long
------------------
123
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToDouble_String_Scalar()
        {
            // Arrange
            string query = @"
print a=todouble(''), b=todouble('123.5')
";

            string expected = @"
a:real; b:real
------------------
(null); 123.5
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToDouble_String_Columnar()
        {
            // Arrange
            string query = @"
datatable(v:string) [ '', '123.5', 'nan' ]
| project a=todouble(v)
";

            string expected = @"
a:real
------------------
(null)
123.5
NaN
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToReal_String_Scalar()
        {
            // Arrange
            string query = @"
print a=toreal(''), b=toreal('123.5')
";

            string expected = @"
a:real; b:real
------------------
(null); 123.5
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Cast_ToReal_String_Columnar()
        {
            // Arrange
            string query = @"
datatable(v:string) [ '', '123.5', 'nan' ]
| project a=toreal(v)
";

            string expected = @"
a:real
------------------
(null)
123.5
NaN
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Iff_Scalar()
        {
            // Arrange
            string query = @"
print 
      bool1 = iff(2 > 1, true, false),
      bool2 = iif(2 < 1, true, false),
      int1  = iff(2 > 1, int(1), int(2)),
      int2  = iff(2 < 1, int(1), int(2)),
      long1 = iff(2 > 1, long(1), long(2)),
      long2 = iff(2 < 1, long(1), long(2)),
      real1 = iff(2 > 1, real(1), real(2)),
      real2 = iff(2 < 1, real(1), real(2)),
      string1 = iff(2 > 1, 'ifTrue', 'ifFalse'),
      string2 = iff(2 < 1, 'ifTrue', 'ifFalse'),
      datetime1 = iff(2 > 1, datetime(2022-01-01), datetime(2022-01-02)),
      datetime2 = iff(2 < 1, datetime(2022-01-01), datetime(2022-01-02)),
      timespan1 = iff(2 > 1, 1s, 2s),
      timespan2 = iff(2 < 1, 1s, 2s)
";

            string expected = @"
bool1:bool; bool2:bool; int1:int; int2:int; long1:long; long2:long; real1:real; real2:real; string1:string; string2:string; datetime1:datetime; datetime2:datetime; timespan1:timespan; timespan2:timespan
------------------
True; False; 1; 2; 1; 2; 1; 2; ifTrue; ifFalse; 2022-01-01T00:00:00.0000000; 2022-01-02T00:00:00.0000000; 00:00:01; 00:00:02
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Iff_Columnar()
        {
            // Arrange
            string query = @"
datatable(predicates:bool) [ true, false ]
| project
      bool1 = iff(predicates, true, false),
      int1  = iff(predicates, int(1), int(2)),
      long1 = iff(predicates, long(1), long(2)),
      real1 = iff(predicates, real(1), real(2)),
      string1 = iff(predicates, 'ifTrue', 'ifFalse'),
      datetime1 = iff(predicates, datetime(2022-01-01), datetime(2022-01-02)),
      timespan1 = iff(predicates, 1s, 2s)
";

            string expected = @"
bool1:bool; int1:int; long1:long; real1:real; string1:string; datetime1:datetime; timespan1:timespan
------------------
True; 1; 1; 1; ifTrue; 2022-01-01T00:00:00.0000000; 00:00:01
False; 2; 2; 2; ifFalse; 2022-01-02T00:00:00.0000000; 00:00:02
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Window_RowCumSum_SingleChunk()
        {
            // Arrange
            string query = @"
datatable(v:long) [ 1, 2, 3, 4 ]
| project cs = row_cumsum(v, false)
";

            string expected = @"
cs:long
------------------
1
3
6
10
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Window_RowCumSum_TwoChunks()
        {
            // Arrange
            string query = @"
let t = datatable(v:long) [ 1, 2, 3 ];
union t,t
| project cs = row_cumsum(v, false)
";

            string expected = @"
cs:long
------------------
1
3
6
7
9
12
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Window_RowCumSum_Restart()
        {
            // Arrange
            string query = @"
datatable(v:int, r:bool)
[
    1, false,
    2, false,
    3, true,
    4, false,
]
| project cs = row_cumsum(v, r)
";

            string expected = @"
cs:int
------------------
1
3
3
7
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact(Skip = "Known bug in window function implementations where a table is evaluated more than once. For now, using materialize() works around this")]
        public void Window_RowCumSum_MultipleEvaluations()
        {
            // Arrange
            string query = @"
let d=
    datatable(v:int) [ 10, 10 ]
    | project cs = row_cumsum(v, false);
let a = toscalar(d | summarize max(cs));
d
| extend normalized = todouble(cs) / a
";

            string expected = @"
cs:int; normalized:real
------------------
10; 0.5
20; 1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Materialize()
        {
            // Arrange
            string query = @"
let d = materialize(
    datatable(v:int) [ 10, 10 ]
    | project cs = row_cumsum(v, false)
);
let a = toscalar(d | summarize max(cs));
d
| extend normalized = todouble(cs) / a
";

            string expected = @"
cs:int; normalized:real
------------------
10; 0.5
20; 1
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_DefaultJoin()
        {
            // Arrange
            string query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join Y on Key
| order by Key asc, Value2 asc
";

            string expected = @"
Key:string; Value1:long; Key1:string; Value2:long
------------------
b; 2; b; 10
c; 4; c; 20
c; 4; c; 30
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_InnerUniquetJoin()
        {
            // Arrange
            string query = @"
let t1 = datatable(key:long, value:string)
[
    1, 'val1.1',
    1, 'val1.2'
];
let t2 = datatable(key:long, value:string)
[
    1, 'val1.3',
    1, 'val1.4'
];
t1 | join kind=innerunique t2 on key
| order by value1 asc
";

            string expected = @"
key:long; value:string; key1:long; value1:string
------------------
1; val1.1; 1; val1.3
1; val1.1; 1; val1.4
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_InnerJoin()
        {
            // Arrange
            string query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind=inner Y on Key
| order by Key asc, Value1 asc, Value2 asc
";

            string expected = @"
Key:string; Value1:long; Key1:string; Value2:long
------------------
b; 2; b; 10
b; 3; b; 10
c; 4; c; 20
c; 4; c; 30
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact(Skip = "$left, $right scopes not properly supported yet")]
        public void Join_InnerJoin_LeftRightScopesOnClause()
        {
            // Arrange
            string query = @"
let me = 'baby';
let A = datatable(a:string, b:string) [
    'abc', 'aLeft',
    'def', 'dLeft',
    'ghi', 'gLeft',
];
let B = datatable(a:string, c:string) [
    'abc', 'aRight',
    'def', 'dRight',
    'jkl', 'jRight',
];
A | join kind=inner B on $left.a == $right.a
| order by a asc
";

            string expected = @"
a:string; b:string; a1:string; c:string
------------------
abc; aLeft; abc; aRight
def; dLeft; def; dRight
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_LeftOuterJoin1()
        {
            // Arrange
            string query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind=leftouter Y on Key
| order by Key asc, Key1 asc
";

            string expected = @"
Key:string; Value1:long; Key1:string; Value2:long
------------------
a; 1; (null); (null)
b; 2; b; 10
b; 3; b; 10
c; 4; c; 20
c; 4; c; 30
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_RightOuterJoin1()
        {
            // Arrange
            string query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind=rightouter Y on Key
| order by Key asc nulls last, Key1 asc
";

            string expected = @"
Key:string; Value1:long; Key1:string; Value2:long
------------------
b; 2; b; 10
b; 3; b; 10
c; 4; c; 20
c; 4; c; 30
(null); (null); d; 40
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_FullOuterJoin1()
        {
            // Arrange
            string query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind=fullouter Y on Key
| order by Key asc nulls last, Key1 asc nulls first
";

            string expected = @"
Key:string; Value1:long; Key1:string; Value2:long
------------------
a; 1; (null); (null)
b; 2; b; 10
b; 3; b; 10
c; 4; c; 20
c; 4; c; 30
(null); (null); d; 40
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_LeftSemiJoin1()
        {
            // Arrange
            string query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind=leftsemi Y on Key
| order by Key asc
";

            string expected = @"
Key:string; Value1:long
------------------
b; 2
b; 3
c; 4
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void Join_RightSemiJoin1()
        {
            // Arrange
            string query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind=rightsemi Y on Key
| order by Key asc
";

            string expected = @"
Key:string; Value2:long
------------------
b; 10
c; 20
c; 30
";

            // Act & Assert
            Test(query, expected);
        }

        [Theory]
        [InlineData("leftanti")]
        [InlineData("anti")]
        [InlineData("leftantisemi")]
        public void Join_LeftAntiJoin1(string kind)
        {
            // Arrange
            string query = $@"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind={kind} Y on Key
";

            string expected = @"
Key:string; Value1:long
------------------
a; 1
";

            // Act & Assert
            Test(query, expected);
        }

        [Theory]
        [InlineData("rightanti")]
        [InlineData("rightantisemi")]
        public void Join_RightAntiJoin1(string kind)
        {
            // Arrange
            string query = $@"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4
];
let Y = datatable(Key:string, Value2:long)
[
    'b',10,
    'c',20,
    'c',30,
    'd',40
];
X | join kind={kind} Y on Key
";

            string expected = @"
Key:string; Value2:long
------------------
d; 40
";

            // Act & Assert
            Test(query, expected);
        }

        private static void Test(string query, string expectedOutput)
        {
            var engine = new BabyKustoEngine();
            var result = (TabularResult?)engine.Evaluate(query);
            Debug.Assert(result != null);
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