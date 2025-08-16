using Kusto.Language.Symbols;
using KustoLoco.Core.Evaluation.BuiltIns;

namespace KustoLoco.PluginSupport;

public interface IKqlFunction : ILokqlPlugin
{
    public void Register(Dictionary<FunctionSymbol, ScalarFunctionInfo> registration);
}
