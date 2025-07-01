using AppInsightsSupport;
using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class AdxCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var blocks = econtext.Sequence;
        var ai = new AdxLoader(exp.Settings, exp._outputConsole);

       
        if (blocks.Complete)
            return;

        var query = blocks.Next();
        //make sure we pick up any variable interpolation in case we are inside a function
        query = exp.Interpolate(query);

        var connection = o.ConnectionString;
        var database = o.Database;
        if (database.IsBlank())
        {
            var i = connection.IndexOf('@');
            if (i >= 0 && i < (connection.Length - 1))
            {
                database = connection[..i];
                connection = connection[(i + 1)..];
            }
        }

        exp.Info("Running ADX query.  This may take a while....");
        var result = await ai.LoadTable(connection,database, query);
        await exp.InjectResult(result);
    }

    [Verb("adx", 
        HelpText =
            """
            Runs a query against adx
            The connection-string is the full connection string to the ADX cluster, which can
            be found in the Azure portal.

            The database is the name of the database.

            The database and connection-string can be combined into a single string
            in the format database@connection-string, 
            e.g. mydb@https://myadx.cluster.kusto.windows.net


            Examples:
             .set cluster https://help.kusto.windows.net/ 
             .set database SampleLogs
             .set path $database@$cluster
             
             .adx $path
             RawSysLogs
             | extend Cpu=tostring(tags.cpu_id)
             | summarize N=count() by Day = bin(timestamp,1d),Cpu
             | render linechart
             "
            """)]
    internal class Options
    {
        [Value(0, HelpText = "connection-string", Required = true)]
        public string ConnectionString { get; set; } = string.Empty;

        [Value(1, HelpText = "database ",Required = false)]
        public string Database { get; set; } = "";
        
    }
}
