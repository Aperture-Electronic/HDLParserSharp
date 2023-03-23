using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Text;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class DelayParser : HDLParser
    {
        public DelayParser(HDLParser other) : base(other) { }

        /// <summary>
        /// event_control:
        ///    AT ( LPAREN ( MUL
        ///                 | event_expression
        ///                 ) RPAREN
        ///         | MUL
        ///         | package_or_class_scoped_hier_id_with_select
        ///       );
        /// </summary>
        public IEnumerable<Expression> VisitEventControl(Event_controlContext context)
        {
            if (context.MUL() != null)
            {
                yield return SymbolType.All.AsNewSymbol();
                yield break;
            }

            var pkgClassScopedHierIdWithSelContext = context.package_or_class_scoped_hier_id_with_select();
            if (pkgClassScopedHierIdWithSelContext != null) 
            {
                ExpressionParser expressionParser = new ExpressionParser(this);
                yield return expressionParser.VisitPackageOrClassScopedHierIdWithSelect(pkgClassScopedHierIdWithSelContext);
                yield break;
            }

            var eventExprContext = context.event_expression();
            if (eventExprContext != null)
            {
                EventExpressionParser eventExpressionParser = new EventExpressionParser(this);
                IEnumerable<Expression> eventExpressions = eventExpressionParser.VisitEventExpression(eventExprContext);

                foreach (Expression expression in eventExpressions)
                {
                    yield return expression;
                }
            }

            throw new Exception("Excepted event control statement");
        }

        /// <summary>
        /// procedural_timing_control:
        ///    delay_control
        ///     | event_control
        ///     | cycle_delay
        ///     | cycle_delay_range
        /// ;
        /// </summary>
        public (Expression?, IEnumerable<Expression>?) VisitProceduralTimingControl(Procedural_timing_controlContext context)
        {
            var delayControlContext = context.delay_control();
            if (delayControlContext != null)
            {
                Expression delayControl = VisitDelayControl(delayControlContext);
                return (delayControl, null);
            }

            var eventControlContext = context.event_control();
            if (eventControlContext != null)
            {
                IEnumerable<Expression> eventControl = VisitEventControl(eventControlContext);
                return (null, eventControl);
            }

            if (context.cycle_delay() != null)
            {
#warning Cycle delay is not implemented now
                return (new NotImplemented("Cycle delay").UpdateCodePosition(context), null);
            }

            if (context.cycle_delay_range() != null)
            {
#warning Range cycle delay is not implemented now
                return (new NotImplemented("Range cycle delay").UpdateCodePosition(context), null);
            }

            throw new Exception("Excepted an procedural timing control");
        }

        /// <summary>
        /// delay_control:
        ///     HASH ( LPAREN mintypmax_expression RPAREN
        ///            | delay_value
        ///          );
        /// </summary>
        private Expression VisitDelayControl(Delay_controlContext context)
        {
            var delayValueContext = context.delay_value();
            if (delayValueContext != null)
            {
                return VisitDelayValue(delayValueContext);
            }
            else
            {
                var mintypmaxExprContext = context.mintypmax_expression();
                return new ExpressionParser(this).VisitMintypmaxExpression(mintypmaxExprContext);
            }
        }

        /// <summary>
        /// delay_value:
        ///     UNSIGNED_NUMBER
        ///     | TIME_LITERAL
        ///     | KW_1STEP
        ///     | real_number
        ///     | ps_identifier;
        /// </summary>
        private Expression VisitDelayValue(Delay_valueContext context)
        {
            var unsignNumberContext = context.UNSIGNED_NUMBER();
            if (unsignNumberContext != null)
            {
                return LiteralParser.VisitUnsignedNumber(unsignNumberContext);
            }

            var timeLiteral = context.TIME_LITERAL();
            if (timeLiteral != null)
            {
                return LiteralParser.VisitTimeLiteral(timeLiteral); 
            }

            if (context.KW_1STEP() != null)
            {
                return new Identifier("1step").UpdateCodePosition(context);
            }

            var realNumberContext = context.real_number();
            if (realNumberContext != null)
            {
                return LiteralParser.VisitRealNumber(realNumberContext);
            }

            var psIdentifierContext = context.ps_identifier();
            if (psIdentifierContext != null)
            {
                return ExpressionParser.VisitPackageScopeIdentifier(psIdentifierContext);
            }

            throw new Exception("Excepted a delay value");
        }

        /// <summary>
        /// delay_or_event_control:
        ///     delay_control
        ///     | ( KW_REPEAT LPAREN expression RPAREN )? event_control
        /// ;
        public (Expression?, IEnumerable<Expression>?) VisitDelayOrEventControl(Delay_or_event_controlContext context)
        {
            var delayControlContext = context.delay_control();
            if (delayControlContext != null)
            {
                var delayControl = VisitDelayControl(delayControlContext);
                return (delayControl, null);
            }

            var eventControlContext = context.event_control();
            var expressionContext = context.expression();

            if (expressionContext != null)
            {
#warning Repeat delay or event control is not implemented now
            }

            return (null, VisitEventControl(eventControlContext));
        }
    }
}
