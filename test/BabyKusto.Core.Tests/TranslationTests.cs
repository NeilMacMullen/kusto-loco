// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace KustoExecutionEngine.Core.Tests;
#if false
    public class TranslationTests
    {
        [Theory]
        [InlineData("MyTable")]
        [InlineData("MyTable | project AppMachine")]
        [InlineData("MyTable | project-keep A*")]
        [InlineData("MyTable | project-away C*")]
        [InlineData("MyTable | project-rename vm=AppMachine")]
        [InlineData("MyTable | project-reorder CounterName")]
        [InlineData("MyTable | summarize count()")]
        [InlineData("MyTable | summarize by AppMachine")]
        [InlineData("MyTable | summarize by a=strcat(AppMachine,'_')")]
        [InlineData("let a=1; MyTable | project b=CounterValue+a")]
        [InlineData("MyTable | extend d=1")]
        [InlineData("MyTable | extend AppMachine='a'")]
        [InlineData("MyTable | extend AppMachine='a', CounterValue=2, j=AppMachine")]
        [InlineData("MyTable | join MyTable on AppMachine")]
        [InlineData("MyTable | join kind=inner (MyTable) on AppMachine")]
        [InlineData("MyTable | join kind=leftouter (MyTable) on AppMachine")]
        [InlineData("MyTable | join kind=rightouter (MyTable) on AppMachine")]
        [InlineData("MyTable | join kind=fullouter (MyTable) on AppMachine")]
        [InlineData("MyTable | join kind=leftanti (MyTable) on AppMachine")]
        [InlineData("MyTable | join kind=rightanti (MyTable) on AppMachine")]
        public void Translation_Works(string query)
        {
            // Arrange
            var engine = new BabyKustoEngine();
            engine.AddGlobalTable("MyTable", GetSampleData());

            // Act
            engine.Evaluate(query);
        }

        private static ITableSource GetSampleData()
        {
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
    }
#endif