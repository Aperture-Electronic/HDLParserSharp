using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;
using SystemVerilog2017;
using SystemVerilog2017Interpreter.Macro;
using SystemVerilog2017Interpreter.Parsers;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace HDLParserSharp
{
    public class SystemVerilogParserContainer :
        HDLParserContainer<SystemVerilog2017Lexer, SystemVerilog2017Parser, SourceTextParser, CodeMacroPreprocessor>
    {
        public SystemVerilogParserContainer(List<HDLObject> hdlContext, HDLLanguage language, Func<Expression, bool> macroIfParser, string path = ".") 
            : base(hdlContext, language, macroIfParser, path)
        {
        }

        protected override void Parse()
        {
            if ((lexer == null) || (parser == null) || (hdlParser == null))
            {
                return;
            }

            Source_textContext tree = parser.source_text();
            hdlParser.VisitSourceText(tree);
        }
    }
}
