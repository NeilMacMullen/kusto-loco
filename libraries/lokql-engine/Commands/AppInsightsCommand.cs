using AppInsightsSupport;
using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Issues a query against an application insights server
/// </summary>
public static class AppInsightsCommand
{
    public const string SchemaCsv = """
                                    availabilityResults,timestamp
                                    availabilityResults,id
                                    availabilityResults,name
                                    availabilityResults,location
                                    availabilityResults,success
                                    availabilityResults,message
                                    availabilityResults,size
                                    availabilityResults,duration
                                    availabilityResults,performanceBucket
                                    availabilityResults,itemType
                                    availabilityResults,customDimensions
                                    availabilityResults,customMeasurements
                                    availabilityResults,operation_Name
                                    availabilityResults,operation_Id
                                    availabilityResults,operation_ParentId
                                    availabilityResults,operation_SyntheticSource
                                    availabilityResults,session_Id
                                    availabilityResults,user_Id
                                    availabilityResults,user_AuthenticatedId
                                    availabilityResults,user_AccountId
                                    availabilityResults,application_Version
                                    availabilityResults,client_Type
                                    availabilityResults,client_Model
                                    availabilityResults,client_OS
                                    availabilityResults,client_IP
                                    availabilityResults,client_City
                                    availabilityResults,client_StateOrProvince
                                    availabilityResults,client_CountryOrRegion
                                    availabilityResults,client_Browser
                                    availabilityResults,cloud_RoleName
                                    availabilityResults,cloud_RoleInstance
                                    availabilityResults,appId
                                    availabilityResults,appName
                                    availabilityResults,iKey
                                    availabilityResults,sdkVersion
                                    availabilityResults,itemId
                                    availabilityResults,itemCount
                                    availabilityResults,_ResourceId
                                    browserTimings,timestamp
                                    browserTimings,name
                                    browserTimings,url
                                    browserTimings,networkDuration
                                    browserTimings,sendDuration
                                    browserTimings,receiveDuration
                                    browserTimings,processingDuration
                                    browserTimings,totalDuration
                                    browserTimings,performanceBucket
                                    browserTimings,customDimensions
                                    browserTimings,customMeasurements
                                    browserTimings,operation_Name
                                    browserTimings,operation_Id
                                    browserTimings,operation_ParentId
                                    browserTimings,operation_SyntheticSource
                                    browserTimings,session_Id
                                    browserTimings,user_Id
                                    browserTimings,user_AuthenticatedId
                                    browserTimings,user_AccountId
                                    browserTimings,application_Version
                                    browserTimings,client_Type
                                    browserTimings,client_Model
                                    browserTimings,client_OS
                                    browserTimings,client_IP
                                    browserTimings,client_City
                                    browserTimings,client_StateOrProvince
                                    browserTimings,client_CountryOrRegion
                                    browserTimings,client_Browser
                                    browserTimings,cloud_RoleName
                                    browserTimings,cloud_RoleInstance
                                    browserTimings,appId
                                    browserTimings,appName
                                    browserTimings,iKey
                                    browserTimings,sdkVersion
                                    browserTimings,itemId
                                    browserTimings,itemType
                                    browserTimings,itemCount
                                    browserTimings,_ResourceId
                                    customEvents,timestamp
                                    customEvents,name
                                    customEvents,itemType
                                    customEvents,customDimensions
                                    customEvents,customMeasurements
                                    customEvents,operation_Name
                                    customEvents,operation_Id
                                    customEvents,operation_ParentId
                                    customEvents,operation_SyntheticSource
                                    customEvents,session_Id
                                    customEvents,user_Id
                                    customEvents,user_AuthenticatedId
                                    customEvents,user_AccountId
                                    customEvents,application_Version
                                    customEvents,client_Type
                                    customEvents,client_Model
                                    customEvents,client_OS
                                    customEvents,client_IP
                                    customEvents,client_City
                                    customEvents,client_StateOrProvince
                                    customEvents,client_CountryOrRegion
                                    customEvents,client_Browser
                                    customEvents,cloud_RoleName
                                    customEvents,cloud_RoleInstance
                                    customEvents,appId
                                    customEvents,appName
                                    customEvents,iKey
                                    customEvents,sdkVersion
                                    customEvents,itemId
                                    customEvents,itemCount
                                    customEvents,_ResourceId
                                    customMetrics,timestamp
                                    customMetrics,name
                                    customMetrics,value
                                    customMetrics,valueCount
                                    customMetrics,valueSum
                                    customMetrics,valueMin
                                    customMetrics,valueMax
                                    customMetrics,valueStdDev
                                    customMetrics,customDimensions
                                    customMetrics,operation_Name
                                    customMetrics,operation_Id
                                    customMetrics,operation_ParentId
                                    customMetrics,operation_SyntheticSource
                                    customMetrics,session_Id
                                    customMetrics,user_Id
                                    customMetrics,user_AuthenticatedId
                                    customMetrics,user_AccountId
                                    customMetrics,application_Version
                                    customMetrics,client_Type
                                    customMetrics,client_Model
                                    customMetrics,client_OS
                                    customMetrics,client_IP
                                    customMetrics,client_City
                                    customMetrics,client_StateOrProvince
                                    customMetrics,client_CountryOrRegion
                                    customMetrics,client_Browser
                                    customMetrics,cloud_RoleName
                                    customMetrics,cloud_RoleInstance
                                    customMetrics,appId
                                    customMetrics,appName
                                    customMetrics,iKey
                                    customMetrics,sdkVersion
                                    customMetrics,itemId
                                    customMetrics,itemType
                                    customMetrics,_ResourceId
                                    dependencies,timestamp
                                    dependencies,id
                                    dependencies,target
                                    dependencies,type
                                    dependencies,name
                                    dependencies,data
                                    dependencies,success
                                    dependencies,resultCode
                                    dependencies,duration
                                    dependencies,performanceBucket
                                    dependencies,itemType
                                    dependencies,customDimensions
                                    dependencies,customMeasurements
                                    dependencies,operation_Name
                                    dependencies,operation_Id
                                    dependencies,operation_ParentId
                                    dependencies,operation_SyntheticSource
                                    dependencies,session_Id
                                    dependencies,user_Id
                                    dependencies,user_AuthenticatedId
                                    dependencies,user_AccountId
                                    dependencies,application_Version
                                    dependencies,client_Type
                                    dependencies,client_Model
                                    dependencies,client_OS
                                    dependencies,client_IP
                                    dependencies,client_City
                                    dependencies,client_StateOrProvince
                                    dependencies,client_CountryOrRegion
                                    dependencies,client_Browser
                                    dependencies,cloud_RoleName
                                    dependencies,cloud_RoleInstance
                                    dependencies,appId
                                    dependencies,appName
                                    dependencies,iKey
                                    dependencies,sdkVersion
                                    dependencies,itemId
                                    dependencies,itemCount
                                    dependencies,_ResourceId
                                    exceptions,timestamp
                                    exceptions,problemId
                                    exceptions,handledAt
                                    exceptions,type
                                    exceptions,message
                                    exceptions,assembly
                                    exceptions,method
                                    exceptions,outerType
                                    exceptions,outerMessage
                                    exceptions,outerAssembly
                                    exceptions,outerMethod
                                    exceptions,innermostType
                                    exceptions,innermostMessage
                                    exceptions,innermostAssembly
                                    exceptions,innermostMethod
                                    exceptions,severityLevel
                                    exceptions,details
                                    exceptions,itemType
                                    exceptions,customDimensions
                                    exceptions,customMeasurements
                                    exceptions,operation_Name
                                    exceptions,operation_Id
                                    exceptions,operation_ParentId
                                    exceptions,operation_SyntheticSource
                                    exceptions,session_Id
                                    exceptions,user_Id
                                    exceptions,user_AuthenticatedId
                                    exceptions,user_AccountId
                                    exceptions,application_Version
                                    exceptions,client_Type
                                    exceptions,client_Model
                                    exceptions,client_OS
                                    exceptions,client_IP
                                    exceptions,client_City
                                    exceptions,client_StateOrProvince
                                    exceptions,client_CountryOrRegion
                                    exceptions,client_Browser
                                    exceptions,cloud_RoleName
                                    exceptions,cloud_RoleInstance
                                    exceptions,appId
                                    exceptions,appName
                                    exceptions,iKey
                                    exceptions,sdkVersion
                                    exceptions,itemId
                                    exceptions,itemCount
                                    exceptions,_ResourceId
                                    pageViews,timestamp
                                    pageViews,id
                                    pageViews,name
                                    pageViews,url
                                    pageViews,duration
                                    pageViews,performanceBucket
                                    pageViews,itemType
                                    pageViews,customDimensions
                                    pageViews,customMeasurements
                                    pageViews,operation_Name
                                    pageViews,operation_Id
                                    pageViews,operation_ParentId
                                    pageViews,operation_SyntheticSource
                                    pageViews,session_Id
                                    pageViews,user_Id
                                    pageViews,user_AuthenticatedId
                                    pageViews,user_AccountId
                                    pageViews,application_Version
                                    pageViews,client_Type
                                    pageViews,client_Model
                                    pageViews,client_OS
                                    pageViews,client_IP
                                    pageViews,client_City
                                    pageViews,client_StateOrProvince
                                    pageViews,client_CountryOrRegion
                                    pageViews,client_Browser
                                    pageViews,cloud_RoleName
                                    pageViews,cloud_RoleInstance
                                    pageViews,appId
                                    pageViews,appName
                                    pageViews,iKey
                                    pageViews,sdkVersion
                                    pageViews,itemId
                                    pageViews,itemCount
                                    pageViews,_ResourceId
                                    performanceCounters,timestamp
                                    performanceCounters,name
                                    performanceCounters,category
                                    performanceCounters,counter
                                    performanceCounters,instance
                                    performanceCounters,value
                                    performanceCounters,customDimensions
                                    performanceCounters,operation_Name
                                    performanceCounters,operation_Id
                                    performanceCounters,operation_ParentId
                                    performanceCounters,operation_SyntheticSource
                                    performanceCounters,session_Id
                                    performanceCounters,user_Id
                                    performanceCounters,user_AuthenticatedId
                                    performanceCounters,user_AccountId
                                    performanceCounters,application_Version
                                    performanceCounters,client_Type
                                    performanceCounters,client_Model
                                    performanceCounters,client_OS
                                    performanceCounters,client_IP
                                    performanceCounters,client_City
                                    performanceCounters,client_StateOrProvince
                                    performanceCounters,client_CountryOrRegion
                                    performanceCounters,client_Browser
                                    performanceCounters,cloud_RoleName
                                    performanceCounters,cloud_RoleInstance
                                    performanceCounters,appId
                                    performanceCounters,appName
                                    performanceCounters,iKey
                                    performanceCounters,sdkVersion
                                    performanceCounters,itemId
                                    performanceCounters,itemType
                                    performanceCounters,_ResourceId
                                    traces,timestamp
                                    traces,message
                                    traces,severityLevel
                                    traces,itemType
                                    traces,customDimensions
                                    traces,customMeasurements
                                    traces,operation_Name
                                    traces,operation_Id
                                    traces,operation_ParentId
                                    traces,operation_SyntheticSource
                                    traces,session_Id
                                    traces,user_Id
                                    traces,user_AuthenticatedId
                                    traces,user_AccountId
                                    traces,application_Version
                                    traces,client_Type
                                    traces,client_Model
                                    traces,client_OS
                                    traces,client_IP
                                    traces,client_City
                                    traces,client_StateOrProvince
                                    traces,client_CountryOrRegion
                                    traces,client_Browser
                                    traces,cloud_RoleName
                                    traces,cloud_RoleInstance
                                    traces,appId
                                    traces,appName
                                    traces,iKey
                                    traces,sdkVersion
                                    traces,itemId
                                    traces,itemCount
                                    traces,_ResourceId
                                    requests,timestamp
                                    requests,id
                                    requests,source
                                    requests,name
                                    requests,url
                                    requests,success
                                    requests,resultCode
                                    requests,duration
                                    requests,performanceBucket
                                    requests,itemType
                                    requests,customDimensions
                                    requests,customMeasurements
                                    requests,operation_Name
                                    requests,operation_Id
                                    requests,operation_ParentId
                                    requests,operation_SyntheticSource
                                    requests,session_Id
                                    requests,user_Id
                                    requests,user_AuthenticatedId
                                    requests,user_AccountId
                                    requests,application_Version
                                    requests,client_Type
                                    requests,client_Model
                                    requests,client_OS
                                    requests,client_IP
                                    requests,client_City
                                    requests,client_StateOrProvince
                                    requests,client_CountryOrRegion
                                    requests,client_Browser
                                    requests,cloud_RoleName
                                    requests,cloud_RoleInstance
                                    requests,appId
                                    requests,appName
                                    requests,iKey
                                    requests,sdkVersion
                                    requests,itemId
                                    requests,itemCount
                                    requests,_ResourceId
                                    """;

