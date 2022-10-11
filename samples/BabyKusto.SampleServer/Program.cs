using BabyKusto.Core;
using BabyKusto.Core.Util;
using BabyKusto.Server;
using BabyKusto.Server.Service;
using Kusto.Language.Symbols;

namespace BabyKusto.SampleServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddSingleton<ITablesProvider, SimpleTableProvider>();
            builder.Services.AddBabyKustoServer();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();
            app.UseRouting();

            app.MapControllers();

            app.Run();
        }

        private class SimpleTableProvider : ITablesProvider
        {
            private readonly List<ITableSource> _tables;

            public SimpleTableProvider()
            {
                var colBuilder = ColumnHelpers.CreateBuilder(ScalarTypes.String);
                colBuilder.Add("val1");
                colBuilder.Add("val2");

                _tables = new List<ITableSource>
                {
                    new InMemoryTableSource(
                        new TableSymbol("MyTable", new ColumnSymbol("Col1", ScalarTypes.String)),
                        new[] { colBuilder.ToColumn() }),
                };
            }

            public List<ITableSource> GetTables()
            {
                return _tables;
            }
        }
    }
}