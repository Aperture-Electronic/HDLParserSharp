using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class ExpressionParser : HDLParser
    {
        public ExpressionParser(HDLParser other) : base(other)
        {

        }

        public Expression VisitConstantExpression(Constant_expressionContext context)
            => VisitExpression(context.expression());

        public Expression VisitRangeExpression(Range_expressionContext context)
        {
            var expressionContext = context.expression();
            if (expressionContext.Any())
            {
                var rangeLower = VisitExpression(expressionContext.First());
                if (expressionContext.Length == 1)
                {
                    return rangeLower;
                }
                else if (expressionContext.Length == 2)
                {
                    var rangeHigher = VisitExpression(expressionContext.Last());
                    return new Operator(HDLAbstractSyntaxTree.Types.OperatorType.Downto,
                        rangeLower, rangeHigher).UpdateCodePosition(context);
                }
            }

            throw new Exception("No valid range expression in context");
        }

        public Expression VisitNetLeftValue(Net_lvalueContext context)
        {
            var variableContext = context.variable_lvalue();
            if (variableContext != null)
            {
                return VisitVariableLeftValue(variableContext);
            }

            throw new Exception("No valid left value in context");
        }

        public Expression VisitPackageScope(Package_scopeContext context)
        {
            var systemCall = context.KW_DOLAR_UNIT();
            if (systemCall != null)
            {
                return new Identifier("$unit").UpdateCodePosition(context);
            }
            else
            {
                var identifier = context.identifier();
                if (identifier != null)
                {
                    return VisitIdentifier(identifier);
                }
            }

            throw new Exception("No valid package scope in context");
        }

        public Expression VisitPackageScopeIdentifier(Ps_identifierContext context)
        {
            var idContext = context.identifier();
            if (idContext == null)
            {
                throw new Exception("No valid package scope identifier in context");
            }

            var identifier = VisitIdentifier(idContext);
            var psContext = context.package_scope();
            if (psContext != null)
            {
                var packageScope = VisitPackageScope(psContext);
                // Sentences like a::b
                identifier = new Operator(OperatorType.DoubleColon, packageScope, identifier);
            }

            return identifier;
        }

        public Expression VisitArrayRangeExpression(Array_range_expressionContext context)
        {
            var exprContext = context.expression();
            var lowerExpression = VisitExpression(exprContext.First());
            if (exprContext.Length == 1)
            {
                return lowerExpression;
            }
            else if (exprContext.Length != 2) 
            {
                throw new Exception("The array range expression is not allowed over 2 items");
            }
            
            var higherExpression = VisitExpression(exprContext.Last());
            var plusMinusContext = context.operator_plus_minus();
            OperatorType opType = OperatorType.Downto;
            if (plusMinusContext != null)
            {
                // Sentences like [A +: B] or [A -: B]
                // A is the offset and B is the width

                var plusMinus = LiteralParser.VisitOperatorPlusMinus(plusMinusContext);
                opType = plusMinus switch
                {
                    OperatorType.Add => OperatorType.PartSelectPost,
                    _ => OperatorType.PartSelectPre
                };
            }

            return new Operator(opType, lowerExpression, higherExpression).UpdateCodePosition(context); 
        }

        public Expression VisitVariableLeftValue(Variable_lvalueContext variableContext) => throw new NotImplementedException();


        public static string GetIdentifierString(IdentifierContext context)
        {
            // identifier:
            //     C_IDENTIFIER
            //     | SIMPLE_IDENTIFIER
            //     | ESCAPED_IDENTIFIER
            //     | KW_SAMPLE
            //     | KW_RANDOMIZE
            //     | KW_TYPE_OPTION
            //     | KW_OPTION
            //     | KW_STD
            // ;
            var escapedIdentifierContext = context.ESCAPED_IDENTIFIER();
            if (escapedIdentifierContext != null)
            {
                return LiteralParser.VisitEscapedIdentifier(escapedIdentifierContext);  
            }
            else
            {
                return context.GetText();
            }
        }

        public static Expression VisitIdentifier(IdentifierContext context)
        {
            throw new NotImplementedException();
        }

        public Expression VisitIdentifierDotedIndexAtEnd(Identifier_doted_index_at_endContext context)
        {
            var identifier = context.identifier();
            throw new NotImplementedException();    
        }

        public Expression VisitExpression(ExpressionContext expressionContext) => throw new NotImplementedException();
        public List<Expression> VisitParameterValueAssignment(Parameter_value_assignmentContext context) => throw new NotImplementedException();
        internal Expression VisitPackageOrClassScopedPath(Package_or_class_scoped_pathContext pkgClassScoped) => throw new NotImplementedException();
        internal Expression VisitMintypmaxExpression(Mintypmax_expressionContext minTypemaxContext) => throw new NotImplementedException();
        internal Expression VisitIdentifierWithBitSelect(Identifier_with_bit_selectContext idWithBitSelectContext, object value) => throw new NotImplementedException();
    }
}
