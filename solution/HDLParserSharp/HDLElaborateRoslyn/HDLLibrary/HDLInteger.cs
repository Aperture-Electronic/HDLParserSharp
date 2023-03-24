using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.HDLLibrary
{
    public struct HDLInteger : IEquatable<HDLInteger>, IEquatable<bool>
    {
        public BigInteger Value { get; }

        public int Bits;

        public HDLInteger(int bits, BigInteger value)
        {
            Value = value;
            Bits = bits;
        }

        public HDLInteger(int bits, params ulong[] values)
        {
            int length = values.Length;
            BigInteger value = values[0];

            for (int i = 1; i < length; i++)
            {
                value <<= 64;
                value |= new BigInteger(values[i]);
            }

            Bits = bits;
            Value = value;
        }

        public HDLInteger(BigInteger value)
        {
            Value = value;
            Bits = (int)value.GetBitLength();
        }

        public override string ToString() => $"{Value}({Bits})";

        public static HDLInteger Zero1Bit => new HDLInteger(1, 0);
        public static HDLInteger One1Bit => new HDLInteger(1, 1);

        public static bool operator ==(HDLInteger op1, HDLInteger op2)
            => op1.Value == op2.Value;

        public static bool operator !=(HDLInteger op1, HDLInteger op2)
            => op1.Value != op2.Value;

        public static HDLInteger operator +(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value + op2.Value);

        public static HDLInteger operator -(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value - op2.Value);

        public static HDLInteger operator *(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value * op2.Value);

        public static HDLInteger operator /(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value / op2.Value);

        public static HDLInteger operator %(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value % op2.Value);

        public static HDLInteger operator !(HDLInteger op1)
            => op1 ? Zero1Bit : One1Bit;

        public static HDLInteger operator &(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value & op2.Value);

        public static HDLInteger operator |(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value | op2.Value);

        public static HDLInteger operator ^(HDLInteger op1, HDLInteger op2)
            => new HDLInteger(op1.Value ^ op2.Value);

        public static HDLInteger operator ~(HDLInteger op1)
            => new HDLInteger(~op1.Value);

        public static HDLInteger operator <<(HDLInteger op1, int op2)
            => new HDLInteger(op1.Value << op2);

        public static HDLInteger operator >>(HDLInteger op1, int op2)
            => new HDLInteger(op1.Value >> op2);

        public static HDLInteger operator >>>(HDLInteger op1, int op2)
            => new HDLInteger(op1.Value >>> op2);

        public static bool operator <(HDLInteger op1, HDLInteger op2)
            => op1.Value < op2.Value;

        public static bool operator >(HDLInteger op1, HDLInteger op2)
            => op1.Value > op2.Value;

        public static bool operator <=(HDLInteger op1, HDLInteger op2)
            => op1.Value <= op2.Value;

        public static bool operator >=(HDLInteger op1, HDLInteger op2)
            => op1.Value >= op2.Value;

        public static HDLInteger operator ++(HDLInteger op1)
            => new HDLInteger(op1.Value + 1);
        public static HDLInteger operator --(HDLInteger op1)
            => new HDLInteger(op1.Value - 1);

        public static HDLInteger RotateLeft(HDLInteger op1, int op2)
            => new HDLInteger(BigInteger.RotateLeft(op1.Value, op2));

        public static HDLInteger RotateRight(HDLInteger op1, int op2)
            => new HDLInteger(BigInteger.RotateRight(op1.Value, op2));

        public static implicit operator int(HDLInteger op) => op.Bits <= 32
                ? (int)op.Value
                : throw new Exception($"The bit count of this HDLInteger({op.Bits}) is over the maximum bit count of int(32)");

        public static implicit operator HDLInteger(int op)
            => new HDLInteger(32, op);

        public static implicit operator HDLInteger(long op)
            => new HDLInteger(64, op);

        public static implicit operator HDLInteger(bool op)
            => op ? One1Bit : Zero1Bit;

        public static implicit operator bool(HDLInteger op)
            => op.Value != 0;

        public static implicit operator BigInteger(HDLInteger op)
            => op.Value;

        public static implicit operator HDLInteger(BigInteger op)
            => new HDLInteger(op);

        public bool Equals(bool other) => this == other;

        public static HDLInteger OrUnary(HDLInteger op1)
            => op1.Value != 0 ? One1Bit : Zero1Bit;

        public static HDLInteger AndUnary(HDLInteger op1)
        {
            if (op1 < 0)
            {
                return One1Bit; // Negative values has a sign-bit of 1
            }

            long bitsCount = op1.Bits;
            BigInteger value = op1.Value;
            for (int i = 0; i < bitsCount; i++)
            {
                if ((value & 0x01) == 0)
                {
                    return Zero1Bit;
                }

                value >>= 1;
            }

            return One1Bit;
        }

        public static HDLInteger XorUnary(HDLInteger op1)
        {
            long bitsCount = op1.Bits;
            BigInteger value = op1.Value;

            int firstReadBit = (int)(value & 0x01);
            value >>= 1;

            for (int i = 1; i < bitsCount; i++)
            {
                if ((int)(value & 0x01) != firstReadBit)
                {
                    return One1Bit;
                }

                value >>= 1;
            }

            return Zero1Bit;
        }

        public static HDLInteger Pow(HDLInteger op1, HDLInteger op2)
            => BigInteger.Pow(op1, (int)op2);

        public static HDLInteger Abs(HDLInteger op1)
            => BigInteger.Abs(op1);

        public static HDLInteger Rem(HDLInteger op1, HDLInteger op2)
            => BigInteger.Remainder(op1, op2);

        public bool Equals(HDLInteger other) => Value.Equals(other.Value);

        public static HDLInteger BitIndex(HDLInteger op1, int index)
        {
            if (index > op1.Bits)
            {
                throw new Exception("The bit index is over the bit count of the integer");
            }

            BigInteger shifted = op1.Value >> index;
            return shifted & 0x01;
        }

        public static HDLInteger DownTo(HDLInteger op1, int msb, int lsb)
        {
            if (msb < lsb)
            {
                // If MSB lower than LSB, swap them
                (msb, lsb) = (lsb, msb);
            }

            if (msb == lsb)
            {
                return BitIndex(op1, lsb);
            }

            if (msb > op1.Bits)
            {
                throw new Exception("The highest bit index is over the bit count of the integer");
            }

            BigInteger shifted = op1.Value >> lsb;
            int bitWidth = msb - lsb + 1;
            BigInteger mask = (BigInteger.One << bitWidth) - 1;

            return mask & shifted;
        }

        public static HDLInteger Concat(HDLInteger left, HDLInteger right)
        {
            int totalBits = left.Bits + right.Bits;
            int leftShiftBits = right.Bits;

            return new HDLInteger(totalBits, (left.Value << leftShiftBits) | right.Value);
        }

        public static HDLInteger ReplConcat(int count, HDLInteger op)
        {
            if (count <= 0)
            {
                throw new Exception("Invalid repeat count");
            }

            if (count == 1)
            {
                return op;
            }

            HDLInteger concat = op;
            do
            {
                concat = Concat(concat, op);
                count--;
            } while (count > 1);

            return concat;
        }
    }
}
