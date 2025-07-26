﻿using System.Linq;

namespace KustoLoco.SourceGeneration
{
    public static class ParamGeneneration
    {
        private const string RowIndex = "rowIndex";

        private const string RowCount = "rowCount";
        public static string Val(Param p) => p.IsRefType ? $"{p.Name}" : $"{p.Name}.Value";

        public static string ColumnName(Param p)
            => $"{p.Name}Column";

        public static string VariableName(Param p)
            => $"{p.Name}";

        public static string GetNullableType(Param p)
            => p.Type.Replace("?", "") + "?";

        public static string MakeTypedColumn(Param p) =>
            $"var {ColumnName(p)} = (TypedBaseColumn<{GetNullableType(p)}>) arguments[{p.ColumnIndex}].Column;";

        public static string MakeTypedVariable(Param p) =>
            $"var {VariableName(p)} = ({GetNullableType(p)}) arguments[{p.ColumnIndex}].Value;";

        public static void BuildOverloadInfo(CodeEmitter dbg, ImplementationMethod method)
        {
            dbg.AppendLine($"internal static {method.OverloadName}  Overload =>");
            dbg.AppendLine($"new(new {method.ClassName}(),");
            var mappedTypes = new[] { method.ReturnType }.Concat(method.TypedArguments)
                .Select(p => $"TypeMapping.SymbolForType(typeof({p.Type}))");
            var arglist = string.Join(",", mappedTypes);
            dbg.AppendLine(arglist);
            dbg.AppendStatement(")");
        }

        public static void BuildScalarMethod(CodeEmitter dbg, ImplementationMethod method)
        {
            var parameters = method.TypedArguments;
            var ret = method.ReturnType;
            dbg.AppendLine("public ScalarResult InvokeScalar(ScalarResult[] arguments)");
            dbg.EnterCodeBlock();
            dbg.AppendStatement($"Debug.Assert(arguments.Length=={parameters.Length})");
            AddTypedVariables(dbg, parameters);
            dbg.AppendStatement($"{GetNullableType(ret)} data=null");
            if (method.HasContext)
            {
                dbg.AppendStatement($"var context = new {method.ContextArgument.Type}()");
            }

            dbg.AppendLine("for(var i=0;i < 1;i++)");
            dbg.EnterCodeBlock();
            PerformNullChecks(dbg, method, false, "data");


            var pvals = string.Join(",", parameters.Select(Val));
            if (method.HasContext)
                pvals = $"context,{pvals}";
            dbg.AppendStatement($"data = {method.Name}({pvals})");
            dbg.ExitCodeBlock();

            dbg.AppendStatement($"var returnType =TypeMapping.SymbolForType(typeof({ret.Type}))");
            dbg.AppendStatement("return new ScalarResult(returnType,data)");
            dbg.ExitCodeBlock();
        }

        public static void BuildColumnarMethod(CodeEmitter dbg, ImplementationMethod method)
        {
            var parameters = method.TypedArguments;
            var ret = method.ReturnType;
            dbg.AppendLine("public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)");
            dbg.EnterCodeBlock();
            dbg.AppendStatement($"Debug.Assert(arguments.Length=={parameters.Length})");
            AddTypedColumns(dbg, parameters);
            dbg.AppendStatement($"var {RowCount} = {ColumnName(parameters[0])}.RowCount");
            dbg.AppendStatement($"var data = new {GetNullableType(ret)}[{RowCount}]");
            if (method.HasContext)
            {
                dbg.AppendStatement($"var context = new {method.ContextArgument.Type}()");
            }

            dbg.AppendLine($"for (var {RowIndex} = 0; {RowIndex} < {RowCount}; {RowIndex}++)");
            dbg.EnterCodeBlock();
            PerformNullChecks(dbg, method, true, $"data[{RowIndex}]");
            var pvals = string.Join(",", parameters.Select(Val));
            if (method.HasContext)
                pvals = $"context,{pvals}";
            dbg.AppendStatement($"data[{RowIndex}] = {method.Name}({pvals})");
            dbg.ExitCodeBlock();

            dbg.AppendStatement("return new ColumnarResult(ColumnFactory.Create(data))");
            dbg.ExitCodeBlock();
        }

        public static void BuildInvokeMethod(CodeEmitter dbg, ImplementationMethod method)
        {
            var parameters = method.TypedArguments;
            var ret = method.ReturnType;
            dbg.AppendLine("public EvaluationResult Invoke(ITableChunk chunk,ColumnarResult[] arguments)");
            dbg.EnterCodeBlock();
            dbg.AppendStatement($"Debug.Assert(arguments.Length=={parameters.Length})");
            AddTypedColumns(dbg, parameters);
            dbg.AppendStatement($"var {RowCount} = {ColumnName(parameters[0])}.RowCount");

            if (method.HasContext)
            {
                dbg.AppendStatement($"var context = new {method.ContextArgument.Type}()");
            }

            dbg.AppendLine($"for (var {RowIndex} = 0; {RowIndex} < {RowCount}; {RowIndex}++)");
            dbg.EnterCodeBlock();
            PerformNullChecks(dbg, method, true, $"data[{RowIndex}]");
            var pvals = string.Join(",", parameters.Select(Val));
            if (method.HasContext)
                pvals = $"context,{pvals}";
            dbg.AppendStatement($"{method.Name}({pvals})");
            dbg.ExitCodeBlock();
            dbg.AppendStatement($"var result = {method.Name}Finish(context)");
            dbg.AppendStatement($"var returnType =TypeMapping.SymbolForType(typeof({ret.Type}))");
            dbg.AppendStatement("return new ScalarResult(returnType,result)");
            dbg.ExitCodeBlock();
        }

        public static void AddTypedColumns(CodeEmitter dbg, Param[] parameters)
        {
            foreach (var p in parameters)
            {
                dbg.AppendLine(MakeTypedColumn(p));
            }
        }

        public static void AddTypedVariables(CodeEmitter dbg, Param[] parameters)
        {
            foreach (var p in parameters)
            {
                dbg.AppendLine(MakeTypedVariable(p));
            }
        }

        public static void PerformNullChecks(CodeEmitter dbg, ImplementationMethod method,
            bool fromColumn, string assignEmptyStringTo)
        {
            var parameters = method.TypedArguments;
            foreach (var p in parameters)
            {
                if (fromColumn)
                    dbg.AppendLine($"var {p.Name} = {IndexedColumn(p)};");
                if (p.IsString)
                {
                    dbg.AppendStatement($"{p.Name} = {p.Name}?? string.Empty");
                }
                else
                {
                    dbg.AppendLine($"if ({p.Name} == null)");
                    dbg.EnterCodeBlock();
                    if (method.ReturnType.IsString)
                        dbg.AppendStatement($"{assignEmptyStringTo}=string.Empty");
                    dbg.AppendStatement("continue");
                    dbg.ExitCodeBlock();
                }
            }
        }

        private static string IndexedColumn(Param p) => $"{ColumnName(p)}[{RowIndex}]";

        public static string GetNonNullableType(Param p) => GetNullableType(p).Replace("?", "");

        public static string ScalarType(Param p)
        {
            var t = GetNonNullableType(p);
            switch (t)
            {
                case "string":
                    return "String";
                case "int":
                    return "Int";
                case "long": return "Long";
                case "double": return "Real";
                case "bool": return "Bool";
                case "guid": return "Guid";
                case "JsonNode": return "Dynamic";
                case "JsonArray": return "Dynamic";
                default: return t;
            }
        }
    }
}
