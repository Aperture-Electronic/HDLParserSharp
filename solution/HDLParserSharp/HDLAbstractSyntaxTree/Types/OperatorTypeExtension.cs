using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Types
{
    public static class OperatorTypeExtension
    {
        public static OperatorType ToUnary(this OperatorType type)
            => type switch
            { 
                OperatorType.Sub => OperatorType.MinusUnary,
                OperatorType.Add => OperatorType.PlusUnary,
                OperatorType.Or => OperatorType.OrUnary,
                OperatorType.And => OperatorType.AndUnary,
                OperatorType.Nand => OperatorType.NandUnary,
                OperatorType.Nor => OperatorType.NorUnary,
                OperatorType.Xor => OperatorType.XorUnary,
                OperatorType.Xnor => OperatorType.XnorUnary,
                _ => throw new NotImplementedException("Invalid operator type")
            };
    }
}
