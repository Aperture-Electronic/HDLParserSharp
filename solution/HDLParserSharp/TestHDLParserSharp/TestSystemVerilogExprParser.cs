using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLElaborateRoslyn;
using HDLElaborateRoslyn.Elaborator;
using HDLElaborateRoslyn.Expressions;
using HDLParserSharp;

namespace TestHDLParserSharp
{
    [TestClass]
    public class TestSystemVerilogExprParser
    {
        [TestMethod]
        public void TestSimpleExpr()
        {
            Console.WriteLine("Test simple System Verilog module parse");

            string fileContent = @"module X(); assign B = A > 0; endmodule";

            List<HDLObject> ast = new List<HDLObject>();
            HDLEvaluator evaluator = new HDLEvaluator();
            SystemVerilogParserContainer systemVerilogParser = new SystemVerilogParserContainer(ast, HDLLanguage.SystemVerilog, evaluator.EvalToBool);
            systemVerilogParser.ParseString(fileContent, true);
           
        }
    }
}