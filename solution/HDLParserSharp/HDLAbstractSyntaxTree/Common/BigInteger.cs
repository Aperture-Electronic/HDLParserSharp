using HDLAbstractSyntaxTree.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Common
{
    public struct BigInteger
    {
        public enum Base
        {
            Invalid = -1,
            Binary = 2,
            Octal = 8,
            Decimal = 10,
            Hexadecimal = 16,
            Character = 256
        }

        public long Value { get; }
        public string BitString { get; }

        public Base Radix;

        public bool IsBitString => Radix != Base.Invalid;

        public BigInteger(long value)
        {
            Value = value;
            BitString = string.Empty;
            Radix = Base.Invalid;
        }

        public BigInteger(string bitString, Base radix)
        {
            Value = 0;
            BitString = bitString;
            Radix = radix;
        }

        public static implicit operator BigInteger(long value)
            => new BigInteger(value);
    }
}
