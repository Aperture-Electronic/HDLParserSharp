using Antlr4.Runtime;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.IO;

namespace HDLParserSharp
{
    public abstract class HDLParserContainer<TAntlrLexer, TAntlrParser, THDLParser, THDLMacroPreprocessor>
        where TAntlrLexer : Lexer
        where TAntlrParser : Parser
        where THDLParser : HDLParser
        where THDLMacroPreprocessor : HDLMacroPreprocessor
    {
        protected TAntlrLexer? lexer;
        protected TAntlrParser? parser;
        protected CommonTokenStream? tokens;
        protected THDLParser? hdlParser;
        protected THDLMacroPreprocessor? macroPreprocessor;
        public List<HDLObject> Context { get; }
        public HDLLanguage Language { get; }

        private void Initialize(AntlrInputStream inputStream)
        {
            lexer = (TAntlrLexer)Activator.CreateInstance(typeof(TAntlrLexer), new object[] { inputStream });
            tokens = new CommonTokenStream(lexer);
            parser = (TAntlrParser)Activator.CreateInstance(typeof(TAntlrParser), new object[] { tokens });

            // TODO: Error listener
            parser.RemoveErrorListeners();
            lexer.RemoveErrorListeners();
        }

        public HDLParserContainer(List<HDLObject> hdlContext, HDLLanguage language, Func<Expression, bool> macroIfParser, string path = ".") 
        {
            Language = language;
            Context = hdlContext;

            macroPreprocessor = (THDLMacroPreprocessor)Activator.CreateInstance(typeof(THDLMacroPreprocessor),
                new object[] { macroIfParser, path });
        }

        public void ParseFile(string fileName, bool hierarchyOnly = true)
        {
            string fileContent = File.ReadAllText(fileName);

            // Pre-process
            if (macroPreprocessor != null)
            {
                fileContent = macroPreprocessor.VisitMacro(fileContent);
            }

            var inputStream = new AntlrInputStream(fileContent);
            inputStream.name = fileName;

            // Parse
            Parse(inputStream, hierarchyOnly);
        }

        public void ParseString(string codeString, bool hierarchyOnly = true)
        {
            // Pre-process
            if (macroPreprocessor != null)
            {
                codeString = macroPreprocessor.VisitMacro(codeString);
            }

            var inputStream = new AntlrInputStream(codeString);
            inputStream.name = nameof(codeString);

            Parse(inputStream, hierarchyOnly);
        }

        protected virtual void Parse()
        {
            // Do nothing
        }

        private void Parse(AntlrInputStream stream, bool hierarchyOnly = true)
        {
            Initialize(stream);

            if (parser == null)
            {
                throw new NullReferenceException("Parser has not been initialized");
            }

            ITokenStream tokenStream = parser.TokenStream;
            hdlParser = (THDLParser)Activator.CreateInstance(typeof(THDLParser), new object[] { tokenStream, Context, hierarchyOnly });
            Parse();            
        }
    }
}
