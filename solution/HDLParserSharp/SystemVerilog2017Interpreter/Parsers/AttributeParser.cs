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

        public static void VisitAttributeInstance(IEnumerable<Attribute_instanceContext> contexts)
        {
            foreach (var context in contexts)
            {
                VisitAttributeInstance(context);
            }
        }

        public static void VisitAttributeInstance(Attribute_instanceContext context)
        {
#warning Attribute is not implemented now
        }
    }
}