    internal static async Task RunAsync(ICommandContext econtext, Options o)
    {
        var console = econtext.Console;
        var blocks = econtext.InputProcessor;
        var settings = econtext.Settings;
        var ai = new ApplicationInsightsLogLoader(settings, console);

        var timespan = o.Timespan;
        if (!TimeRangeProcessor.ParseTime(timespan, out var t))
        {
            console.Warn($"Unable to parse timespan '{timespan}'");
            return;
        }

        if (blocks.IsComplete)
            return;

        var query = blocks.ConsumeNextBlock();
        var rid = o.Rid;
        var tenant = o.TenantId;
        var i = rid.IndexOf(':');
        if (i >= 0 && i < (rid.Length-1))
        {
            tenant = rid[..i];
            rid = rid[(i + 1)..];
        }

        console.Info("Running application insights query.  This may take a while....");
        var result = await ai.LoadTable(tenant, rid, query, t, DateTime.UtcNow);
        await econtext.InjectResult(result);
    }

    [Verb("appinsights", aliases: ["ai"],
        HelpText = """
                   Runs a query against application insights
                   The resourceId is the full resourceId of the application insights instance which can be obtained
                   from the JSON View of the Insights resource in the Azure portal.

                   The timespan is a duration string which can be in the format of '7d', '30d', '1h' etc.
                   If not specified, the default is 24 hours.

                   The TenantId is the tenant-id of the subscription that contains the application insights instance.
                   This is optional but may be required if you have multiple tenants or if the application insights
                   instance is in a different tenant than the one you are currently logged into.
                   The tenant id can be specified as a prefix to the resourceId separated with a colon.

                   Examples:
                   .set appservice /subscriptions/12a.... 
                   .appinsights $appservice 7d
                   traces | where message contains 'error'

                   .appinsights $appservice 30d
                   exceptions
                   | summarize count() by outerMessage
                   | render piechart

                   .set tenantid "ae...."
                   .set svc  "$tenantid:$appservice" 
                   .appinsights $svc 1d
                   traces | order by timestamp | take 100

                   """)]
    internal class Options
    {
        [Value(0, HelpText = "resourceId", Required = true)]
        public string Rid { get; set; } = string.Empty;

        [Value(1, HelpText = "timespan ")] public string Timespan { get; set; } = "1d";


        [Option(HelpText = "The tenant-id of the subscription that contains the application insights instance")]
        public string TenantId { get; set; } = string.Empty;
    }

    public static void RegisterSchema(ICommandProcessor processor)
    {
       processor.RegisterSchema("appinsights",SchemaCsv);
    }
}
