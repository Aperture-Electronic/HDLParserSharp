using Antlr4.Runtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLParserBase
{
    public abstract class HDLCommentParser
    {
        public abstract string Parse<T>(T context) where T : ParserRuleContext;
    }
}
