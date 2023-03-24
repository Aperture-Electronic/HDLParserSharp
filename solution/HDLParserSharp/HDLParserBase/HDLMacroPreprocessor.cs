using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLParserBase
{
    public abstract class HDLMacroPreprocessor
    {
        public string ContextFilePath { get; set; }

        public List<HDLMacro> MacroContext { get; }

        public Func<Expression, bool> MacroIfParser { get; }

        public HDLMacroPreprocessor(Func<Expression, bool> macroIfParser, string path = ".")
        {
            ContextFilePath = path;
            MacroContext = new List<HDLMacro>();
            MacroIfParser = macroIfParser;
        }

        public abstract string VisitMacro(string code);
    }
}
