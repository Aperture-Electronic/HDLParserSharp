using System;
using System.Collections.Generic;
using System.Text;

namespace SystemVerilog2017Interpreter.Extensions
{
    internal static class DictionaryExtension
    {
        public static void AddRange<T, U>(this Dictionary<T, U> dictionary, IDictionary<T, U> toAdd)
        {
            foreach (var item in toAdd) 
            { 
                dictionary.Add(item.Key, item.Value);
            }
        }
    }
}
