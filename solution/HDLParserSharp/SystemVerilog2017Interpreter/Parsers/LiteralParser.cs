using Antlr4.Runtime.Tree;
using HDLAbstractSyntaxTree.Common;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class LiteralParser : HDLParser
    {
        public LiteralParser(HDLParser other) : base(other)
        {

        }

        public static Expression VisitIntegralNumber(Integral_numberContext context)
        {
            // integral_number:
            //    BASED_NUMBER_WITH_SIZE
            //    | ( UNSIGNED_NUMBER )? ANY_BASED_NUMBER
            //    | UNSIGNED_NUMBER
            // ;

            ITerminalNode basedNumberWithSize = context.BASED_NUMBER_WITH_SIZE();
            if (basedNumberWithSize != null)
            {
                string text = basedNumberWithSize.GetText();
                int whereIsApostrophe = text.IndexOf("'");
                if (whereIsApostrophe < 0)
                {
                    throw new Exception("The based number has not a identifier of '(apostrophe)");
                }

                int size = ParseSizeUnsignedNumber(text[0..whereIsApostrophe]);
                string value = text[whereIsApostrophe..];
                return VisitAnyBasedNumber(context, value, size);
            }

            ITerminalNode anyBasedNumber = context.ANY_BASED_NUMBER();
            if (anyBasedNumber != null)
            {
                int size = -1;
                ITerminalNode unsignedNumber = context.UNSIGNED_NUMBER();
                if (unsignedNumber != null)
                {
                    size = ParseSizeUnsignedNumber(unsignedNumber.GetText());
                }

                return VisitAnyBasedNumber(context, anyBasedNumber.GetText(), size);
            }

            return VisitUnsignedNumber(context.UNSIGNED_NUMBER());
        }

        public static int ParseSizeUnsignedNumber(string valueString)
            => valueString.ToNumberInVerilog();

        public static Expression VisitUnsignedNumber(ITerminalNode terminalNode)
        {
            string noSeparator = terminalNode.GetText().WithOutVerilogNumberSeparator();
            return new Integer(noSeparator, Integer.Base.Decimal).UpdateCodePosition(terminalNode);
        }

        public static Expression VisitAnyBasedNumber(Integral_numberContext context, string value, int size)
        {
            string noSeparator = value.WithOutVerilogNumberSeparator();
            if (noSeparator[0] != '\'')
            {
                throw new Exception("Invalid Verilog based number with out an identifier of '(apostrophe)");
            }

            // Signed/unsigned
            int valuePartStart = noSeparator[1] == 's' ? 3 : 2;
            char lowerRadix = char.ToLower(noSeparator[valuePartStart - 1]);

            Integer.Base radix = lowerRadix switch
            {
                'b' => Integer.Base.Binary,
                'o' => Integer.Base.Octal,
                'd' => Integer.Base.Decimal,
                'h' => Integer.Base.Hexadecimal,
                _ => throw new Exception("Invalid Verilog number base")
            };

            string valueString = noSeparator[valuePartStart..];
            return size != -1
                ? new Integer(valueString, size, radix).UpdateCodePosition(context)
                : (Expression)new Integer(valueString, radix).UpdateCodePosition(context);
        }

        public static Expression VisitNumber(NumberContext context)
        {
            Integral_numberContext integralNum = context.integral_number();
            if (integralNum != null)
            {
                return VisitIntegralNumber(integralNum);
            }

            Real_numberContext realNum = context.real_number();
            return realNum != null ? VisitRealNumber(realNum) : throw new Exception("Invalid number is not integral either real");
        }

        public static Expression VisitSimpleIdentifier(ITerminalNode terminalNode)
            => new Identifier(terminalNode.GetText()).UpdateCodePosition(terminalNode);

        public static Expression VisitComplexIdentifier(ITerminalNode terminalNode)
            => VisitSimpleIdentifier(terminalNode);

        public static string VisitEscapedIdentifier(ITerminalNode terminalNode)
            => terminalNode.GetText()[1..];

#warning Time literal is not implemented now
        public static Expression VisitTimeLiteral(ITerminalNode terminalNode)
            => new NotImplemented("Time literal").UpdateCodePosition(terminalNode);

        public static Expression VisitRealNumber(Real_numberContext realNum)
        {
            double value = double.Parse(realNum.GetText());
            return new Real(value).UpdateCodePosition(realNum);
        }

        public static Expression VisitString(ITerminalNode terminalNode)
            => new HDLAbstractSyntaxTree.Value.String(terminalNode.GetText().Trim('"')).UpdateCodePosition(terminalNode);

        public static OperatorType VisitUnaryModulePathOperator(Unary_module_path_operatorContext context)
        {
            // unary_module_path_operator:
            //     NOT
            //     | NEG
            //     | AMPERSAND
            //     | NAND
            //     | BAR
            //     | NOR
            //     | XOR
            //     | NXOR
            //     | XORN
            // ;
            if (context.NOT() != null)
            {
                return OperatorType.NegLog;
            }
            else if (context.NEG() != null)
            {
                return OperatorType.Neg;
            }
            else if (context.AMPERSAND() != null)
            {
                return OperatorType.AndUnary;
            }
            else if (context.NAND() != null)
            {
                return OperatorType.NandUnary;
            }
            else if (context.BAR() != null)
            {
                return OperatorType.OrUnary;
            }
            else if (context.NOR() != null)
            {
                return OperatorType.NorUnary;
            }
            else if (context.XOR() != null)
            {
                return OperatorType.XorUnary;
            }
            else if (context.NXOR() != null)
            {
                return OperatorType.XnorUnary;
            }
            else if (context.XORN() != null)
            {
                return OperatorType.XnorUnary;
            }

            throw new Exception("Expected an unary module path operator");
        }

        public static OperatorType VisitUnaryOperator(Unary_operatorContext context)
        {
            if (context.PLUS() != null)
            {
                return OperatorType.PlusUnary;
            }
            else if (context.MINUS() != null)
            {
                return OperatorType.MinusUnary;
            }
            else
            {
                Unary_module_path_operatorContext unaryModule = context.unary_module_path_operator();
                return unaryModule != null ? VisitUnaryModulePathOperator(unaryModule) :
                    throw new Exception("Expected an unary operator");
            }
        }

        public static OperatorType VisitOperatorMulDivMod(Operator_mul_div_modContext context)
        {
            // operator_mul_div_mod:
            // 	   MUL
            //     | DIV
            //     | MOD
            // ;
            if (context.MUL() != null)
            {
                return OperatorType.Mul;
            }
            else if (context.DIV() != null)
            {
                return OperatorType.Div;
            }
            else if (context.MOD() != null)
            {
                return OperatorType.Mod;
            }

            throw new Exception("Expceted a multiply(*), divide(/) or modulo(%) operator");
        }

        public static OperatorType VisitOperatorPlusMinus(Operator_plus_minusContext context)
        {
            if (context.PLUS() != null)
            {
                return OperatorType.Add;
            }
            else if (context.MINUS() != null)
            {
                return OperatorType.Sub;
            }

            throw new Exception("Expected a plus(+) or minus(-) operator");
        }

        public static OperatorType VisitOperatorShift(Operator_shiftContext context)
        {
            // operator_shift:
            // 	SHIFT_LEFT
            //    | SHIFT_RIGHT
            //    | ARITH_SHIFT_LEFT
            //    | ARITH_SHIFT_RIGHT
            // ;
            if (context.SHIFT_LEFT() != null)
            {
                return OperatorType.Sll;
            }
            else if (context.SHIFT_RIGHT() != null)
            {
                return OperatorType.Srl;
            }
            else if (context.ARITH_SHIFT_LEFT() != null)
            {
                return OperatorType.Sla;
            }
            else if (context.ARITH_SHIFT_RIGHT() != null)
            {
                return OperatorType.Sra;
            }

            throw new Exception("Expected a shift operator");
        }

        public static OperatorType VisitOperatorCompare(Operator_cmpContext context)
        {
            // operator_cmp:
            // 	 LT
            //    | LE
            //    | GT
            //    | GE
            // ;
            if (context.LT() != null)
            {
                return OperatorType.Lt;
            }
            else if (context.LE() != null)
            {
                return OperatorType.Le;
            }
            else if (context.GT() != null)
            {
                return OperatorType.Gt;
            }
            else if (context.GE() != null)
            {
                return OperatorType.Ge;
            }

            throw new Exception("Expected a compare operator");
        }

        public static OperatorType VisitOperatorEquality(Operator_eq_neqContext context)
        {
            // operator_eq_neq:
            // 	    EQ
            //    | NE
            //    | CASE_EQ
            //    | CASE_NE
            //    | WILDCARD_EQ
            //    | WILDCARD_NE
            // ;
            if (context.EQ() != null)
            {
                return OperatorType.Eq;
            }
            else if (context.NE() != null)
            {
                return OperatorType.Ne;
            }
            else if (context.CASE_EQ() != null)
            {
                return OperatorType.Is;
            }
            else if (context.CASE_NE() != null)
            {
                return OperatorType.IsNot;
            }
            else if (context.WILDCARD_EQ() != null)
            {
                return OperatorType.EqMatch;
            }
            else if (context.WILDCARD_NE() != null)
            {
                return OperatorType.NeMatch;
            }

            throw new Exception("Expected a equality check operator");
        }

        public static OperatorType VisitOperatorXor(Operator_xorContext context)
        {
            // operator_xor:
            // 	  XOR
            // 	| NXOR
            // 	| XORN
            // ;
            if (context.XOR() != null)
            {
                return OperatorType.Xor;
            }
            else if (context.NXOR() != null)
            {
                return OperatorType.Xnor;
            }
            else if (context.XORN() != null)
            {
                return OperatorType.Xnor;
            }

            throw new Exception("Expected a xor operator");
        }

#warning Bi-direction arrow is not implemented now
        public static OperatorType VisitOperatorImplement(Operator_implContext context)
        {
            // operator_impl:
            // 	  ARROW
            // 	| BI_DIR_ARROW
            // ;
            if (context.Start.Type == ARROW)
            {
                if (context.ARROW() != null)
                {
                    return OperatorType.Arrow;
                }
                else if (context.BI_DIR_ARROW() != null)
                {
                    return OperatorType.Arrow;
                }
            }

            throw new Exception("Expected a implement operator");
        }

        public static OperatorType VisitOperatorAssign(Operator_assignmentContext context)
            => context.Start.Type switch
            {
                ASSIGN => OperatorType.Assign,
                PLUS_ASSIGN => OperatorType.PlusAssign,
                MINUS_ASSIGN => OperatorType.MinusAssign,
                MUL_ASSIGN => OperatorType.MulAssign,
                DIV_ASSIGN => OperatorType.DivAssign,
                MOD_ASSIGN => OperatorType.ModAssign,
                AND_ASSIGN => OperatorType.AndAssign,
                OR_ASSIGN => OperatorType.OrAssign,
                XOR_ASSIGN => OperatorType.XorAssign,
                SHIFT_LEFT_ASSIGN => OperatorType.ShiftLeftAssign,
                SHIFT_RIGHT_ASSIGN => OperatorType.ShiftRightAssign,
                ARITH_SHIFT_LEFT_ASSIGN => OperatorType.ArithShiftLeftAssign,
                ARITH_SHIFT_RIGHT_ASSIGN => OperatorType.ArithShiftRightAssign,
                _ => throw new Exception("Expected an assign operator")
            };

        public static OperatorType VisitIncreaseDecreaseOperator(Inc_or_dec_operatorContext context, bool prefix)
        {
            if (prefix)
            {
                if (context.INCR() != null)
                {
                    return OperatorType.IncrPre;
                }
                else if (context.DECR() != null)
                {
                    return OperatorType.DecrPre;
                }
            }
            else
            {
                if (context.INCR() != null)
                {
                    return OperatorType.IncrPost;
                }
                else if (context.DECR() != null)
                {
                    return OperatorType.DecrPost;
                }
            }

            throw new Exception("Expected an increase(++)/decrease(--) operator");
        }

        public static Expression VisitPrimaryLiteral(Primary_literalContext context)
        {
            // primary_literal:
            //     TIME_LITERAL
            //     | UNBASED_UNSIZED_LITERAL
            //     | STRING_LITERAL
            //     | number
            //     | KW_NULL
            //     | KW_THIS
            //     | DOLAR
            // ;
            ITerminalNode timeL = context.TIME_LITERAL();
            if (timeL != null)
            {
                return VisitTimeLiteral(timeL);
            }

            ITerminalNode unbasedUnsigned = context.UNBASED_UNSIZED_LITERAL();
            if (unbasedUnsigned != null)
            {
                // UNBASED_UNSIZED_LITERAL:
                //     APOSTROPHE Z_OR_X
                //     | '\'0'
                //     | '\'1'
                // ;
                string text = unbasedUnsigned.GetText();
                return text.Length != 2
                    ? throw new Exception("Invalid unbased unsigned literal")
                    : (Expression)new Integer(text[1..], Integer.Base.Decimal).UpdateCodePosition(context);
            }

            ITerminalNode stringLiteral = context.STRING_LITERAL();
            if (stringLiteral != null)
            {
                return VisitString(stringLiteral);
            }

            NumberContext number = context.number();
            if (number != null)
            {
                return VisitNumber(number);
            }

            if (context.KW_NULL() != null)
            {
                return new Symbol(SymbolType.Null).UpdateCodePosition(context);
            }
            else if (context.KW_THIS() != null)
            {
                return new Identifier("this").UpdateCodePosition(context);
            }
            else if (context.DOLAR() != null)
            {
                return new Identifier("$").UpdateCodePosition(context);
            }

            throw new Exception("Unexpected primary literal");
        }

        /// <summary>
        /// any_system_tf_identifier:
        ///     SYSTEM_TF_IDENTIFIER
        ///     | KW_DOLAR_SETUPHOLD
        ///     | KW_DOLAR_SETUP
        ///     | KW_DOLAR_FULLSKEW
        ///     | KW_DOLAR_WARNING
        ///     | KW_DOLAR_WIDTH
        ///     | KW_DOLAR_ROOT
        ///     | KW_DOLAR_RECOVERY
        ///     | KW_DOLAR_SKEW
        ///     | KW_DOLAR_FATAL
        ///     | KW_DOLAR_REMOVAL
        ///     | KW_DOLAR_RECREM
        ///     | KW_DOLAR_ERROR
        ///     | KW_DOLAR_PERIOD
        ///     | KW_DOLAR_HOLD
        ///     | KW_DOLAR_INFO
        ///     | KW_DOLAR_UNIT
        ///     | KW_DOLAR_TIMESKEW
        ///     | KW_DOLAR_NOCHANGE
        /// ;
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Expression VisitAnySystemTaskFunctionIdentifier(Any_system_tf_identifierContext context)
            => new Identifier(context.GetText()).UpdateCodePosition(context);

        /// <summary>
        /// assignment_operator:
        ///    ASSIGN
        ///    | PLUS_ASSIGN
        ///    | MINUS_ASSIGN
        ///    | MUL_ASSIGN
        ///    | DIV_ASSIGN
        ///    | MOD_ASSIGN
        ///    | AND_ASSIGN
        ///    | OR_ASSIGN
        ///    | XOR_ASSIGN
        ///    | SHIFT_LEFT_ASSIGN
        ///    | SHIFT_RIGHT_ASSIGN
        ///    | ARITH_SHIFT_LEFT_ASSIGN
        ///    | ARITH_SHIFT_RIGHT_ASSIGN
        /// ;
        /// </summary>
        public static OperatorType VisitAssignmentOperator(Assignment_operatorContext context)
            => context.Start.Type switch
            {
                ASSIGN => OperatorType.Assign,
                PLUS_ASSIGN => OperatorType.PlusAssign,
                MINUS_ASSIGN => OperatorType.MinusAssign,
                MUL_ASSIGN => OperatorType.MulAssign,
	            DIV_ASSIGN => OperatorType.DivAssign,
	            MOD_ASSIGN => OperatorType.ModAssign,
	            AND_ASSIGN => OperatorType.AndAssign,
	            OR_ASSIGN => OperatorType.OrAssign,
	            XOR_ASSIGN => OperatorType.XorAssign,
	            SHIFT_LEFT_ASSIGN => OperatorType.ShiftLeftAssign,
	            SHIFT_RIGHT_ASSIGN => OperatorType.ShiftRightAssign,
	            ARITH_SHIFT_LEFT_ASSIGN => OperatorType.ArithShiftLeftAssign,
	            ARITH_SHIFT_RIGHT_ASSIGN => OperatorType.ArithShiftRightAssign,
                _ => throw new Exception("Invalid assignment operator")
            };
    }
}
