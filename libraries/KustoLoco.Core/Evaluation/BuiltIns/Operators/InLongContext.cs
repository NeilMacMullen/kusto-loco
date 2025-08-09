using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public class InLongContext
{
    public long LastA;
    public long?[] LastLongArray = [];
    public JsonNode LastNode = new JsonObject();
    public bool LastResult;
}
