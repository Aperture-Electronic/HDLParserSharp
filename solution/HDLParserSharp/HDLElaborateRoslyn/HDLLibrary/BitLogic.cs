using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace HDLElaborateRoslyn.HDLLibrary
{
    /// <summary>
    /// This library is for the bit logic expressions (like logic operator or shift)
    /// in HDL converted to Roslyn C# script
    /// </summary>
    public static class BitLogic
    {
        private static bool ToLogic<T>(T op) where T : IEquatable<bool> => op.Equals(true);

        public static bool NegLog(bool op) => !op;
        public static bool NegLog(HDLInteger op) => NegLog(ToLogic(op));

        public static bool AndLog(bool op1, bool op2) => op1 && op2;
        public static bool AndLog<T, U>(T op1, U op2) 
            where T : IEquatable<bool>
            where U : IEquatable<bool>
            => AndLog(ToLogic(op1), ToLogic(op2));

        public static bool OrLog(bool op1, bool op2) => op1 || op2;
        public static bool OrLog<T, U>(T op1, U op2)
            where T : IEquatable<bool>
            where U : IEquatable<bool>
            => OrLog(ToLogic(op1), ToLogic(op2));

        public static HDLInteger And(HDLInteger op1, HDLInteger op2) => op1 & op2;

        public static HDLInteger Or(HDLInteger op1, HDLInteger op2) => op1 | op2;

        public static HDLInteger Neg(HDLInteger op) => ~op;

        public static HDLInteger Nand(HDLInteger op1, HDLInteger op2) => ~(op1 & op2);

        public static HDLInteger Nor(HDLInteger op1, HDLInteger op2) => ~(op1 | op2);

        public static HDLInteger Xor(HDLInteger op1, HDLInteger op2) => (op1 ^ op2);

        public static HDLInteger Xnor(HDLInteger op1, HDLInteger op2) => ~(op1 ^ op2);

        public static HDLInteger Sll(HDLInteger op1, int op2) => op1 << op2;

        public static HDLInteger Srl(HDLInteger op1, int op2) => op1 >> op2;

        public static HDLInteger Sla(HDLInteger op1, int op2) => op1 << op2;

        public static HDLInteger Sra(HDLInteger op1, int op2) => op1 >>> op2;

        public static HDLInteger Rol(HDLInteger op1, int op2) => HDLInteger.RotateLeft(op1, op2);

        public static HDLInteger Ror(HDLInteger op1, int op2) => HDLInteger.RotateRight(op1, op2);

        public static HDLInteger OrUnary(HDLInteger op) => HDLInteger.OrUnary(op);

        public static HDLInteger AndUnary(HDLInteger op) => HDLInteger.AndUnary(op);

        public static HDLInteger NorUnary(HDLInteger op) => !HDLInteger.OrUnary(op);

        public static HDLInteger NandUnary(HDLInteger op) => !HDLInteger.AndUnary(op);

        public static HDLInteger XorUnary(HDLInteger op) => HDLInteger.XorUnary(op);

        public static HDLInteger XnorUnary(HDLInteger op) => !HDLInteger.XorUnary(op);

        public static HDLInteger Index(HDLInteger op, HDLInteger index) => HDLInteger.BitIndex(op, index);

        public static HDLInteger Index(HDLInteger op, DownTo index) => HDLInteger.DownTo(op, index.MSB, index. LSB);

        public static HDLInteger Concat(HDLInteger left, HDLInteger right) => HDLInteger.Concat(left, right);

        public static HDLInteger ReplConcat(HDLInteger count, HDLInteger op) => HDLInteger.ReplConcat(count, op);
    }
}
