using Antlr4.Runtime;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using System;
using System.Collections.Generic;
using System.Text;
using SystemVerilog2017Interpreter.Types;

namespace SystemVerilog2017Interpreter.Extensions
{
    internal static class ExpressionExtension
    {
        public static Expression Append(this Expression? selectedName, ParserRuleContext context, OperatorType operatorJoin, Expression newPart)
            => selectedName == null ? newPart : new Operator(operatorJoin, selectedName, newPart).UpdateCodePosition(context);
       
        public static Expression WireIdentifier => new Identifier("wire");

        public static Expression IntegerIdentifier => new Identifier("integer");

        public static Expression Signing(SigningValue signing) =>
            signing switch
            {
                SigningValue.NoSign => SymbolType.Null.AsNewSymbol(),
                SigningValue.Signed => new Integer(1),
                SigningValue.Unsigned => new Integer(0),
                _ => throw new NotSupportedException()
            };

        public static Expression AssignWireType(this Expression? netType, ParserRuleContext? context, Expression range, SigningValue signing)
        {
            List<Expression> operands = new List<Expression>
            {
                range,
                Signing(signing)
            };

            netType ??= WireIdentifier;
            Operator op = Operator.Parametrization(netType, operands);

            if (context != null)
            {
                op.UpdateCodePosition(context);
            }

            return op;
        }
    }
}
