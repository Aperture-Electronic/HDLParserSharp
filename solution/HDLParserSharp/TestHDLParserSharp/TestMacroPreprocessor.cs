using HDLAbstractSyntaxTree.Elements;
using HDLElaborateRoslyn.Elaborator;
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

            HDLEvaluator evaluator = new HDLEvaluator();
            CodeMacroPreprocessor macroPreprocessor
                = new CodeMacroPreprocessor(evaluator.EvalToBool);

            string newFile = macroPreprocessor.VisitMacro(fileContent);
            Console.WriteLine(newFile);
        }
    }
}
