using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public class InContext
{
    public string LastA = string.Empty;
    public JsonNode LastNode = new JsonObject();
    public bool LastResult;
    public string[] LastStringArray = [];
}
