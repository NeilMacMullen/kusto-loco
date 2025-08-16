using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.PluginSupport;

namespace FizzBuzzPlugin;

public class FizzBuzzFunctionPlugin : IKqlFunction
{
    public string GetNameAndVersion() => "FizzBuzz v1";

    public void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> registration)
        => FizzFunction.Register(registration);
}
