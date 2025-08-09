using System.Text.Json;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ParseJson")]
public partial class ParseJsonFunction
{
    JsonNode? StringImpl(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        try
        {
            return JsonNode.Parse(input);
        }
        catch (JsonException)
        {
            // From official docs at https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/parsejsonfunction:
            // If json is of type string, but it isn't a properly formatted JSON string, then the returned value is an object of type dynamic that holds the original string value.
            return JsonValue.Create(input);
        }
    }

    JsonNode? JsonImpl(JsonNode input)
    {
        //     From docs at <see href="https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/parsejsonfunction" />:
        //     If json is of type dynamic, its value is used as-is.
        //        //TODO - this is effectively an identity function and the source-generator could hoist this into
        //returning a columnar result of the input column - eg
        // var column = (GenericTypedBaseColumnOfJsonNode)arguments[0].Column;
        // return new ColumnarResult(column);
        return input;
    }

}
