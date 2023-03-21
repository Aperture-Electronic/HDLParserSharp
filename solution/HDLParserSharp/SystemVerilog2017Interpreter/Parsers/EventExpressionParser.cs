using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Types;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class EventExpressionParser : HDLParser
    {
        public EventExpressionParser(HDLParser other) : base(other) { }

        /// <summary>
        /// event_expression: event_expression_item ( ( KW_OR | COMMA ) event_expression_item )*;
        /// </summary>
        /// <remarks>'or' and ',' is a Verilog-1995/2001 difference, they work exactly the same</remarks>
        public IEnumerable<Expression> VisitEventExpression(Event_expressionContext context)
            => context.event_expression_item().SelectMany(ep => VisitEventExpressionItem(ep));

        /// <summary>
        /// event_expression_item:
        ///     LPAREN event_expression RPAREN
        ///     | ( edge_identifier )? expression ( KW_IFF expression )?
        /// ;
        private IEnumerable<Expression> VisitEventExpressionItem(Event_expression_itemContext context)
        {
            var eventExprContext = context.event_expression();
            if (eventExprContext != null)
            {
                var eventExpressions = VisitEventExpression(eventExprContext);
                foreach (var e in eventExpressions)
                {
                    yield return e;
                }

                yield break;
            }
            
            var expressionContext = context.expression();
            ExpressionParser expressionParser = new ExpressionParser(this);
            Expression expression = expressionParser.VisitExpression(expressionContext.First());
            var edgeIdentifierContext = context.edge_identifier();
            if (edgeIdentifierContext != null)
            {
#warning Edge type identifier is not fully implemented
                (OperatorType type, bool explicited) edgeType = VisitEventIdentifier(edgeIdentifierContext);
                if (edgeType.explicited)
                {
                    expression = new Operator(edgeType.type, expression).UpdateCodePosition(expressionContext.First());
                }

                if (expressionContext.Length != 1)
                {
#warning Multiple event expression item is not implemented now
                }

                yield return expression;
            }
        }

        /// <summary>
        /// edge_identifier:
        ///    KW_POSEDGE
        ///    | KW_NEGEDGE
        ///    | KW_EDGE
        /// ;
        /// </summary>
        private (OperatorType type, bool explicited) VisitEventIdentifier(Edge_identifierContext context)
        {
            if (context.KW_POSEDGE() != null)
            {
                return (OperatorType.Rising, true);
            }
            else if (context.KW_NEGEDGE() != null)
            {
                return (OperatorType.Falling, true);
            }
            else if (context.KW_EDGE() != null)
            {
                return (OperatorType.Rising, false);
            }

            throw new Exception("Invalid event edge identifier");
        }
    }
}
