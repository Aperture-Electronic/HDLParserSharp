using HDLAbstractSyntaxTree.Elements;
using HDLElaborateRoslyn;
using HDLElaborateRoslyn.Expressions;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SystemVerilog2017Interpreter.Macro;

namespace TestHDLParserSharp
{
    [TestClass]
    public class TestMacroPreprocessor
    {
        [TestMethod]
        public void TestSimpleMacroDefineAndIf()
        {
            Console.WriteLine("TestSimpleMacroDefineAndIf");

            string fileContent =
                @"`define MACRO_WITHOUT_VALUE
                `define MACRO_VALUE 1

                `if `MACRO_VALUE <= 1
                `ifdef MACRO_WITHOUT_VALUE
                module A();
                `else
                module C();
                `endif
                endmodule
                `else
                module B(); 
                endmodule
                `endif";

            HDLElaborator elaborator = new HDLElaborator();
            CodeMacroPreprocessor macroPreprocessor
                = new CodeMacroPreprocessor(delegate (Expression e)
                {
                    string csharpCode = ExpressionToSharp.ToSharp(e);
                    Console.WriteLine(csharpCode);

                    return elaborator.EvalToBool(csharpCode);
                });

            string newFile = macroPreprocessor.VisitMacro(fileContent);
            Console.WriteLine(newFile);
        }
    }
}
