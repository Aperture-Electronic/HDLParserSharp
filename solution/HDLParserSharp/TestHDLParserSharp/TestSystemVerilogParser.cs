using HDLAbstractSyntaxTree.HDLElement;
using HDLParserSharp;

namespace TestHDLParserSharp
{
    [TestClass]
    public class TestSystemVerilogParser
    {
        [TestMethod]
        public void TestSimpleModule()
        {
            Console.WriteLine("Test simple System Verilog module parse");

            string fileContent = @"
                // System Verilog Source file
                // Module: A
                // Design for test

                module A
                (
                    input logic clk, 
                    output logic clk_out
                ); 

                endmodule";

            List<HDLObject> ast = new List<HDLObject>();
            SystemVerilogParserContainer systemVerilogParser = new SystemVerilogParserContainer(ast, HDLLanguage.SystemVerilg);
            systemVerilogParser.ParseString(fileContent, true);


        }
    }
}