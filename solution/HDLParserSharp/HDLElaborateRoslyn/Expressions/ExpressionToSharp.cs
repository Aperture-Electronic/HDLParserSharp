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
using HDLElaborateRoslyn.HDLLibrary;

namespace HDLElaborateRoslyn.Expressions
{
    public static class ExpressionToSharp
    {
        private static string IdentifierToSharp(Identifier identifier)
            => $"Global.hdl_id_{identifier}";

        private static string RealToSharp(Real real)
            => $"{real.Value}";

        private static string StringToSharp(HDLAbstractSyntaxTree.Value.String str)
            => $"\"{str.Content}\"";

        private static string IntegerToSharp(Integer integer)
        {
            if (integer.Bits > 64)
            {
                BigInteger value = integer.Value;
                BigInteger mask = ulong.MaxValue;
                List<string> param = new List<string>();
                do
                {
                    param.Add($"{(ulong)(value & mask)}");
                    value >>= 64;
                } while (value > 0);

                return $"new HDLInteger({integer.Bits}, {string.Join(',', param)})";
            }
            else
            {
                return $"new HDLInteger({integer.Bits}, {integer.Value})";
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
                OperatorType.MinusUnary => $"-({x[0]})",
                OperatorType.PlusUnary => $"+({x[0]})",
                OperatorType.Add => $"({x[0]}) + ({x[1]})",
                OperatorType.Div => $"({x[0]}) / ({x[1]})",
                OperatorType.Mul => $"({x[0]}) * ({x[1]})",
                OperatorType.Mod => $"({x[0]}) % ({x[1]})",
                OperatorType.Rem => $"ArithmeticMath.Rem({x[0]}, {x[1]})",
                OperatorType.Pow => $"ArithmeticMath.Pow({x[0]}, {x[1]})",
                OperatorType.Abs => $"ArithmeticMath.Abs({x[0]})",
                OperatorType.IncrPre => $"++{x[0]}",
                OperatorType.DecrPre => $"--{x[0]}",
                OperatorType.IncrPost => $"{x[0]}++",
                OperatorType.DecrPost => $"{x[0]}++",
                OperatorType.NegLog => $"BitLogic.NegLog(({x[0]}))",
                OperatorType.Neg => $"BitLogic.Neg(({x[0]}))",
                OperatorType.AndLog => $"BitLogic.AndLog(({x[0]}), ({x[1]}))",
                OperatorType.OrLog => $"BitLogic.OrLog(({x[0]}), ({x[1]}))",
                OperatorType.And => $"BitLogic.And(({x[0]}), ({x[1]}))",
                OperatorType.Or => $"BitLogic.Or(({x[0]}), ({x[1]}))",
                OperatorType.Nand => $"BitLogic.Nand(({x[0]}), ({x[1]}))",
                OperatorType.Nor => $"BitLogic.Nor(({x[0]}), ({x[1]}))",
                OperatorType.Xor => $"BitLogic.Xor(({x[0]}), ({x[1]}))",
                OperatorType.Xnor => $"BitLogic.Xnor(({x[0]}), ({x[1]}))",
                OperatorType.OrUnary => $"BitLogic.OrUnary(({x[0]}))",
                OperatorType.AndUnary => $"BitLogic.AndUnary(({x[0]}))",
                OperatorType.NandUnary => $"BitLogic.NandUnary(({x[0]}))",
                OperatorType.NorUnary => $"BitLogic.NorUnary(({x[0]}))",
                OperatorType.XorUnary => $"BitLogic.XorUnary(({x[0]}))",
                OperatorType.XnorUnary => $"BitLogic.XnorUnary(({x[0]}))",
                OperatorType.Sll => $"BitLogic.Sll(({x[0]}), ({x[1]}))",
                OperatorType.Srl => $"BitLogic.Srl(({x[0]}), ({x[1]}))",
                OperatorType.Sla => $"BitLogic.Sla(({x[0]}), ({x[1]}))",
                OperatorType.Sra => $"BitLogic.Sra(({x[0]}), ({x[1]}))",
                OperatorType.Rol => $"BitLogic.Rol(({x[0]}), ({x[1]}))",
                OperatorType.Ror => $"BitLogic.Ror(({x[0]}), ({x[1]}))",
                OperatorType.Eq => $"({x[0]}) == ({x[1]})",
                OperatorType.Ne => $"({x[0]}) != ({x[1]})",
                OperatorType.Gt => $"({x[0]}) > ({x[1]})",
                OperatorType.Lt => $"({x[0]}) < ({x[1]})",
                OperatorType.Le => $"({x[0]}) <= ({x[1]})",
                OperatorType.Ge => $"({x[0]}) >= ({x[1]})",
                OperatorType.Index => $"BitLogic.Index(({x[0]}), ({x[1]}))", 
                OperatorType.Concat => $"BitLogic.Concat(({x[0]}), ({x[1]}))",
                OperatorType.ReplConcat => $"BitLogic.ReplConcat(({x[0]}), ({x[1]}))",
                OperatorType.PartSelectPost => $"new PartSelect(({x[0]}), ({x[1]}), true)",
                OperatorType.PartSelectPre => $"new PartSelect(({x[0]}), ({x[1]}), false)",
                OperatorType.Downto => $"new DownTo(({x[0]}), ({x[1]}))",
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
                Real real => RealToSharp(real),
                HDLAbstractSyntaxTree.Value.String str => StringToSharp(str),
                _ => string.Empty
            };
        }
    }
}
