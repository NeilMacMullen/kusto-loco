﻿using AppInsightsSupport;
using CommandLine;

namespace Lokql.Engine.Commands;

/// <summary>
///     Issues a query against an application insights server
/// </summary>
public static class AppInsightsCommand
{
    public const string SchemaCsv = @"Command,Table,Column
.appinsights,availabilityResults,timestamp
.appinsights,availabilityResults,id
.appinsights,availabilityResults,name
.appinsights,availabilityResults,location
.appinsights,availabilityResults,success
.appinsights,availabilityResults,message
.appinsights,availabilityResults,size
.appinsights,availabilityResults,duration
.appinsights,availabilityResults,performanceBucket
.appinsights,availabilityResults,itemType
.appinsights,availabilityResults,customDimensions
.appinsights,availabilityResults,customMeasurements
.appinsights,availabilityResults,operation_Name
.appinsights,availabilityResults,operation_Id
.appinsights,availabilityResults,operation_ParentId
.appinsights,availabilityResults,operation_SyntheticSource
.appinsights,availabilityResults,session_Id
.appinsights,availabilityResults,user_Id
.appinsights,availabilityResults,user_AuthenticatedId
.appinsights,availabilityResults,user_AccountId
.appinsights,availabilityResults,application_Version
.appinsights,availabilityResults,client_Type
.appinsights,availabilityResults,client_Model
.appinsights,availabilityResults,client_OS
.appinsights,availabilityResults,client_IP
.appinsights,availabilityResults,client_City
.appinsights,availabilityResults,client_StateOrProvince
.appinsights,availabilityResults,client_CountryOrRegion
.appinsights,availabilityResults,client_Browser
.appinsights,availabilityResults,cloud_RoleName
.appinsights,availabilityResults,cloud_RoleInstance
.appinsights,availabilityResults,appId
.appinsights,availabilityResults,appName
.appinsights,availabilityResults,iKey
.appinsights,availabilityResults,sdkVersion
.appinsights,availabilityResults,itemId
.appinsights,availabilityResults,itemCount
.appinsights,availabilityResults,_ResourceId
.appinsights,browserTimings,timestamp
.appinsights,browserTimings,name
.appinsights,browserTimings,url
.appinsights,browserTimings,networkDuration
.appinsights,browserTimings,sendDuration
.appinsights,browserTimings,receiveDuration
.appinsights,browserTimings,processingDuration
.appinsights,browserTimings,totalDuration
.appinsights,browserTimings,performanceBucket
.appinsights,browserTimings,customDimensions
.appinsights,browserTimings,customMeasurements
.appinsights,browserTimings,operation_Name
.appinsights,browserTimings,operation_Id
.appinsights,browserTimings,operation_ParentId
.appinsights,browserTimings,operation_SyntheticSource
.appinsights,browserTimings,session_Id
.appinsights,browserTimings,user_Id
.appinsights,browserTimings,user_AuthenticatedId
.appinsights,browserTimings,user_AccountId
.appinsights,browserTimings,application_Version
.appinsights,browserTimings,client_Type
.appinsights,browserTimings,client_Model
.appinsights,browserTimings,client_OS
.appinsights,browserTimings,client_IP
.appinsights,browserTimings,client_City
.appinsights,browserTimings,client_StateOrProvince
.appinsights,browserTimings,client_CountryOrRegion
.appinsights,browserTimings,client_Browser
.appinsights,browserTimings,cloud_RoleName
.appinsights,browserTimings,cloud_RoleInstance
.appinsights,browserTimings,appId
.appinsights,browserTimings,appName
.appinsights,browserTimings,iKey
.appinsights,browserTimings,sdkVersion
.appinsights,browserTimings,itemId
.appinsights,browserTimings,itemType
.appinsights,browserTimings,itemCount
.appinsights,browserTimings,_ResourceId
.appinsights,customEvents,timestamp
.appinsights,customEvents,name
.appinsights,customEvents,itemType
.appinsights,customEvents,customDimensions
.appinsights,customEvents,customMeasurements
.appinsights,customEvents,operation_Name
.appinsights,customEvents,operation_Id
.appinsights,customEvents,operation_ParentId
.appinsights,customEvents,operation_SyntheticSource
.appinsights,customEvents,session_Id
.appinsights,customEvents,user_Id
.appinsights,customEvents,user_AuthenticatedId
.appinsights,customEvents,user_AccountId
.appinsights,customEvents,application_Version
.appinsights,customEvents,client_Type
.appinsights,customEvents,client_Model
.appinsights,customEvents,client_OS
.appinsights,customEvents,client_IP
.appinsights,customEvents,client_City
.appinsights,customEvents,client_StateOrProvince
.appinsights,customEvents,client_CountryOrRegion
.appinsights,customEvents,client_Browser
.appinsights,customEvents,cloud_RoleName
.appinsights,customEvents,cloud_RoleInstance
.appinsights,customEvents,appId
.appinsights,customEvents,appName
.appinsights,customEvents,iKey
.appinsights,customEvents,sdkVersion
.appinsights,customEvents,itemId
.appinsights,customEvents,itemCount
.appinsights,customEvents,_ResourceId
.appinsights,customMetrics,timestamp
.appinsights,customMetrics,name
.appinsights,customMetrics,value
.appinsights,customMetrics,valueCount
.appinsights,customMetrics,valueSum
.appinsights,customMetrics,valueMin
.appinsights,customMetrics,valueMax
.appinsights,customMetrics,valueStdDev
.appinsights,customMetrics,customDimensions
.appinsights,customMetrics,operation_Name
.appinsights,customMetrics,operation_Id
.appinsights,customMetrics,operation_ParentId
.appinsights,customMetrics,operation_SyntheticSource
.appinsights,customMetrics,session_Id
.appinsights,customMetrics,user_Id
.appinsights,customMetrics,user_AuthenticatedId
.appinsights,customMetrics,user_AccountId
.appinsights,customMetrics,application_Version
.appinsights,customMetrics,client_Type
.appinsights,customMetrics,client_Model
.appinsights,customMetrics,client_OS
.appinsights,customMetrics,client_IP
.appinsights,customMetrics,client_City
.appinsights,customMetrics,client_StateOrProvince
.appinsights,customMetrics,client_CountryOrRegion
.appinsights,customMetrics,client_Browser
.appinsights,customMetrics,cloud_RoleName
.appinsights,customMetrics,cloud_RoleInstance
.appinsights,customMetrics,appId
.appinsights,customMetrics,appName
.appinsights,customMetrics,iKey
.appinsights,customMetrics,sdkVersion
.appinsights,customMetrics,itemId
.appinsights,customMetrics,itemType
.appinsights,customMetrics,_ResourceId
.appinsights,dependencies,timestamp
.appinsights,dependencies,id
.appinsights,dependencies,target
.appinsights,dependencies,type
.appinsights,dependencies,name
.appinsights,dependencies,data
.appinsights,dependencies,success
.appinsights,dependencies,resultCode
.appinsights,dependencies,duration
.appinsights,dependencies,performanceBucket
.appinsights,dependencies,itemType
.appinsights,dependencies,customDimensions
.appinsights,dependencies,customMeasurements
.appinsights,dependencies,operation_Name
.appinsights,dependencies,operation_Id
.appinsights,dependencies,operation_ParentId
.appinsights,dependencies,operation_SyntheticSource
.appinsights,dependencies,session_Id
.appinsights,dependencies,user_Id
.appinsights,dependencies,user_AuthenticatedId
.appinsights,dependencies,user_AccountId
.appinsights,dependencies,application_Version
.appinsights,dependencies,client_Type
.appinsights,dependencies,client_Model
.appinsights,dependencies,client_OS
.appinsights,dependencies,client_IP
.appinsights,dependencies,client_City
.appinsights,dependencies,client_StateOrProvince
.appinsights,dependencies,client_CountryOrRegion
.appinsights,dependencies,client_Browser
.appinsights,dependencies,cloud_RoleName
.appinsights,dependencies,cloud_RoleInstance
.appinsights,dependencies,appId
.appinsights,dependencies,appName
.appinsights,dependencies,iKey
.appinsights,dependencies,sdkVersion
.appinsights,dependencies,itemId
.appinsights,dependencies,itemCount
.appinsights,dependencies,_ResourceId
.appinsights,exceptions,timestamp
.appinsights,exceptions,problemId
.appinsights,exceptions,handledAt
.appinsights,exceptions,type
.appinsights,exceptions,message
.appinsights,exceptions,assembly
.appinsights,exceptions,method
.appinsights,exceptions,outerType
.appinsights,exceptions,outerMessage
.appinsights,exceptions,outerAssembly
.appinsights,exceptions,outerMethod
.appinsights,exceptions,innermostType
.appinsights,exceptions,innermostMessage
.appinsights,exceptions,innermostAssembly
.appinsights,exceptions,innermostMethod
.appinsights,exceptions,severityLevel
.appinsights,exceptions,details
.appinsights,exceptions,itemType
.appinsights,exceptions,customDimensions
.appinsights,exceptions,customMeasurements
.appinsights,exceptions,operation_Name
.appinsights,exceptions,operation_Id
.appinsights,exceptions,operation_ParentId
.appinsights,exceptions,operation_SyntheticSource
.appinsights,exceptions,session_Id
.appinsights,exceptions,user_Id
.appinsights,exceptions,user_AuthenticatedId
.appinsights,exceptions,user_AccountId
.appinsights,exceptions,application_Version
.appinsights,exceptions,client_Type
.appinsights,exceptions,client_Model
.appinsights,exceptions,client_OS
.appinsights,exceptions,client_IP
.appinsights,exceptions,client_City
.appinsights,exceptions,client_StateOrProvince
.appinsights,exceptions,client_CountryOrRegion
.appinsights,exceptions,client_Browser
.appinsights,exceptions,cloud_RoleName
.appinsights,exceptions,cloud_RoleInstance
.appinsights,exceptions,appId
.appinsights,exceptions,appName
.appinsights,exceptions,iKey
.appinsights,exceptions,sdkVersion
.appinsights,exceptions,itemId
.appinsights,exceptions,itemCount
.appinsights,exceptions,_ResourceId
.appinsights,pageViews,timestamp
.appinsights,pageViews,id
.appinsights,pageViews,name
.appinsights,pageViews,url
.appinsights,pageViews,duration
.appinsights,pageViews,performanceBucket
.appinsights,pageViews,itemType
.appinsights,pageViews,customDimensions
.appinsights,pageViews,customMeasurements
.appinsights,pageViews,operation_Name
.appinsights,pageViews,operation_Id
.appinsights,pageViews,operation_ParentId
.appinsights,pageViews,operation_SyntheticSource
.appinsights,pageViews,session_Id
.appinsights,pageViews,user_Id
.appinsights,pageViews,user_AuthenticatedId
.appinsights,pageViews,user_AccountId
.appinsights,pageViews,application_Version
.appinsights,pageViews,client_Type
.appinsights,pageViews,client_Model
.appinsights,pageViews,client_OS
.appinsights,pageViews,client_IP
.appinsights,pageViews,client_City
.appinsights,pageViews,client_StateOrProvince
.appinsights,pageViews,client_CountryOrRegion
.appinsights,pageViews,client_Browser
.appinsights,pageViews,cloud_RoleName
.appinsights,pageViews,cloud_RoleInstance
.appinsights,pageViews,appId
.appinsights,pageViews,appName
.appinsights,pageViews,iKey
.appinsights,pageViews,sdkVersion
.appinsights,pageViews,itemId
.appinsights,pageViews,itemCount
.appinsights,pageViews,_ResourceId
.appinsights,performanceCounters,timestamp
.appinsights,performanceCounters,name
.appinsights,performanceCounters,category
.appinsights,performanceCounters,counter
.appinsights,performanceCounters,instance
.appinsights,performanceCounters,value
.appinsights,performanceCounters,customDimensions
.appinsights,performanceCounters,operation_Name
.appinsights,performanceCounters,operation_Id
.appinsights,performanceCounters,operation_ParentId
.appinsights,performanceCounters,operation_SyntheticSource
.appinsights,performanceCounters,session_Id
.appinsights,performanceCounters,user_Id
.appinsights,performanceCounters,user_AuthenticatedId
.appinsights,performanceCounters,user_AccountId
.appinsights,performanceCounters,application_Version
.appinsights,performanceCounters,client_Type
.appinsights,performanceCounters,client_Model
.appinsights,performanceCounters,client_OS
.appinsights,performanceCounters,client_IP
.appinsights,performanceCounters,client_City
.appinsights,performanceCounters,client_StateOrProvince
.appinsights,performanceCounters,client_CountryOrRegion
.appinsights,performanceCounters,client_Browser
.appinsights,performanceCounters,cloud_RoleName
.appinsights,performanceCounters,cloud_RoleInstance
.appinsights,performanceCounters,appId
.appinsights,performanceCounters,appName
.appinsights,performanceCounters,iKey
.appinsights,performanceCounters,sdkVersion
.appinsights,performanceCounters,itemId
.appinsights,performanceCounters,itemType
.appinsights,performanceCounters,_ResourceId
.appinsights,traces,timestamp
.appinsights,traces,message
.appinsights,traces,severityLevel
.appinsights,traces,itemType
.appinsights,traces,customDimensions
.appinsights,traces,customMeasurements
.appinsights,traces,operation_Name
.appinsights,traces,operation_Id
.appinsights,traces,operation_ParentId
.appinsights,traces,operation_SyntheticSource
.appinsights,traces,session_Id
.appinsights,traces,user_Id
.appinsights,traces,user_AuthenticatedId
.appinsights,traces,user_AccountId
.appinsights,traces,application_Version
.appinsights,traces,client_Type
.appinsights,traces,client_Model
.appinsights,traces,client_OS
.appinsights,traces,client_IP
.appinsights,traces,client_City
.appinsights,traces,client_StateOrProvince
.appinsights,traces,client_CountryOrRegion
.appinsights,traces,client_Browser
.appinsights,traces,cloud_RoleName
.appinsights,traces,cloud_RoleInstance
.appinsights,traces,appId
.appinsights,traces,appName
.appinsights,traces,iKey
.appinsights,traces,sdkVersion
.appinsights,traces,itemId
.appinsights,traces,itemCount
.appinsights,traces,_ResourceId
.appinsights,requests,timestamp
.appinsights,requests,id
.appinsights,requests,source
.appinsights,requests,name
.appinsights,requests,url
.appinsights,requests,success
.appinsights,requests,resultCode
.appinsights,requests,duration
.appinsights,requests,performanceBucket
.appinsights,requests,itemType
.appinsights,requests,customDimensions
.appinsights,requests,customMeasurements
.appinsights,requests,operation_Name
.appinsights,requests,operation_Id
.appinsights,requests,operation_ParentId
.appinsights,requests,operation_SyntheticSource
.appinsights,requests,session_Id
.appinsights,requests,user_Id
.appinsights,requests,user_AuthenticatedId
.appinsights,requests,user_AccountId
.appinsights,requests,application_Version
.appinsights,requests,client_Type
.appinsights,requests,client_Model
.appinsights,requests,client_OS
.appinsights,requests,client_IP
.appinsights,requests,client_City
.appinsights,requests,client_StateOrProvince
.appinsights,requests,client_CountryOrRegion
.appinsights,requests,client_Browser
.appinsights,requests,cloud_RoleName
.appinsights,requests,cloud_RoleInstance
.appinsights,requests,appId
.appinsights,requests,appName
.appinsights,requests,iKey
.appinsights,requests,sdkVersion
.appinsights,requests,itemId
.appinsights,requests,itemCount
.appinsights,requests,_ResourceId
";

    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var blocks = econtext.Sequence;
        var ai = new ApplicationInsightsLogLoader(exp.Settings, exp._outputConsole);

