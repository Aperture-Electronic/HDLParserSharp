using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.HDLLibrary
{
    public static class ArithmeticMath
    {
        public static HDLInteger Pow(HDLInteger op1, HDLInteger op2)
            => HDLInteger.Pow(op1, op2);

        public static HDLInteger Abs(HDLInteger op1)
            => HDLInteger.Abs(op1);

        public static HDLInteger Rem(HDLInteger op1, HDLInteger op2)
            => HDLInteger.Rem(op1, op2);

        public static HDLInteger CeilingLog2(HDLInteger op1)
        {
            BigInteger value = op1.Value;
            BigInteger log2 = BigInteger.Log2(value);
            BigInteger power = BigInteger.Pow(2, (int)log2);
            return (power < value) ? log2 + 1 : log2;
        }
    }
}
