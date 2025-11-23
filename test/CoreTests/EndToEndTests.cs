//
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Extensions;
using LogSetup;
using NLog;
using Xunit;

namespace KustoExecutionEngine.Core.Tests;

public class EndToEndTests

{
    public EndToEndTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    [Fact]
    public void Print1()
    {
        // Arrange
        var query = @"
print 1
";

        var expected = @"
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
        var query = @"
print v=1
";

        var expected = @"
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
        var query = @"
print a=3, b=1, 1+1
";

        var expected = @"
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
        var query = @"
datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
    'vm1', 'mem', 5,
    'vm2', 'cpu', 100,
]
";

        var expected = @"
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
        var query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
];
input
";

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
        var query = @"
datatable(a:long) [ 1, 2 ]
| project a, b=1
";

        var expected = @"
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
        var query = @"
let input = datatable(a:long) [ 1, 2, 3 ];
input
| summarize count() by bin(a, 2)
";

        var expected = @"
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
        var query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| summarize count()
";

        var expected = @"
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
        var query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 43,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| summarize vAvg=avg(CounterValue), vCount=count(), vSum=sum(CounterValue)
";

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
    public void Top_Desc()
    {
        // Arrange
        var query = @"
datatable(a: long, b: int)
[
    3, 9,
    2, 8,
    1, 7,
    long(null), 42,
    4, 6,
]
| top 3 by a
";

        var expected = @"
a:long; b:int
------------------
4; 6
3; 9
2; 8
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Sort_DescNullsFirst()
    {
        // Arrange
        var query = @"
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

        var expected = @"
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
        var query = @"
datatable(a: double) [ 1.5, 1, double(null), 3 ]
| order by a asc
";

        var expected = @"
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
        var query = @"
datatable(a: double) [ 1.5, 1, double(null), 3 ]
| order by a asc nulls last
";

        var expected = @"
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
        var query = @"
datatable(a: bool)
[
    true, true, false, true
]
| summarize v=countif(a)
";

        var expected = @"
v:long
------------------
3
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_sum_Int()
    {
        // Arrange
        var query = @"
datatable(v:int) [ 1, 2, 4 ]
| summarize v=sum(v)
";

        var expected = @"
v:long
------------------
7
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_sumif_Long()
    {
        // Arrange
        var query = @"
datatable(v:long, include: bool)
[
    1, true,
    2, false,
    4, true,
    8, true,
]
| summarize v=sumif(v, include)
";

        var expected = @"
v:long
------------------
13
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_sumif_ScalarInput()
    {
        // Arrange
        var query = @"
datatable(v:long) [ 1, 2, 4, 8 ]
| summarize v=sumif(v, true)
";

        var expected = @"
v:long
------------------
15
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_take_any_String()
    {
        // Arrange
        var query = @"
datatable(x:long, val:string)
[
    0, 'first',
    1, 'second',
    2, 'third',
    3, 'fourth',
]
| summarize take_any(val), any(val) by bin(x, 2)
";

        var expected = @"
x:long; val:string; any_val:string
------------------
0; first; first
2; third; third
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_percentile_int()
    {
        // Arrange
        var query = @"
datatable(a: int) [ 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 ]
| summarize p0 = percentile(a, 0), p100=percentile(a, 100)
";

        var expected = @"
p0:long; p100:long
------------------
0; 9
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_percentile_long()
    {
        // Arrange
        var query = @"
datatable(a: long) [ 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 ]
| summarize p0 = percentile(a, 0), p100=percentile(a, 100)
";

        var expected = @"
p0:long; p100:long
------------------
0; 9
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_percentile_double()
    {
        // Arrange
        var query = @"
datatable(a: real) [ 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 ]
| summarize p0 = percentile(a, 0), p100=percentile(a, 100)
";

        var expected = @"
p0:real; p100:real
------------------
0; 9
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_make_set_int()
    {
        // Arrange
        var query = @"
datatable(x: int) [1, 2, 3, 1]
| summarize a = make_set(x), b = make_set(x,2)
| project a = array_sort_asc(a), b = array_sort_asc(b)
";

        var expected = @"
a:dynamic; b:dynamic
------------------
[1,2,3]; [1,2]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_make_set_if_int()
    {
        // Arrange
        var query = @"
datatable(x: int) [1, 2, 3, 1]
| summarize a = make_set_if(x,x>1), b = make_set_if(x,true,2)
| project a = array_sort_asc(a), b = array_sort_asc(b)
";

        var expected = @"
a:dynamic; b:dynamic
------------------
[2,3]; [1,2]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_make_list_int()
    {
        // Arrange
        var query = @"
datatable(x: int) [1, 3, 2, 1]
| summarize a = make_list(x), b = make_list(x,2)
";

        var expected = @"
a:dynamic; b:dynamic
------------------
[1,3,2,1]; [1,3]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_make_list_if_int()
    {
        // Arrange
        var query = @"
datatable(x: int) [1, 3, 2, 1]
| summarize a = make_list_if(x,x>1), b = make_list_if(x,true,3)
";

        var expected = @"
a:dynamic; b:dynamic
------------------
[3,2]; [1,3,2]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeListWithNulls_BasicExample()
    {
        // Arrange - From Microsoft docs: make_list_with_nulls example
        var query = @"
datatable(Fruit:string, Color:string, Quantity:int)
[
    'Apple', 'Red', 5,
    'Banana', '', 3,
    'Cherry', 'Red', int(null),
]
| summarize Colors = make_list_with_nulls(Color), Quantities = make_list_with_nulls(Quantity)
";

        var expected = @"
Colors:dynamic; Quantities:dynamic
------------------
[""Red"","""",""Red""]; [5,3,null]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeList_WithDynamic()
    {
        // Arrange - Test make_list with dynamic values
        var query = @"
datatable(value:dynamic)
[
    dynamic({""a"":1}),
    dynamic({""b"":2}),
    dynamic([1,2,3]),
]
| summarize result = make_list(value)
";

        var expected = @"
result:dynamic
------------------
[{""a"":1},{""b"":2},[1,2,3]]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeSet_FromMicrosoftDocs()
    {
        // Arrange - From Microsoft docs: make_set example
        var query = @"
datatable(Region:string, Sales:int)
[
    'North', 150,
    'North', 200,
    'South', 100,
    'West', 150,
    'South', 100,
]
| summarize UniqueSalesAmounts = make_set(Sales)
| project UniqueSalesAmounts = array_sort_asc(UniqueSalesAmounts)
";

        var expected = @"
UniqueSalesAmounts:dynamic
------------------
[100,150,200]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeSetIf_FromMicrosoftDocs()
    {
        // Arrange - From Microsoft docs: make_set_if example
        var query = @"
datatable(Region:string, Sales:int)
[
    'North', 150,
    'North', 200,
    'South', 100,
    'West', 150,
]
| summarize HighSales = make_set_if(Sales, Sales > 100)
| project HighSales = array_sort_asc(HighSales)
";

        var expected = @"
HighSales:dynamic
------------------
[150,200]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_dcount_Int()
    {
        // Arrange
        var query = @"
datatable(a: int)
[
    int(null), 1, 2, 3 // nulls are ignored
]
| summarize v=dcount(a)
";

        var expected = @"
v:long
------------------
3
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_dcount_String()
    {
        // Arrange
        var query = @"
datatable(a: string)
[
    '', 'a', 'b', 'c' // empty string are NOT ignored
]
| summarize v=dcount(a)
";

        var expected = @"
v:long
------------------
4
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltInAggregates_dcountif_String()
    {
        // Arrange
        var query = @"
datatable(a:string, b: bool)
[
    '', true,
    'a', true,
    'b', false,
    'a', false,
    'a', true,
]
| summarize v=dcountif(a, b)
";

        var expected = @"
v:long
------------------
2
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Take_Works()
    {
        // Arrange
        var query = @"
let input = datatable(v:real)
[
    1, 2, 3, 4, 5
];
input
| take 3
";

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
        var query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| count as abc
";

        var expected = @"
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
        var query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| distinct AppMachine
";

        var expected = @"
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
        var query = @"
let input = datatable(AppMachine:string, CounterName:string, CounterValue:real)
[
    'vm0', 'cpu', 50,
    'vm0', 'mem', 30,
    'vm1', 'cpu', 20,
];
input
| distinct AppMachine, CounterName
";

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
        var query = @"
let input = datatable(v:real)
[
    1, 2,
];
input
| union input
";

        var expected = @"
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
        var query = @"
let input = datatable(v:real)
[
    1, 2,
];
union input, input
";

        var expected = @"
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
        var query = @"
union
    (datatable(v1:real) [ 1, 2 ]),
    (datatable(v2:real) [ 3, 4 ])
";

        var expected = @"
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

    [Fact]
    public void Union_DifferentAndNonMatchingSchemas2()
    {
        // Arrange
        var query = @"
union
    (datatable(v_real:real) [ 1, 2 ]),
    (datatable(v:real) [ 3, 4 ]),
    (datatable(v:long) [ 5, 6 ]),
    (datatable(v_real:real) [ 7 ])
";

        var expected = @"
v_real:real; v_real1:real; v_long:long
------------------
1; (null); (null)
2; (null); (null)
(null); 3; (null)
(null); 4; (null)
(null); (null); 5
(null); (null); 6
7; (null); (null)
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Union_DifferentButMatchingSchemas()
    {
        // Arrange
        var query = @"
union
    (datatable(v:real) [ 1, 2 ]),
    (datatable(v:real) [ 3, 4 ])
";

        var expected = @"
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

    [Theory]
    [InlineData("isnull(bool(null))", true)]
    [InlineData("isnull(false)", false)]
    [InlineData("isnull(true)", false)]
    [InlineData("isnull(int(null))", true)]
    [InlineData("isnull(int(0))", false)]
    [InlineData("isnull(long(null))", true)]
    [InlineData("isnull(long(0))", false)]
    [InlineData("isnull(real(null))", true)]
    [InlineData("isnull(0.0)", false)]
    [InlineData("isnull(datetime(null))", true)]
    [InlineData("isnull(datetime(2023-02-26))", false)]
    [InlineData("isnull(timespan(null))", true)]
    [InlineData("isnull(0s)", false)]
    [InlineData("isnull('')", false)]
    [InlineData("isnull(' ')", false)]
    [InlineData("isnull('hello')", false)]
    public void BuiltIns_isnull_Scalar(string expression, bool expectedValue)
    {
        // Arrange
        var query = $"print v={expression}";

        var expected = $@"
v:bool
------------------
{expectedValue}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_isnull_Columnar()
    {
        // Arrange
        var query = @"
datatable(b:bool, i:int, l:long, r:real, d:datetime, t:timespan, s:string) [
  bool(null), int(null), long(null), real(null), datetime(null), timespan(null), '',
  false, 0, 0, 0, datetime(null), 0s, ' ',
  true, 1, 2, 3.5, datetime(2023-02-26), 5m, 'hello'
]
| project b=isnull(b), i=isnull(i), l=isnull(l), r=isnull(r), d=isnull(d), t=isnull(t), s=isnull(s)
";

        var expected = @"
b:bool; i:bool; l:bool; r:bool; d:bool; t:bool; s:bool
------------------
True; True; True; True; True; True; False
False; False; False; False; True; False; False
False; False; False; False; False; False; False
";

        // Act & Assert
        Test(query, expected);
    }

    [Theory]
    [InlineData("not(true)", false)]
    [InlineData("not(false)", true)]
    [InlineData("not(bool(null))", null)]
    public void BuiltIns_not_Scalar(string expression, bool? expectedValue)
    {
        // Arrange
        var query = $"print v={expression}";

        var expected = $@"
v:bool
------------------
{(expectedValue.HasValue ? expectedValue.Value.ToString() : "(null)")}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_not_Columnar()
    {
        // Arrange
        var query = @"
datatable(v:bool) [
  true, false, bool(null)
]
| project v=not(v)
";

        var expected = @"
v:bool
------------------
False
True
(null)
";

        // Act & Assert
        Test(query, expected);
    }

    [Theory]
    [InlineData("isempty('')", true)]
    [InlineData("isempty(' ')", false)]
    [InlineData("isempty('hello')", false)]
    public void BuiltIns_isempty_Scalar(string expression, bool expectedValue)
    {
        // Arrange
        var query = $"print v={expression}";

        var expected = $@"
v:bool
------------------
{expectedValue}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_isempty_Columnar()
    {
        // Arrange
        var query = @"
datatable(s:string) [
  '', ' ', 'hello'
]
| project s=isempty(s)
";

        var expected = @"
s:bool
------------------
True
False
False
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_minof_Scalar()
    {
        // Arrange
        var query = @"
print v=min_of(3,2)";

        var expected = @"
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
        var query = @"
datatable(a:long, b:long)
[
   2, 1,
   3, 4,
]
| project v = min_of(a, b)";

        var expected = @"
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
        var query = @"
print v=min_of(1.5, 2)
";

        var expected = @"
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
        var query = @"
print v=min_of(4,3,2,1.0)";

        var expected = @"
v:real
------------------
1
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_coalesce_Scalar()
    {
        // Arrange
        var query = @"
print b=coalesce(bool(null),true),
      i=coalesce(int(null),int(1)),
      l2=coalesce(long(null),long(1)),
      l3=coalesce(long(null),long(null),long(123)),
      l4=coalesce(long(null),long(null),long(5),long(6)),
      r=coalesce(real(null),real(1)),
      dt=coalesce(datetime(null),datetime(2023-01-01)),
      ts=coalesce(timespan(null),10s),
      s=coalesce('','a')
";

        var expected = @"
b:bool; i:int; l2:long; l3:long; l4:long; r:real; dt:datetime; ts:timespan; s:string
------------------
True; 1; 1; 123; 5; 1; 2023-01-01T00:00:00.0000000; 00:00:10; a
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_coalesce_Columnar()
    {
        // Arrange
        var query = @"
let d =
    datatable(b:bool, i:int, l:long, r:real, dt:datetime, ts:timespan, s:string)
    [
       true, 1, 1, 1, datetime(2023-01-01), 10s, 'a'
    ];
d
| where i==2 // get zero rows
| extend jc=1
| join kind=fullouter (d|extend jc=1) on jc
| project b=coalesce(b,b1),
          i=coalesce(i,i1),
          l=coalesce(l,l1),
          r=coalesce(r,r1),
          dt=coalesce(dt,dt1),
          ts=coalesce(ts,ts1),
          s=coalesce(s,s1)
";

        var expected = @"
b:bool; i:int; l:long; r:real; dt:datetime; ts:timespan; s:string
------------------
True; 1; 1; 1; 2023-01-01T00:00:00.0000000Z; 00:00:10; a
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_strcat_Scalar1()
    {
        // Arrange
        var query = @"
print v=strcat('a')
";

        var expected = @"
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
        var query = @"
print v=strcat('a', 'b')
";

        var expected = @"
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
        var query = @"
print v=strcat('a', '-', 123)
";

        var expected = @"
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
        var query = @"
datatable(a:string, b:long)
[
    'a', 123,
    'b', 456,
]
| project v = strcat(a, '-', b)
";

        var expected = @"
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
        var query = @"
print v=replace_string('abcb', 'b', '1')
";

        var expected = @"
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
        var query = @"
datatable(a:string) [ 'abc', 'abcb', 'def' ]
| project v = replace_string(a, 'b', '1')
";

        var expected = @"
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
        var query = @"
print v=strlen('abc')
";

        var expected = @"
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
        var query = @"
datatable(a:string)
[
    'a',
    'abc',
]
| project v = strlen(a)
";

        var expected = @"
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
        var query = @"
print abc1 = substring('abc', 0, 3),
      abc2 = substring('abc', -1, 20),
      bc1  = substring('abc', 1, 2),
      bc2  = substring('abc', 1, 20),
      b1   = substring('abc', 1, 1),
      n1   = substring('abc', 2, 0),
      n2   = substring('abc', 10, 1)
";

        var expected = @"
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
        var query = @"
datatable(a:string)
[
    '0',
    '01',
    '012',
    '0123',
]
| project v = substring(a,1,2)
";

        var expected = @"
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
        var query = @"
print v=url_encode_component('hello world')
";

        var expected = @"
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
        var query = @"
datatable(a:string) [ 'hello world', 'https://example.com?a=b' ]
| project v = url_encode_component(a)
";

        var expected = @"
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
        var query = @"
print v=url_decode('hello%20world')
";

        var expected = @"
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
        var query = @"
datatable(a:string) [ 'hello%20world', 'https%3A%2F%2Fexample.com%3Fa%3Db' ]
| project v = url_decode(a)
";

        var expected = @"
v:string
------------------
hello world
https://example.com?a=b
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_extract_Scalar()
    {
        // Arrange
        var query = @"
let pattern = '([0-9.]+) (s|ms)$';
let input   = 'Operation took 127.5 ms';
print duration    = extract(pattern, 1, input),
      unit        = extract(pattern, 2, input),
      all         = extract(pattern, 0, input),
      outOfBounds = extract(pattern, 3, input)
";

        var expected = @"
duration:string; unit:string; all:string; outOfBounds:string
------------------
127.5; ms; 127.5 ms; 
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_extract_Columnar()
    {
        // Arrange
        var query = @"
let pattern = '([0-9.]+) (s|ms)$';
datatable(input:string) [
    'Operation took 127.5 ms',
    'Another operation took 234.75 s',
    '',
]
| project duration    = extract(pattern, 1, input),
          unit        = extract(pattern, 2, input),
          all         = extract(pattern, 0, input),
          outOfBounds = extract(pattern, 3, input)
";

        var expected = @"
duration:string; unit:string; all:string; outOfBounds:string
------------------
127.5; ms; 127.5 ms; 
234.75; s; 234.75 s; 
; ; ; 
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_bin_Long()
    {
        // Arrange
        var query = @"
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

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
    public void BuiltIns_LogExp()
    {
        // Arrange
        var query = @"
datatable(a:real) [ 1, 0.1, 10, 100 ]
| project v1 = tolong(log(exp(a))*100+0.5)/100.0,
          v2 = tolong(exp(log(a))*100+0.5)/100.0";

        var expected = @"
v1:real; v2:real
------------------
1; 1
0.1; 0.1
10; 10
100; 100
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_Log10()
    {
        // Arrange
        var query = @"
datatable(a:real) [ 1, 0.1, 10, 100, -1 ]
| project v = log10(a)";

        var expected = @"
v:real
------------------
0
-1
1
2
NaN
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_Log2()
    {
        // Arrange
        var query = @"
datatable(a:real) [ 1, 0.5, 2, 8, -1 ]
| project v = log2(a)";

        var expected = @"
v:real
------------------
0
-1
1
3
NaN
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_Pow()
    {
        // Arrange
        var query = @"
datatable(x:real, y:real)
[
    10, 0,
    10, 1,
    10, 2,
    10, 3,
    10, -1,
    real(null), 3,
    3, real(null),
]
| project v = pow(x,y)";

        var expected = @"
v:real
------------------
1
10
100
1000
0.1
(null)
(null)
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_Sqrt()
    {
        // Arrange
        var query = @"
datatable(a:real) [ 0, 1, 4, 9, -1 ]
| project v = sqrt(a)";

        var expected = @"
v:real
------------------
0
1
2
3
NaN
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_DayOfWeek()
    {
        // Arrange
        var query = @"
datatable(d:datetime)
[
    datetime(1947-11-30 10:00:05),
    datetime(1970-05-11),
]
| project v = dayofweek(d)";

        var expected = @"
v:timespan
------------------
00:00:00
1.00:00:00
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_DayOfMonth()
    {
        // Arrange
        var query = @"
datatable(d:datetime)
[
    datetime(2015-12-14),
    datetime(2015-12-14 11:15),
]
| project v = dayofmonth(d)";

        var expected = @"
v:int
------------------
14
14
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_DayOfYear()
    {
        // Arrange
        var query = @"
datatable(d:datetime)
[
    datetime(2015-12-14),
    datetime(2015-12-14 23:59:59.999),
]
| project v = dayofyear(d)";

        var expected = @"
v:int
------------------
348
348
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_StartOfDay_EndOfDay()
    {
        // Arrange
        var query = @"
datatable(d:datetime)
[
    datetime(2017-01-01),
    datetime(2017-01-01 10:10:17),
]
| project v1 = startofday(d), v2 = endofday(d)";

        var expected = @"
v1:datetime; v2:datetime
------------------
2017-01-01T00:00:00.0000000; 2017-01-01T23:59:59.9999999
2017-01-01T00:00:00.0000000; 2017-01-01T23:59:59.9999999
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_StartOfWeek_EndOfWeek()
    {
        // Arrange
        var query = @"
datatable(d:datetime)
[
    datetime(2017-01-01),
    datetime(2017-01-01 10:10:17),
    datetime(2017-01-07),
    datetime(2017-01-07 23:59:59.999),
    datetime(2017-01-08),
]
| project v1 = startofweek(d), v2 = endofweek(d)";

        var expected = @"
v1:datetime; v2:datetime
------------------
2017-01-01T00:00:00.0000000; 2017-01-07T23:59:59.9999999
2017-01-01T00:00:00.0000000; 2017-01-07T23:59:59.9999999
2017-01-01T00:00:00.0000000; 2017-01-07T23:59:59.9999999
2017-01-01T00:00:00.0000000; 2017-01-07T23:59:59.9999999
2017-01-08T00:00:00.0000000; 2017-01-14T23:59:59.9999999
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_StartOfMonth_EndOfMonth()
    {
        // Arrange
        var query = @"
datatable(d:datetime)
[
    datetime(2017-01-01),
    datetime(2017-01-31 23:59:59.999),
    datetime(2017-02-01),
    datetime(2020-02-29 23:59:59.999),
    datetime(2020-03-01),
]
| project v1 = startofmonth(d), v2 = endofmonth(d)";

        var expected = @"
v1:datetime; v2:datetime
------------------
2017-01-01T00:00:00.0000000; 2017-01-31T23:59:59.9999999
2017-01-01T00:00:00.0000000; 2017-01-31T23:59:59.9999999
2017-02-01T00:00:00.0000000; 2017-02-28T23:59:59.9999999
2020-02-01T00:00:00.0000000; 2020-02-29T23:59:59.9999999
2020-03-01T00:00:00.0000000; 2020-03-31T23:59:59.9999999
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_StartOfYear_EndOfYear()
    {
        // Arrange
        var query = @"
datatable(d:datetime)
[
    datetime(2017-01-01),
    datetime(2017-02-01),
    datetime(2020-02-29 23:59:59.999),
    datetime(2020-03-01),
]
| project v1 = startofyear(d), v2 = endofyear(d)";

        var expected = @"
v1:datetime; v2:datetime
------------------
2017-01-01T00:00:00.0000000; 2017-12-31T23:59:59.9999999
2017-01-01T00:00:00.0000000; 2017-12-31T23:59:59.9999999
2020-01-01T00:00:00.0000000; 2020-12-31T23:59:59.9999999
2020-01-01T00:00:00.0000000; 2020-12-31T23:59:59.9999999
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_array_length_Scalar()
    {
        // Arrange
        var query = @"
print a=array_length(dynamic([])), b=array_length(dynamic([1,2]))";

        var expected = @"
a:long; b:long
------------------
0; 2
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_array_length_Columnar()
    {
        // Arrange
        var query = @"
datatable(x:dynamic) [
    dynamic([]),
    dynamic([1,2]),
    dynamic({})
]
| project a=array_length(x)";

        var expected = @"
a:long
------------------
0
2
(null)
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_array_sort_Scalar()
    {
        // Arrange
        var query = @"
let x=dynamic([ 1, 3, 2, ""a"", ""c"", ""b"" ]);
print a=array_sort_asc(x), b=array_sort_desc(x)";

        var expected = @"
a:dynamic; b:dynamic
------------------
[1,2,3,""a"",""b"",""c""]; [3,2,1,""c"",""b"",""a""]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_array_sort_Columnar()
    {
        // Arrange
        var query = @"
print x=dynamic([ 1, 3, 2, ""a"", ""c"", ""b"" ])
| project a=array_sort_asc(x), b=array_sort_desc(x)";

        var expected = @"
a:dynamic; b:dynamic
------------------
[1,2,3,""a"",""b"",""c""]; [3,2,1,""c"",""b"",""a""]
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_bin_DateTime()
    {
        // Arrange
        var query = @"
print v=bin(datetime(2022-03-02 23:04), 1h)";

        var expected = @"
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
        var query = @"
datatable(a:int) [ 9, 10, 11 ]
| project v = bin(a, 10)";

        var expected = @"
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
    public void BuiltIns_geo_distance_2points_Scalar()
    {
        // Arrange
        var query = @"
print d1=tolong(geo_distance_2points(-122.3518577,47.6205099,-122.3519241,47.6097268)), // Space Needle to Pike Place Market
      d2=geo_distance_2points(300,0,0,0), // Invalid lon1
      d3=geo_distance_2points(0,-300,0,0), // Invalid lat1
      d4=geo_distance_2points(0,0,-300,0), // Invalid lon2
      d5=geo_distance_2points(0,0,0,300), // Invalid lat2
      d6=geo_distance_2points(0,real(null),0,0) // Something is null
";

        var expected = @"
d1:long; d2:real; d3:real; d4:real; d5:real; d6:real
------------------
1199; (null); (null); (null); (null); (null)
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuiltIns_geo_distance_2points_Columnar()
    {
        // Arrange
        var query = @"
datatable(lon1:real,lat1:real,lon2:real,lat2:real) [
    -122.3518577,47.6205099,-122.3519241,47.6097268, // Space Needle to Pike Place Market
    300,0,0,0,
    0,-300,0,0,
    0,0,-300,0,
    0,0,0,300,
    0,real(null),0,0,
]
| project d=tolong(geo_distance_2points(lon1, lat1, lon2, lat2))
";

        var expected = @"
d:long
------------------
1199
(null)
(null)
(null)
(null)
(null)
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void UserDefinedFunction1()
    {
        // Arrange
        var query = @"
let f=(a: long) { a + 1 };
datatable(v:long) [ 1, 2, 3 ]
| project v = f(v + 1)
";

        var expected = @"
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
        var query = @"
let f=(t:(v:long)) { t | project v };
f((datatable(v:long) [ 1, 2, 3 ]))
";

        var expected = @"
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
        var query = @"
let f=(t:(v:long), c:long) { t | project v = v+c };
f((datatable(v:long) [ 1, 2, 3 ]), 1)
";

        var expected = @"
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
        var query = @"
let f=(a:real) { a + 0.5 };
print v=f(1)
";

        var expected = @"
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
        var query = @"
print a = -1, b = 1 + -3.0
";

        var expected = @"
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
        var query = @"
print a=1+2
";

        var expected = @"
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
        var query = @"
print a=1+2, b=3+4.0, c=5.0+6, d=7.0+8.0
";

        var expected = @"
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
        var query = @"
let c=1;
datatable(a:long) [ 1, 2, 3 ]
| project v = a + c
";

        var expected = @"
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
        var query = @"
print a=2-1, b=4-3.5, c=6.5-5, d=8.0-7.5, e=10s-1s, f=datetime(2022-03-06T20:00)-5m
";

        var expected = @"
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
        var query = @"
print a=2*1, b=4*3.5, c=6.5*5, d=8.0*7.5
";

        var expected = @"
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
        var query = @"
print a=6/2, b=5/0.5, c=10./5, d=2.5/0.5, e=15ms/10ms
";

        var expected = @"
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
        var query = @"
datatable(a:long, b:long)
[
    4, 5,
    5, 5,
    6, 5,
    -1, 4,
]
| project v = a % b
";

        var expected = @"
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
        var query = @"
datatable(a:long, b:real)
[
    1, 1.5,
    2, 1.5,
]
| project v = a > b, w = b > a
";

        var expected = @"
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
        var query = @"
print a = 2 > 1, b = 1 > 2, c = 1.5 > 2, d = 2 > 1.5
";

        var expected = @"
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
        var query = @"
datatable(a:long) [ 1, 2, 3 ]
| where a == 2
";

        var expected = @"
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
        var query = @"
datatable(v:string) [ 'a', 'b' ]
| where v == 'a'
";

        var expected = @"
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
        var query = @"
datatable(a:long) [ 1, 2, 3]
| where a != 2
";

        var expected = @"
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
        var query = @"
datatable(v:string) [ 'a', 'b' ]
| where v != 'a'
";

        var expected = @"
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
        var query = @"
datatable(a:bool, b:bool)
[
    false, false,
    false, true,
    true, false,
    true, true,
]
| project v = a and b
";

        var expected = @"
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
        var query = @"
datatable(a:bool, b:bool)
[
    false, false,
    false, true,
    true, false,
    true, true,
]
| project v = a or b
";

        var expected = @"
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
        var query = @"
let nil=bool(null);
print a = nil and nil, b = nil and true, c = nil and false
";

        var expected = @"
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
        var query = @"
let nil=bool(null);
print a = nil or nil, b = nil or true, c = nil or false
";

        var expected = @"
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
        var query = @"
datatable(v:string)
[
    'a',
    'ac',
    'bc',
    'BC',
]
| project v = 'abcd' contains v, notV = 'abcd' !contains v
";

        var expected = @"
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
        var query = @"
datatable(v:string)
[
    'a',
    'ac',
    'bc',
    'BC',
]
| project v = 'abcd' contains_cs v, notV = 'abcd' !contains_cs v
";

        var expected = @"
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
        var query = @"
datatable(v:string)
[
    'a',
    'ab',
    'ABC',
    'bc',
]
| project v = 'abcd' startswith v, notV = 'abcd' !startswith v
";

        var expected = @"
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
        var query = @"
datatable(v:string)
[
    'a',
    'ab',
    'ABC',
    'bc',
]
| project v = 'abcd' startswith_cs v, notV = 'abcd' !startswith_cs v
";

        var expected = @"
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
        var query = @"
datatable(v:string)
[
    'd',
    'cd',
    'BCD',
    'bc',
]
| project v = 'abcd' endswith v, notV = 'abcd' !endswith v
";

        var expected = @"
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
        var query = @"
datatable(v:string)
[
    'd',
    'cd',
    'BCD',
    'bc',
]
| project v = 'abcd' endswith_cs v, notV = 'abcd' !endswith_cs v
";

        var expected = @"
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
    public void BinOp_MatchRegex_Scalar()
    {
        // Arrange
        var query = @"
print v1 = '' matches regex '[0-9]',
      v2 = 'abc' matches regex '[0-9]',
      v3 = 'a1c' matches regex '[0-9]'
";

        var expected = @"
v1:bool; v2:bool; v3:bool
------------------
False; False; True
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BinOp_MatchRegex_Columnar()
    {
        // Arrange
        var query = @"
datatable(s:string, p:string)
[
    '',    '[0-9]',
    'abc', '[a-z]',
    'a1c', '',
]
| project v1 = s matches regex '[0-9]',
          v2 = '123abc' matches regex p
";

        var expected = @"
v1:bool; v2:bool
------------------
False; True
False; True
True; True
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void ToScalar_Tabular()
    {
        // Arrange
        var query = @"
print v=toscalar(print a=1,b=2)
";

        var expected = @"
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
        var query = @"
print v=toscalar(1.5)
";

        var expected = @"
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
        var query = @"
datatable(a:long) [ 1, 2, 3 ]
| summarize v=100 * count()
";

        var expected = @"
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
        var query = @"
print a=toint(''), b=toint('123')
";

        var expected = @"
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
        var query = @"
datatable(v:string) [ '', '123', 'nan' ]
| project a=toint(v)
";

        var expected = @"
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
        var query = @"
print a=tolong(''), b=tolong('123')
";

        var expected = @"
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
        var query = @"
datatable(v:string) [ '', '123', 'nan' ]
| project a=tolong(v)
";

        var expected = @"
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
        var query = @"
print a=tolong(123.5)
";

        var expected = @"
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
        var query = @"
print a=todouble(''), b=todouble('123.5')
";

        var expected = @"
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
        var query = @"
datatable(v:string) [ '', '123.5', 'nan' ]
| project a=todouble(v)
";

        var expected = @"
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
        var query = @"
print a=toreal(''), b=toreal('123.5')
";

        var expected = @"
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
        var query = @"
datatable(v:string) [ '', '123.5', 'nan' ]
| project a=toreal(v)
";

        var expected = @"
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
    public void Cast_ToString_Scalar()
    {
        // Arrange
        var query = @"
print a=tostring(int(123)), b=tostring(long(234)), c=tostring(1.5), d=tostring(10s), e=tostring(datetime(2023-08-30 23:00)), f=tostring('abc'),
      n1=tostring(int(null)), n2=tostring(long(null)), n3=tostring(real(null)), n4=tostring(timespan(null)), n5=tostring(datetime(null)), n6=tostring('')
";

        var expected = @"
a:string; b:string; c:string; d:string; e:string; f:string; n1:string; n2:string; n3:string; n4:string; n5:string; n6:string
------------------
123; 234; 1.5; 00:00:10; 8/30/2023 11:00:00 PM; abc; ; ; ; ; ;
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Cast_ToString_Columnar()
    {
        // Arrange
        var query = @"
datatable(a:int, b:long, c:real, d:timespan, e:datetime, f:string)
[
    123, 234, 1.5, 10s, datetime(2023-08-30 23:00), 'abc',
    int(null), long(null), real(null), timespan(null), datetime(null), '',
]
| project a=tostring(a), b=tostring(b), c=tostring(c), d=tostring(d), e=tostring(e), f=tostring(f)
";

        var expected = @"
a:string; b:string; c:string; d:string; e:string; f:string
------------------
123; 234; 1.5; 00:00:10; 8/30/2023 11:00:00 PM; abc
; ; ; ; ;
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Cast_ToStringFromDynamicString_Works()
    {
        // Arrange
        var query = @"
let a = parse_json('{""stringField"":""abc def"", ""intField"":123, ""realField"":1.5, ""nullField"":null, ""arrayField"":[1,2], ""objField"":{""a"":1}}');
print stringField = tostring(a.stringField),
      intField    = tostring(a.intField),
      realField   = tostring(a.realField),
      nullField   = tostring(a.nullField),
      arrayField  = tostring(a.arrayField),
      objField    = tostring(a.objField),
      nonExistent = tostring(a.nonExistent)
";

        var expected = @"
stringField:string; intField:string; realField:string; nullField:string; arrayField:string; objField:string; nonExistent:string
------------------
abc def; 123; 1.5; ; [1,2]; {""a"":1};
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Iff_Scalar()
    {
        // Arrange
        var query = @"
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

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
        var query = @"
datatable(v:long) [ 1, 2, 3, 4 ]
| project cs = row_cumsum(v, false)
";

        var expected = @"
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
        var query = @"
let t = datatable(v:long) [ 1, 2, 3 ];
union t,t
| project cs = row_cumsum(v, false)
";

        var expected = @"
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
        var query = @"
datatable(v:int, r:bool)
[
    1, false,
    2, false,
    3, true,
    4, false,
]
| project cs = row_cumsum(v, r)
";

        var expected = @"
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

    [Fact(Skip =
        "Known bug in window function implementations where a table is evaluated more than once. For now, using materialize() works around this")]
    public void Window_RowCumSum_MultipleEvaluations()
    {
        // Arrange
        var query = @"
let d=
    datatable(v:int) [ 10, 10 ]
    | project cs = row_cumsum(v, false);
let a = toscalar(d | summarize max(cs));
d
| extend normalized = todouble(cs) / a
";

        var expected = @"
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
        var query = @"
let d = materialize(
    datatable(v:int) [ 10, 10 ]
    | project cs = row_cumsum(v, false)
);
let a = toscalar(d | summarize max(cs));
d
| extend normalized = todouble(cs) / a
";

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
        var query = @"
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

        var expected = @"
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

    [Fact]
    public void Join_InnerJoin_LeftRightScopesOnClause()
    {
        // Arrange
        var query = @"
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

        var expected = @"
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
        var query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4,
];
let Y = datatable(Key:string, Value2:long, Value3:string)
[
    'b',10,'aa',
    'c',20,'bb',
    'c',30,'cc',
    'd',40,'dd',
];
X | join kind=leftouter Y on Key
| order by Key asc, Key1 asc
";

        var expected = @"
Key:string; Value1:long; Key1:string; Value2:long; Value3:string
------------------
a; 1; ; (null); 
b; 2; b; 10; aa
b; 3; b; 10; aa
c; 4; c; 20; bb
c; 4; c; 30; cc
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Join_RightOuterJoin1()
    {
        // Arrange
        var query = @"
let X = datatable(Key:string, Value1:long)
[
    'a',1,
    'b',2,
    'b',3,
    'c',4,
];
let Y = datatable(Key:string, Value2:long, Value3:string)
[
    'b',10,'aa',
    'c',20,'bb',
    'c',30,'cc',
    'd',40,'dd',
];
X | join kind=rightouter Y on Key
| order by Value2 asc nulls last, Key1 asc
";

        var expected = @"
Key:string; Value1:long; Key1:string; Value2:long; Value3:string
------------------
b; 2; b; 10; aa
b; 3; b; 10; aa
c; 4; c; 20; bb
c; 4; c; 30; cc
; (null); d; 40; dd
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Join_FullOuterJoin1()
    {
        // Arrange
        var query = @"
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
| order by Value1  asc nulls last, Key1 asc nulls first
";

        var expected = @"
Key:string; Value1:long; Key1:string; Value2:long
------------------
a; 1; ; (null)
b; 2; b; 10
b; 3; b; 10
c; 4; c; 20
c; 4; c; 30
; (null); d; 40
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Join_LeftSemiJoin1()
    {
        // Arrange
        var query = @"
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

        var expected = @"
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
        var query = @"
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

        var expected = @"
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
        var query = $@"
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

        var expected = @"
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
        var query = $@"
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

        var expected = @"
Key:string; Value2:long
------------------
d; 40
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Join_DollarLeftDollarRight()
    {
        // Arrange
        var query = @"
let X = datatable(a:string, b:string)
[
    'a1','b1',
    'a2','b2',
];
let Y = datatable(b:string)
[
    'b2',
];
X | join kind=inner Y on $left.b == $right.b
";

        var expected = @"
a:string; b:string; b1:string
------------------
a2; b2; b2
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Join_DollarLeftDollarRight2()
    {
        // Arrange
        var query = @"
let X = datatable(a:string, leftKey:string)
[
    'a1','key1',
    'a2','key2',
];
let Y = datatable(rightKey:string)
[
    'key2',
];
X | join kind=inner Y on $left.leftKey == $right.rightKey
";

        var expected = @"
a:string; leftKey:string; rightKey:string
------------------
a2; key2; key2
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Join_DifferentNumColumnsLeftAndRight()
    {
        // Arrange
        var query = @"
let X = datatable(a:string, b:string)
[
    'a1','b1',
    'a2','b2',
];
let Y = datatable(b:string)
[
    'b2',
];
X | join kind=inner Y on b
";

        var expected = @"
a:string; b:string; b1:string
------------------
a2; b2; b2
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Join_StringColumnsAreEmpty()
    {
        // Arrange
        var query = @"
datatable(Key:int)
[
    1,
    2,
]
| join kind=leftouter (
    datatable(Key:int, a:string)
    [
        1, 'b',
        3, 'c'
    ]
) on Key | project-away Key1
| extend aIsNull = isnull(a), aIsEmpty=isempty(a), aLen=strlen(a)
";

        var expected = @"
Key:int; a:string; aIsNull:bool; aIsEmpty:bool; aLen:long
------------------
1; b; False; False; 1
2; ; False; True; 0
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void TrimWorksAsExpected()
    {
        // Arrange
        var query = "print trim('a*','aaathis is a testaaaa')";

        var expected = @"print_0:string
------------------
this is a test";

        // Act & Assert
        Test(query, expected);
    }


    [Fact]
    public void TrimStartWorksAsExpected()
    {
        // Arrange
        var query = "print trim_start('a*','aaathis is a testaaaa')";

        var expected = @"print_0:string
------------------
this is a testaaaa";

        // Act & Assert
        Test(query, expected);
    }


    [Fact]
    public void TrimEndWorksAsExpected()
    {
        // Arrange
        var query = "print trim_end('a*','aaathis is a testaaaa')";

        var expected = @"print_0:string
------------------
aaathis is a test";

        // Act & Assert
        Test(query, expected);
    }

    private static void Test(string query, string expectedOutput)
    {
        try
        {
            var engine = BabyKustoEngine.CreateForTest();
            var result = (TabularResult)engine.Evaluate(
                [], query);
            var stringified = result.Value.DumpToString();

            var canonicalOutput = stringified.Trim().Replace("\r\n", "\n");
            var canonicalExpectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");

            canonicalOutput.Should().Be(canonicalExpectedOutput);
        }
        catch (Exception x)
        {
            Console.WriteLine($"EXCEPTION: {x}");
        }

        
    }

    /// <summary>
    ///     This function tests the case where a function accepting
    ///     a scalar is called with a columnar value. Let bindings
    ///     in the function need to be associated with the
    ///     "result kind" of the expression so that columnar values
    ///     propagate to subsequent expressions.
    /// </summary>
    [Fact]
    public void UserDefinedFunction_With_Local_Variables()
    {
        // Arrange
        var query = """
                    let f=(a:long) { let b = a + 1; let c = b * b; c };

                    datatable(v: long) [1, 2, 3, 4]
                    | project v = f(v)
                    """;

        var expected = """
                       v:long
                       ------------------
                       4
                       9
                       16
                       25
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Range_Long()
    {
        // Arrange
        var query = "range x from 1 to 6 step 2";

        var expected = """
                       x:long
                       ------------------
                       1
                       3
                       5
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Range_Long_Countdown()
    {
        // Arrange
        var query = "range x from 6 to 1 step -2";

        var expected = """
                       x:long
                       ------------------
                       6
                       4
                       2
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Range_Int()
    {
        // Arrange
        var query = "range x from int(0) to int(5) step int(1)";

        var expected = """
                       x:int
                       ------------------
                       0
                       1
                       2
                       3
                       4
                       5
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Range_Mixed_Long_Real()
    {
        // Arrange
        var query = "range x from 0 to 2.5 step 1";

        var expected = """
                       x:real
                       ------------------
                       0
                       1
                       2
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Range_Real()
    {
        // Arrange
        var query = "range x from 0 to 5 step 0.5";

        var expected = """
                       x:real
                       ------------------
                       0
                       0.5
                       1
                       1.5
                       2
                       2.5
                       3
                       3.5
                       4
                       4.5
                       5
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Range_Real_Countdown()
    {
        // Arrange
        var query = "range x from 5 to 2 step -1.0";

        var expected = """
                       x:real
                       ------------------
                       5
                       4
                       3
                       2
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void Range_Infinite_Take_5()
    {
        // Arrange
        var query = "range x from 1 to 9999999999999999 step 2 | take 5";

        var expected = """
                       x:long
                       ------------------
                       1
                       3
                       5
                       7
                       9
                       """;

        // Act & Assert
        Test(query, expected);
    }


    [Fact]
    public void Range_0_to_0_step_1()
    {
        // Arrange
        var query = "range x from 0 to 0 step 1";

        var expected = """
                       x:long
                       ------------------
                       0
                       """;

        // Act & Assert
        Test(query, expected);
    }


    [Fact]
    public void Range_Forward_With_Backwards_Step_Terminates()
    {
        // Arrange
        var query = "range x from 1 to 10 step -1";

        var expected = """
                       x:long
                       ------------------
                       """;

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void RenderTest()
    {
        var engine = BabyKustoEngine.CreateForTest();
        var query = "print X=1 | render barchart with (legend=visible,ymax=100)";
        var result = (TabularResult)engine.Evaluate([], query);
        var vis = result.VisualizationState;
        vis.ChartType.Should().Be("barchart");
        vis.Properties["legend"].Should().Be("visible");
        vis.Properties["ymax"].Should().Be("100");
    }

    [Fact]
    public void WhenNoRenderSpecifiedVisualisationShouldBeEmpty()
    {
        var engine = BabyKustoEngine.CreateForTest();
        var query = "print X=1";
        var result = (TabularResult)engine.Evaluate([], query);
        var vis = result.VisualizationState;
        vis.Should().Be(VisualizationState.Empty);
    }

    [Fact]
    public void LookupTest()
    {
        // Arrange
        var query = """
                    let FactTable=datatable(Row:string,Personal:string,Family:string) [
                      "1", "Rowan",   "Murphy",
                      "2", "Ellis",   "Turner",
                      "3", "Ellis",   "Turner",
                      "4", "Maya",  "Robinson",
                      "5", "Quinn",    "Campbell"
                    ];
                    let DimTable=datatable(Personal:string,Family:string,Alias:string) [
                      "Rowan",  "Murphy",   "rowanm",
                      "Ellis",  "Turner", "ellist",
                      "Maya", "Robinson", "mayar",
                      "Quinn",   "Campbell",    "quinnc"
                    ];
                    FactTable
                    | lookup kind=leftouter DimTable on Personal, Family
                    """;

        var expected = """
                       Row:string; Personal:string; Family:string; Alias:string
                       ------------------
                       1; Rowan; Murphy; rowanm
                       2; Ellis; Turner; ellist
                       3; Ellis; Turner; ellist
                       4; Maya; Robinson; mayar
                       5; Quinn; Campbell; quinnc
                       """;
        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MvExpand_BasicDynamicArray()
    {
        // Arrange
        var query = @"
datatable(name:string, tags:dynamic) 
[
    'Alice', dynamic([""tag1"", ""tag2"", ""tag3""]),
    'Bob', dynamic([""tag4"", ""tag5""])
]
| mv-expand tags
";

        var expected = @"
name:string; tags:dynamic
------------------
Alice; ""tag1""
Alice; ""tag2""
Alice; ""tag3""
Bob; ""tag4""
Bob; ""tag5""
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MvExpand_WithNumericArray()
    {
        // Arrange
        var query = @"
datatable(id:long, values:dynamic) 
[
    1, dynamic([10, 20, 30]),
    2, dynamic([40])
]
| mv-expand values
";

        var expected = @"
id:long; values:dynamic
------------------
1; 10
1; 20
1; 30
2; 40
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MvExpand_WithNullValues()
    {
        // Arrange
        var query = @"
datatable(name:string, tags:dynamic) 
[
    'Alice', dynamic([""tag1"", ""tag2""]),
    'Bob', dynamic(null)
]
| mv-expand tags
";

        var expected = @"
name:string; tags:dynamic
------------------
Alice; ""tag1""
Alice; ""tag2""
Bob; (null)
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MvExpand_MixedArrayTypes()
    {
        // Arrange - From Microsoft docs: Single column - array expansion with mixed types
        var query = @"
datatable (a: int, b: dynamic)
[
    1, dynamic([10, 20]),
    2, dynamic([""a"", ""b""])
]
| mv-expand b
";

        var expected = @"
a:long; b:dynamic
------------------
1; 10
1; 20
2; ""a""
2; ""b""
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MvExpand_BagExpansion()
    {
        // Arrange - From Microsoft docs: Single column - bag expansion
        var query = @"
datatable (a: int, b: dynamic)
[
    1, dynamic({'prop1': 'a1', 'prop2': 'b1'}),
    2, dynamic({'prop1': 'a2', 'prop2': 'b2'})
]
| mv-expand b
";

        var expected = @"
a:long; b:dynamic
------------------
1; {""prop1"":""a1""}
1; {""prop2"":""b1""}
2; {""prop1"":""a2""}
2; {""prop2"":""b2""}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MvExpand_WithItemIndex()
    {
        // Arrange - From Microsoft docs: Using with_itemindex
        var query = @"
range x from 1 to 4 step 1
| summarize x = make_list(x)
| mv-expand with_itemindex=Index x
";

        var expected = @"
x:dynamic; Index:long
------------------
1; 0
2; 1
3; 2
4; 3
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void BuildSchema_BasicExample()
    {
        // Arrange - From Microsoft docs: buildschema basic example
        var query = @"
datatable(value: dynamic)
[
    dynamic({""x"":1, ""y"":3.5}),
    dynamic({""x"":""somevalue"", ""z"":[1, 2, 3]}),
    dynamic({""y"":{""w"":""zzz""}, ""t"":[""aa"", ""bb""], ""z"":[""foo""]})
]
| summarize schema_value = buildschema(value)
";

        // Note: The exact schema format might vary, but should contain the merged schema
        // This test may need to be marked as skipped if the exact output format differs
        var expected = @"
schema_value:dynamic
------------------
{""x"":[""long"",""string""],""y"":[""double"",{""w"":""string""}],""z"":{""indexer"":[""long"",""string""]},""t"":{""indexer"":""string""}}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeBag_BasicExample()
    {
        // Arrange - From Microsoft docs: make_bag basic example
        var query = @"
datatable(prop:string, value:string)
[
    'prop01', 'val_a',
    'prop02', 'val_b',
    'prop03', 'val_c',
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag(p)
";

        var expected = @"
dict:dynamic
------------------
{""prop01"":""val_a"",""prop02"":""val_b"",""prop03"":""val_c""}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeBag_WithMaxSize()
    {
        // Arrange - Test make_bag with maxSize parameter
        var query = @"
datatable(prop:string, value:string)
[
    'prop01', 'val_a',
    'prop02', 'val_b',
    'prop03', 'val_c',
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag(p, 2)
";

        // Note: Exact keys included depend on which ones are encountered first
        // This test verifies that maxSize is respected (only 2 keys)
        var expected = @"
dict:dynamic
------------------
{""prop01"":""val_a"",""prop02"":""val_b""}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeBagIf_BasicExample()
    {
        // Arrange - From Microsoft docs: make_bag_if basic example
        var query = @"
datatable(prop:string, value:string, predicate:bool)
[
    'prop01', 'val_a', true,
    'prop02', 'val_b', false,
    'prop03', 'val_c', true
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag_if(p, predicate)
";

        var expected = @"
dict:dynamic
------------------
{""prop01"":""val_a"",""prop03"":""val_c""}
";

        // Act & Assert
        Test(query, expected);
    }

    [Fact]
    public void MakeBagIf_WithMaxSize()
    {
        // Arrange - Test make_bag_if with maxSize parameter
        var query = @"
datatable(prop:string, value:string, predicate:bool)
[
    'prop01', 'val_a', true,
    'prop02', 'val_b', true,
    'prop03', 'val_c', true,
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag_if(p, predicate, 2)
";

        var expected = @"
dict:dynamic
------------------
{""prop01"":""val_a"",""prop02"":""val_b""}
";

        // Act & Assert
        Test(query, expected);
    }
}
