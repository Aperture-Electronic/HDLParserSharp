using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Common
{
    /// <summary>
    /// Indicate a range of value
    /// </summary>
    /// <typeparam name="T">Value type</typeparam>
    public struct ValueRange<T> where T : IFormattable
    {
        public T Start { get; set; }
        public T End { get; set; }

        public ValueRange(T start, T end)
        {
            Start = start;
            End = end;
        }

        public ValueRange(ValueRange<T> copy)
        {
            Start = copy.Start;
            End = copy.End;
        }

        public override string ToString() => $"{Start}:{End}";
    }
}
