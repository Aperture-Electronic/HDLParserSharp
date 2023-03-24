using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    public class Integer : Expression
    {
        public int Bits { get; }

        public enum Base
        {
            Binary = 2,
            Octal = 8,
            Decimal = 10,
            Hexadecimal = 16,
        }

        public BigInteger Value { get; }

        /// <summary>
        /// Create an integer with specificed bits
        /// </summary>
        public Integer(BigInteger value, int bits)
        {
            Bits = bits;
            Value = value;
        }

        /// <summary>
        /// Create a 32-bit standard integer
        /// </summary>
        /// <param name="value"></param>
        public Integer(BigInteger value) : this(value, 32) 
        { 
        
        }

        private static string TrimBitStringZero(string bitString)
            => bitString.Length == 1 ? bitString : (bitString[0..^1].TrimStart('0') + bitString[^1]);

        /// <summary>
        /// Create an integer with bit string and radix
        /// The bit length will auto determined by the bit string length (without prefix 0)
        /// </summary>
        public Integer(string bitString, Base radix)
            : this(bitString, 0, radix)
        {
            // Infer the bit length
            if (Value > 0)
            {
                // Positive
                Bits = 0;
                byte[] bytes = Value.ToByteArray(isBigEndian: true);
                Bits += 8 * (bytes.Length - 1); 

                // Final byte
                byte msb = bytes.First();
                while (msb > 0)
                {
                    Bits++;
                    msb >>= 1;
                }
            }
            else if (Value < 0)
            {
                // Negative
                Bits = 0;
                byte[] bytes = Value.ToByteArray(isBigEndian: true);
                Bits += 8 * (bytes.Length - 1);

                // Final byte
                byte msb = bytes.First();
                byte mask = 0xFF;
                while ((msb & mask) != 0xFF)
                {
                    Bits++;
                    msb >>= 1;
                    mask >>= 1;
                }
            }
            else
            {
                // Zero, only bit 1
                Bits = 1;
            }
        }

        private int ConvertBitCharacterToNumber(char bitChar) =>
            char.IsDigit(bitChar) ? (bitChar - '0') : (bitChar - 'A' + 10);

        private BigInteger ParseBigIntegerRadix(string bitString, Base radix)
        {
            string noPrefixZero = TrimBitStringZero(bitString);
            int baseWeight = (int)radix;
            int stringLength = noPrefixZero.Length;
            BigInteger integer = BigInteger.Zero;

            for (int i = 0; i < stringLength; i++)
            {
                integer *= baseWeight;

                char bitChar = bitString[i];
                int bitNumber = ConvertBitCharacterToNumber(bitChar);

                integer += bitNumber;
            }

            return integer;
        }

        /// <summary>
        /// Create an integer with bit string and radix, 
        /// with a specificed bit length
        /// </summary>
        public Integer(string bitString, int bits, Base radix)
        {
            Bits = bits;
            Value = radix == Base.Decimal ? 
                             BigInteger.Parse(bitString) : 
                             ParseBigIntegerRadix(bitString, radix);
        }

        public override Expression Clone() => new Integer(Value, Bits);

        public override string ToString() => $"{Value}({Bits})";
    }
}
