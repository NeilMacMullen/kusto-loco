// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation
{
    internal class LocalScope
    {
        private readonly LocalScope? _outer;
        private readonly Dictionary<string, (Symbol Symbol, EvaluationResult? Value)> _locals = new();

        public LocalScope(LocalScope? outer = null)
        {
            _outer = outer;
        }

        public void AddSymbol(Symbol symbol, EvaluationResult? value)
        {
            if (Lookup(symbol.Name) != null)
            {
                throw new InvalidOperationException($"A symbol with name {symbol.Name} is already defined.");
            }

            _locals.Add(symbol.Name, (symbol, value));
        }

        public (Symbol Symbol, EvaluationResult? Value)? Lookup(string name)
        {
            if (_locals.TryGetValue(name, out var symbol))
            {
                return symbol;
            }

            return _outer?.Lookup(name);
        }
    }
}
