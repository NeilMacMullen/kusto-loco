// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal abstract class OverloadInfoBase
    {
        protected OverloadInfoBase(TypeSymbol returnType, params TypeSymbol[] parameterTypes)
        {
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }

        public TypeSymbol ReturnType { get; }
        public IReadOnlyList<TypeSymbol> ParameterTypes { get; }
    }
}