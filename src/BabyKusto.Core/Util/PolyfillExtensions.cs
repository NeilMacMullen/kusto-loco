// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if !NETSTANDARD2_1_OR_GREATER
namespace System.Collections.Generic
{
    internal static class PolyfillDictionaryExtensions
    {
        internal static bool TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        where TKey :notnull
        {
            try
            {
                dictionary.Add(key, value);
                return true;
            }
            catch (ArgumentException ex) when (ex is not ArgumentNullException)
            {
                return false;
            }
        }
    }
}
#endif
