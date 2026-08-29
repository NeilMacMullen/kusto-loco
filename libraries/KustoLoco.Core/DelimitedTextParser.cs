//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace KustoLoco.Core;

/// <summary>
/// Splits delimited text into rows of string cells, following the delimiter implied by a KQL
/// <c>with (format=...)</c> specifier. Provided by the engine so a host writing an
/// <see cref="IExternalDataResolver"/> does not have to reimplement delimited parsing: the host owns transport and
/// policy, the engine owns the format the query declared.
/// </summary>
public static class DelimitedTextParser
{
    /// <summary>
    /// The delimiter for a KQL data format name. The delimited family is csv / tsv / tsve / scsv / sohsv / psv;
    /// anything else (including an absent format) falls back to comma, matching the KQL default.
    /// </summary>
    public static char DelimiterFor(string? format) => (format?.Trim().ToLowerInvariant()) switch
    {
        "tsv" or "tsve" => '\t',
        "scsv" => ';',
        "sohsv" => '',
        "psv" => '|',
        _ => ',',
    };

    /// <summary>
    /// Parse delimited <paramref name="content"/> into rows of cells. Fields may be double-quoted, in which case a
    /// doubled quote is a literal quote and the delimiter and line breaks are taken literally (RFC4180). Blank lines
    /// are skipped. Line endings may be CRLF or LF.
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<string>> Parse(string content, char delimiter)
    {
        var rows = new List<IReadOnlyList<string>>();
        if (string.IsNullOrEmpty(content))
            return rows;

        var row = new List<string>();
        var cell = new StringBuilder();
        var inQuotes = false;
        var cellWasQuoted = false;

        void EndCell()
        {
            row.Add(cell.ToString());
            cell.Clear();
            cellWasQuoted = false;
        }

        void EndRow()
        {
            EndCell();
            // A blank line (one empty, unquoted cell) carries no data.
            if (row.Count > 1 || row[0].Length > 0)
                rows.Add(row.ToArray());
            row.Clear();
        }

        for (var i = 0; i < content.Length; i++)
        {
            var c = content[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"') { cell.Append('"'); i++; }
                    else inQuotes = false;
                }
                else cell.Append(c);
                continue;
            }

            if (c == '"' && cell.Length == 0 && !cellWasQuoted) { inQuotes = true; cellWasQuoted = true; }
            else if (c == delimiter) EndCell();
            else if (c == '\r') { /* handled by the \n that follows, or ignored at EOF */ }
            else if (c == '\n') EndRow();
            else cell.Append(c);
        }

        // Trailing content with no final newline is still a row.
        if (cell.Length > 0 || row.Count > 0) EndRow();

        return rows;
    }
}
