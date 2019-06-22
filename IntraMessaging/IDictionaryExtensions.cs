using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IntraMessaging
{
    public static class IDictionaryExtensions
    {
        public static IDictionary<K, V> CastReadOnly<K, V>(this IDictionary dictionary)
        {
            return CastHelper(dictionary).ToDictionary(k => (K)k.Key, v => (V)v.Value);
        }

        private static IEnumerable<DictionaryEntry> CastHelper(IDictionary dictionary)
        {
            foreach (DictionaryEntry entry in dictionary)
            {
                yield return entry;
            }
        }
    }
}
