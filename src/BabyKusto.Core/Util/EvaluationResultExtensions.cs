// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using BabyKusto.Core.Evaluation;
using Microsoft.Extensions.Internal;

namespace BabyKusto.Core.Extensions
{
    public static class EvaluationResultExtensions
    {
        public static void Dump(this EvaluationResult? result, TextWriter writer, int indent = 0)
        {
            switch (result)
            {
                case TabularResult tabularResult:
                    tabularResult.Value.Dump(writer, indent);
                    break;
                case ScalarResult scalarResult:
                    if (indent > 0)
                    {
                        writer.Write(new string(' ', indent));
                    }
                    writer.Write(scalarResult.Value);
                    writer.Write(" (");
                    writer.Write(scalarResult.Type.Display);
                    writer.WriteLine(")");
                    break;
                default:
                    throw new NotSupportedException($"Unsupported evaluation result type cannot be dumped: {TypeNameHelper.GetTypeDisplayName(result)}");
            }
        }
    }
}
