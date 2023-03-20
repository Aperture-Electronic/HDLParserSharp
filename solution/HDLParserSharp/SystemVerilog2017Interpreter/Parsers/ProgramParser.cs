using HDLAbstractSyntaxTree.Definition;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemVerilog2017;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class ProgramParser : HDLParser
    {
        public ProgramParser(HDLParser other) : base(other) { }

        public FunctionDefinition VisitFunctionDeclaration(Function_declarationContext context)
        {

            throw new NotImplementedException();
        }
    }
}
