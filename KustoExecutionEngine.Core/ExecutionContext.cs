namespace KustoExecutionEngine.Core
{
    internal class ExecutionContext
    {
        private readonly ExecutionContext? _outer;
        private readonly Dictionary<string, object?> _bindings;

        public ExecutionContext(ExecutionContext? outer, params KeyValuePair<string, object?>[] bindings)
        {
            _outer = outer;
            _bindings = new Dictionary<string, object?>(bindings);
        }

        public bool TryGetBinding(string name, out object? value)
        {
            if (_bindings.TryGetValue(name, out value))
            {
                return true;
            }

            return _outer is null ? false : _outer.TryGetBinding(name, out value);
        }
    }
}