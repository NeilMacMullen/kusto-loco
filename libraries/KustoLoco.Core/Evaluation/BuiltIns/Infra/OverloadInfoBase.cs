//
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns;

public abstract class OverloadInfoBase
{
    protected OverloadInfoBase(TypeSymbol returnType, params TypeSymbol[] parameterTypes)
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
        NumParametersToMatch= int.MaxValue;
    }
    protected OverloadInfoBase(TypeSymbol returnType, int numParametersToMatch,params TypeSymbol[] parameterTypes)
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
        NumParametersToMatch = numParametersToMatch;
    }

    protected OverloadInfoBase(TypeSymbol returnType,  bool loopParams, params TypeSymbol[] parameterTypes)
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
        NumParametersToMatch = int.MaxValue;
        RepeatParams=loopParams;
    }
    public TypeSymbol ReturnType { get; }
    public IReadOnlyList<TypeSymbol> ParameterTypes { get; }
    public int NumParametersToMatch
    {
        get;
    }

    public bool RepeatParams { get; }

}
