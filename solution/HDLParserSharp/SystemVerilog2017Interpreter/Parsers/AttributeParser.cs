using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Value;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Text;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class AttributeParser : HDLParser
    {
        public AttributeParser(HDLParser other) : base(other)
        {

        }

        public Dictionary<Identifier, Expression> VisitAttributeInstance(IEnumerable<Attribute_instanceContext> contexts)
        {
            Dictionary<Identifier, Expression> attributes = new Dictionary<Identifier, Expression>();
            foreach (var context in contexts)
            {
                (Identifier? identifier, Expression? expression) = VisitAttributeInstance(context);
                if ((identifier != null) && (expression != null))
                {
                    attributes.Add(identifier, expression);
                }
            }

            return attributes;
        }

        public (Identifier?, Expression?) VisitAttributeInstance(Attribute_instanceContext context)
        {
#warning Attribute is not implemented now
            ExpressionParser expressionParser = new ExpressionParser(this);
            var attrSpecContext = context.attr_spec();
            if ((attrSpecContext != null) && (attrSpecContext.Length == 1))
            {
                var attrFirstSpec = attrSpecContext[0];
                var idExpr = ExpressionParser.VisitIdentifier(attrFirstSpec.identifier());
                var expression = expressionParser.VisitExpression(attrFirstSpec.expression());

                if (idExpr is Identifier identifier)
                {
                    return (identifier, expression);
                }
            }

            return (null, null);
        }
    }
}
