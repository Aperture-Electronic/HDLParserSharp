using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLElaborateRoslyn;
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
            SystemVerilogParserContainer systemVerilogParser = new SystemVerilogParserContainer(ast, HDLLanguage.SystemVerilog);
            systemVerilogParser.ParseString(fileContent, true);
           
        }
    }
}