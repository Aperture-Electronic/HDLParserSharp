using Antlr4.Runtime;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Text;
using SystemVerilog2017;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class CommentParser : HDLCommentParser
    {
        private readonly CommonTokenStream tokens;

        public CommentParser(ITokenStream tokens)
        {
            if (tokens is CommonTokenStream commonToken)
            {
                this.tokens = commonToken;
            }
            else
            {
                throw new InvalidCastException("Token stream is not a common token stream");
            }
        }

        /// <summary>
        /// Parse the comment from context
        /// </summary>
        public override string Parse<T>(T context)
        {
            string tempText = string.Empty;
            int lastTokenIndex = context.Start.TokenIndex;

            // Find the first comment in block of comments
            int commentToken = lastTokenIndex;
            for (; commentToken > 0; commentToken--)
            {
                IToken token = tokens.Get(commentToken - 1);
                if (token.Channel != 1)
                {
                    break;
                }
            }

            // Collect the text from the comments
            for (int i = commentToken; i < lastTokenIndex; i++)
            {
                IToken token = tokens.Get(i);
                string text = token.Text;
                if (token.Type == SystemVerilog2017Lexer.WHITE_SPACE)
                {
                    continue;
                }

                text.TrimStart('/');
                tempText += text;
            }

            return tempText;
        }
    }
}
