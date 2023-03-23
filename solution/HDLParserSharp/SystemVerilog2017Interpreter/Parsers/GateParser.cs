using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Statement;
using HDLAbstractSyntaxTree.Types;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemVerilog2017;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class GateParser : HDLParser
    {
        public GateParser(HDLParser other) : base(other) { }

        /// <summary>
        /// enable_terminal: expression;
        /// </summary>
        private Expression VisitEnableTerminal(Enable_terminalContext context)
            => new ExpressionParser(this).VisitExpression(context.expression());

        /// <summary>
        /// inout_terminal: net_lvalue;
        /// </summary>
        private Expression VisitInoutTerminal(Inout_terminalContext context)
            => new ExpressionParser(this).VisitNetLeftValue(context.net_lvalue());

        /// <summary>
        /// input_terminal: expression;
        /// </summary>
        private Expression VisitInputTerminal(Input_terminalContext context)
            => new ExpressionParser(this).VisitExpression(context.expression());

        /// <summary>
        /// output_terminal: net_lvalue;
        /// </summary>
        private Expression VisitOutputTerminal(Output_terminalContext context)
            => new ExpressionParser(this).VisitNetLeftValue(context.net_lvalue());

        /// <summary>
        /// n_output_gatetype:
        ///  KW_BUF
        ///   | KW_NOT
        /// ;
        /// </summary>
        private OperatorType VisitNOutputGateType(N_output_gatetypeContext context)
            => false switch
            {
                _ when (context.KW_BUF() != null) => OperatorType.Assign,
                _ when (context.KW_NOT() != null) => OperatorType.Neg,
                _ => throw new Exception("Excepted a valid N output gate type")
            };

        /// <summary>
        /// n_output_gate_instance:
        ///  ( name_of_instance )? LPAREN output_terminal ( COMMA output_terminal )* COMMA input_terminal
        ///       RPAREN;
        /// </summary>
        private (Expression, Expression) VisitNOutputGateInstance(N_output_gate_instanceContext context)
        {
            var nameOfInstanceContext = context.name_of_instance();
            if (nameOfInstanceContext != null)
            {
                // name_of_instance: identifier ( unpacked_dimension )*;
                // auto name = VerExprParser::visitIdentifier(noi->identifier());
#warning Name of N output gate instance is not implemented now
            }

            IEnumerable<Expression> outputs = context.output_terminal().Select((t) => VisitOutputTerminal(t));
            
            var inputTerminalContext = context.input_terminal();
            Expression inputTerminal = VisitInputTerminal(inputTerminalContext);

            if (outputs.Count() > 1)
            {
                Expression rOutputTerminal = Operator.Reduce(outputs, OperatorType.Concat);
                return (rOutputTerminal, inputTerminal);
            }
            else
            {
                return (outputs.First(), inputTerminal);
            }
        }

        /// <summary>
        /// gate_instantiation:
        ///  ( ( KW_PULLDOWN ( pulldown_strength )?
        ///       | KW_PULLUP ( pullup_strength )?
        ///       ) pull_gate_instance ( COMMA pull_gate_instance )*
        ///   | ( cmos_switchtype | mos_switchtype) ( delay3 )? enable_gate_or_mos_switch_or_cmos_switch_instance ( COMMA enable_gate_or_mos_switch_or_cmos_switch_instance )*
        ///   | enable_gatetype ( drive_strength )? ( delay3 )? enable_gate_or_mos_switch_or_cmos_switch_instance ( COMMA enable_gate_or_mos_switch_or_cmos_switch_instance )*
        ///   | n_input_gatetype ( drive_strength )? ( delay2 )? n_input_gate_instance ( COMMA  n_input_gate_instance )*
        ///   | n_output_gatetype ( drive_strength )? ( delay2 )? n_output_gate_instance ( COMMA n_output_gate_instance )*
        ///   | pass_en_switchtype ( delay2 )? pass_enable_switch_instance ( COMMA pass_enable_switch_instance )*
        ///   | pass_switchtype pass_switch_instance ( COMMA pass_switch_instance )*
        ///   ) SEMI
        /// ;
        /// </summary>
        public void VisitGateInstantiation(Gate_instantiationContext context, List<HDLObject> objects)
        {
            var nOutputGateTypeContext = context.n_output_gatetype();
            if (nOutputGateTypeContext != null)
            {
                OperatorType outputGateType = VisitNOutputGateType(nOutputGateTypeContext);
                var driveStrengthContext = context.drive_strength();
                if (driveStrengthContext != null)
                {
#warning Drive strength is not implemented now
                }

                var delay2Context = context.delay2();
                if (delay2Context != null)
                {
#warning Delay2 sentence is not implemented now
                }

                foreach (var outputGateContext in context.n_output_gate_instance())
                {
                    (Expression output, Expression input) = VisitNOutputGateInstance(outputGateContext);
                    AssignStatement assignStatement = outputGateType == OperatorType.Assign ?
                        new AssignStatement(output, input, false).UpdateCodePosition(outputGateContext) :
                        new AssignStatement(output, 
                                            new Operator(OperatorType.Neg, input).UpdateCodePosition(outputGateContext),
                                            false).UpdateCodePosition(outputGateContext);

                    objects.Add(assignStatement);
                }     
            }
            else
            {
#warning Gate type (expect output gate) are not implemented now
            }
        }
    }
}