        var timespan = o.Timespan;
        if (!TimeRangeProcessor.ParseTime(timespan, out var t))
        {
            exp.Warn($"Unable to parse timespan '{timespan}'");
            return;
        }

        if (blocks.Complete)
            return;

        var query = blocks.Next();
        //make sure we pick up any variable interpolation in case we are inside a function
        query = exp._interpolator.Interpolate(query);
        exp.Info("Running application insights query.  This may take a while....");
        var result = await ai.LoadTable(o.TenantId,o.Rid, query, t, DateTime.UtcNow);
        await exp.InjectResult(result);
    }

    [Verb("appinsights", aliases: ["ai"],
        HelpText = @"Runs a query against application insights
The resourceId is the full resourceId of the application insights instance which can be obtained
from the JSON View of the Insights resource in the Azure portal.
The timespan is a duration string which can be in the format of '7d', '30d', '1h' etc.
If not specified, the default is 24 hours.

Examples:
 .set appservice /subscriptions/12a.... 
 .appinsights $appservice 7d
 traces | where message contains 'error'

 .appinsights $appservice 30d
 exceptions
 | summarize count() by outerMessage
 | render piechart
")]
    internal class Options
    {
        [Value(0, HelpText = "resourceId", Required = true)]
        public string Rid { get; set; } = string.Empty;

        [Value(1, HelpText = "timespan ")] public string Timespan { get; set; } = "1d";


        [Option(HelpText = "The tenant-id of the subscription that contains the application insights instance")]
        public string TenantId { get; set; } = string.Empty;

    }
}
