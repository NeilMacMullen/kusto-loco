using System;
using Kusto.Language.Syntax;

namespace BabyKusto.Core.Statements
{
    internal abstract class BabyKustoStatement<T> : BabyKustoStatement
        where T : Statement
    {
        private readonly T _statement;

        protected BabyKustoStatement(BabyKustoEngine engine, T statement)
            : base(engine)
        {
            _statement = statement;
        }
    }

    internal abstract class BabyKustoStatement
    {
        protected readonly BabyKustoEngine _engine;
        protected bool _initialized = true;

        internal BabyKustoStatement(BabyKustoEngine engine)
        {
            _engine = engine;
        }

        public object? Execute()
        {
            if (!_initialized)
            {
                Initialize();
                _initialized = true;
            }

            return ExecuteInternal();
        }

        protected virtual void Initialize()
        {
        }

        protected abstract object? ExecuteInternal();

        protected internal static BabyKustoStatement Build(BabyKustoEngine engine, Statement statement)
        {
            return statement.Kind switch
            {
                SyntaxKind.LetStatement => new BabyKustoLetStatement(engine, (LetStatement)statement),
                SyntaxKind.ExpressionStatement => new BabyKustoExpressionStatement(engine, (ExpressionStatement)statement),
                _ => throw new InvalidOperationException($"Unsupported statement type '{statement.Kind}'."),
            };
        }
    }
}
