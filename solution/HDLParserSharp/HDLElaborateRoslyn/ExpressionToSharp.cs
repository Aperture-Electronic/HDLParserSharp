using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;

namespace HDLElaborateRoslyn
{
    public class ExpressionToSharp
    {
        private static string IdentifierToSharp(Identifier identifier)
            => $"hdl_id_{identifier}";

        private static string RealToSharp(Real real)
            => $"{real.Value}";

        private static string StringToSharp(HDLAbstractSyntaxTree.Value.String str)
            => $"\"{str.Content}\"";

        private static string IntegerToSharp(Integer integer)
        {
            // The integer under 64-bit (long) can be directly parse
            if ((integer.Value <= long.MaxValue) && (integer.Value >= long.MinValue))
            {
                return ((long)integer.Value).ToString();
            }
            else
            {
                // Else, we need to convert it to byte array
                // and create a new BigInteger in the output code
                byte[] byteArray = integer.Value.ToByteArray();
                // BigInteger bigInteger = new BigInteger(new byte { 0x01, ... });
                string byteArrayString = string.Join(',', byteArray.Select(x => x.ToString()));
                return $"new BigInteger(new byte {{{byteArrayString}}})";
            }
        }

        private static string OperatorToSharp(Operator op)
        { 
            // Process operands
            string[] x = op.Operands.Select(o => ToSharp(o)).ToArray();

            // Return the expression
            return op.Type switch
            {
                OperatorType.Sub => $"({x[0]}) - ({x[1]})",
                OperatorType.Ternary => $"({x[0]}) ? ({x[1]}) : ({x[2]})",
                OperatorType.Gt => $"({x[0]}) > ({x[1]})",
                _ => ""
            };
        }

        public static string ToSharp(Expression expression)
        {
            return expression switch
            {
                Operator op => OperatorToSharp(op),
                Identifier identifier => IdentifierToSharp(identifier),
                Integer integer => IntegerToSharp(integer),
                _ => string.Empty
            };
        }
    }
}
