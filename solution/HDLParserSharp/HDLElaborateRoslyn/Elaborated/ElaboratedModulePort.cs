using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLElaborateRoslyn.Elaborator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.Elaborated
{
    public class ElaboratedModulePort
    {
        public IdentifierDefinition Definition { get; }

        public string Name => Definition.Name;

        public Direction Direction => Definition.Direction;

        private ElaboratedSize  packedSize;
        private ElaboratedSize? unpackedSize;

        public ElaboratedModulePort(IdentifierDefinition definition)
        {
            Definition = definition;
        }

        public Expression? SizeExpression
        {
            get
            {
                if (Definition.Type is Operator op)
                {
                    if ((op.Type == OperatorType.Parametrization) && (op.Operands.Count == 3)) 
                    {
                        return op.Operands[1];
                    }
                }

                return null;
            }
        }

        public void ElaborateSize(Func<Expression, int> evaluator)
        {
            if (SizeExpression is Symbol nullSymbol)
            {
                packedSize = new ElaboratedSize(1);
                unpackedSize = null;
            }
            else if (SizeExpression is Operator op)
            {
                if (op.Type == OperatorType.MultiDimension)
                {
                    // Both packed and unpacked size
                    Operator packed = (Operator)op[0];
                    Operator unpacked = (Operator)op[1];

                    Expression packedMSB = packed[0];
                    Expression packedLSB = packed[1];
                    Expression unpackedMSB = unpacked[0];
                    Expression unpackedLSB = unpacked[1];

                    packedSize = new ElaboratedSize(
                        msb: evaluator(packedMSB),
                        lsb: evaluator(packedLSB));

                    unpackedSize = new ElaboratedSize(
                        msb: evaluator(unpackedMSB),
                        lsb: evaluator(unpackedLSB));
                }
                else if (op.Type == OperatorType.Downto)
                {
                    // [A:B]
                    Expression msb = op[0];
                    Expression lsb = op[1];

                    packedSize = new ElaboratedSize(
                        msb: evaluator(msb), 
                        lsb: evaluator(lsb));
                    unpackedSize = null;
                }
            }
        }

        public (ElaboratedSize packed, ElaboratedSize? unpacked) Size => (packedSize, unpackedSize);

        public override string ToString()
        {
            if (unpackedSize != null)
            {
                return $"{packedSize} {Name} {unpackedSize} ";
            }
            else
            {
                return $"{packedSize} {Name} ";
            }
        }
    }
}
