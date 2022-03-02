// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns
{
    internal sealed class ScalarOverloadInfo : OverloadInfoBase
    {
        public ScalarOverloadInfo(IScalarFunctionImpl impl, TypeSymbol returnType, params TypeSymbol[] parameterTypes)
           : base(returnType, parameterTypes)
        {
            ScalarImpl = impl;
        }

        public IScalarFunctionImpl ScalarImpl { get; }
    }
}
