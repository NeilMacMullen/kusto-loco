﻿using System;
using System.Collections.Generic;
using System.Text;

namespace KustoLoco.SourceGeneration
{
    public class CustomAttributeHelper<T>
        where T : Attribute
    {
        private readonly Dictionary<string, string> _attributes;

        public CustomAttributeHelper(Dictionary<string, string> attributes)
        {
            _attributes = attributes;
        }


        public static string Name()
        {
            var fullName = typeof(T).Name;
            return fullName.EndsWith("Attribute")
                ? fullName.Substring(0, fullName.Length - "Attribute".Length)
                : fullName;
        }

        public string Unquote(string other)
        {
            if (other.StartsWith("\""))
                other = other.Substring(1);
            if (other.EndsWith("\""))
                other = other.Substring(0, other.Length - 1);
            return other;
        }

        public string GetStringFor(string keywordName) =>
            _attributes.TryGetValue(keywordName, out var p) ? Unquote(p) : string.Empty;

        public string Dump()
        {
            var sb = new StringBuilder();
            foreach (var a in _attributes) sb.AppendLine($"'{a.Key}' => '{a.Value}'");

            return sb.ToString();
        }
    }
}
