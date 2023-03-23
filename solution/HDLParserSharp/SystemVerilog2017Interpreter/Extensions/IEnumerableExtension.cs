using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SystemVerilog2017Interpreter.Extensions
{
    internal static class IEnumerableExtension
    {
        public static IEnumerable<T> ReplaceSequenceWithOneValue<T>(this IEnumerable<T> sequence,
            T firstToReplace, int replaceLength, T replacement)
            where T : class
        {
            bool startReplace = false;
            int replaceCount = 0;

            foreach (var item in sequence)
            {
                if (item == firstToReplace)
                {
                    yield return replacement;
                    startReplace = true;
                }

                if (startReplace)
                {
                    replaceCount++;
                    if (replaceCount == replaceLength)
                    {
                        startReplace = false;
                    }
                }
                else
                {
                    yield return item;
                }
            }
        }
    }
}
