using HDLAbstractSyntaxTree.BasicUnit;
using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using HDLAbstractSyntaxTree.Common;
using HDLParserBase;

namespace SystemVerilog2017Interpreter.Extensions
{
    internal static class ICodePositionExtension
    {
        public static T UpdateCodePosition<T>(this T position, IParseTree parseTree) where T : ICodePosition
        {
            if (parseTree != null)
            {
                var context = parseTree as ParserRuleContext;
                if (context != null)
                {
                    if ((parseTree is ITerminalNode terminalNode) && terminalNode.Parent != null)
                    {
                        context = terminalNode.Parent as ParserRuleContext;
                    }
                }

                if (context != null)
                {
                    position.Line = new ValueRange<int>(context.Start.Line, context.Stop.Line);
                    position.Column = new ValueRange<int>(context.Start.Column, context.Stop.Column);
                }
            }

            return position;
        }

        public static T UpdateDocument<T> (this T document, ParserRuleContext context, BaseCommentParser parser) 
            where T : IDocumented
        {
            if ((context != null) && (parser != null))
            {
                document.Document = parser.Parse(context);
            }

            return document;
        }
    }
}
