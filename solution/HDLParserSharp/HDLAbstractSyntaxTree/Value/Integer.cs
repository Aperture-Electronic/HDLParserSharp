using HDLAbstractSyntaxTree.Common;
using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    public class Integer : Expression
    {
        public int Bits { get; }

        public BigInteger Value { get; }

        public Integer(BigInteger value, int bits)
        {
            Bits = bits;
            Value = value;
        }

        public Integer(BigInteger value) : this(value, -1) 
        { 
        
        }

        public Integer(string bitString, BigInteger.Base radix)
            : this(new BigInteger(bitString, radix))
        {

        }

        public Integer(string bitString, int bits, BigInteger.Base radix)
            : this(new BigInteger(bitString, radix), bits)
        {

        }

        public override Expression Clone() => new Integer(Value, Bits);
    }
}
