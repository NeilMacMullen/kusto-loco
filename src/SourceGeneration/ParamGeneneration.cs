using System.Linq;

namespace SourceGeneration
{
    public static class ParamGeneneration
    {
        private const string RowIndex = "rowIndex";

        private const string RowCount = "rowCount";
        public static string Val(Param p) => p.IsString ? $"{p.Name}" : $"{p.Name}.Value";

        public static string ColumnName(Param p)
            => $"{p.Name}Column";

        public static string VariableName(Param p)
            => $"{p.Name}";

        public static string GetNullableType(Param p)
            => p.Type.Replace("?", "") + "?";

        public static string MakeTypedColumn(Param p) =>
            $"var {ColumnName(p)} = (TypedBaseColumn<{GetNullableType(p)}>) arguments[{p.Index}].Column;";

        public static string MakeTypedVariable(Param p) =>
            $"var {VariableName(p)} = ({GetNullableType(p)}) arguments[{p.Index}].Value;";


        public static string BuildScalarMethod(CodeAcccumulator dbg, ImplementationMethod method)
        {
            var parameters = method.Arguments;
            var ret = method.ReturnType;
            dbg.AppendLine("public ScalarResult InvokeScalar(ScalarResult[] arguments)");
            dbg.EnterCodeBlock();
            dbg.AppendStatement($"Debug.Assert(arguments.Length=={parameters.Length})");
            AddTypedVariables(dbg, parameters);
            dbg.AppendStatement($"{GetNullableType(ret)} data=null");
            dbg.AppendLine("for(var i=0;i < 1;i++)");
            dbg.EnterCodeBlock();
            PerformNullChecks(dbg, parameters, false);


            var pvals = string.Join(",", parameters.Select(Val));

            dbg.AppendStatement($"data = {method.Name}({pvals})");
            dbg.ExitCodeBlock();

            dbg.AppendStatement($"var returnType =TypeMapping.SymbolForType(typeof({ret.Type}))");
            dbg.AppendStatement("return new ScalarResult(returnType,data)");
            dbg.ExitCodeBlock();
            return dbg.ToString();
        }

        public static string BuildColumnarMethod(CodeAcccumulator dbg, ImplementationMethod method)
        {
            var parameters = method.Arguments;
            var ret = method.ReturnType;
            dbg.AppendLine("public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)");
            dbg.EnterCodeBlock();
            dbg.AppendStatement($"Debug.Assert(arguments.Length=={parameters.Length})");
            AddTypedColumns(dbg, parameters);
            dbg.AppendStatement($"var {RowCount} = {ColumnName(parameters[0])}.RowCount");
            dbg.AppendStatement($"var data = new {ret.Type}[{RowCount}]");
            dbg.AppendLine($"for (var {RowIndex} = 0; {RowIndex} < {RowCount}; {RowIndex}++)");
            dbg.EnterCodeBlock();
            PerformNullChecks(dbg, parameters, true);
            var pvals = string.Join(",", parameters.Select(Val));

            dbg.AppendStatement($"data[{RowIndex}] = {method.Name}({pvals})");
            dbg.ExitCodeBlock();

            dbg.AppendStatement("return new ColumnarResult(ColumnFactory.Create(data))");
            dbg.ExitCodeBlock();

            return dbg.ToString();
        }


        public static void AddTypedColumns(CodeAcccumulator dbg, Param[] parameters)
        {
            foreach (var p in parameters)
            {
                dbg.AppendLine(MakeTypedColumn(p));
            }
        }

        public static void AddTypedVariables(CodeAcccumulator dbg, Param[] parameters)
        {
            foreach (var p in parameters)
            {
                dbg.AppendLine(MakeTypedVariable(p));
            }
        }

        public static void PerformNullChecks(CodeAcccumulator dbg, Param[] parameters, bool fromColumn)
        {
            foreach (var p in parameters)
            {
                if (fromColumn)
                    dbg.AppendLine($"var {p.Name} = {IndexedColumn(p)};");
                dbg.AppendLine(p.IsString
                    ? $"if (string.IsNullOrEmpty({p.Name})) continue;"
                    : $"if ({p.Name} == null) continue;");
            }
        }

        private static string IndexedColumn(Param p) => $"{ColumnName(p)}[{RowIndex}]";
    }
}