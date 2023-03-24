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

                `define MACRO_TEST

                module A
                #(
                    parameter DATA_WIDTH = 20
                )
                (
                    output        logic [1:0] dout    
                ); 

                endmodule";

            List<HDLObject> ast = new List<HDLObject>();
            SystemVerilogParserContainer systemVerilogParser = new SystemVerilogParserContainer(ast, HDLLanguage.SystemVerilog);
            systemVerilogParser.ParseString(fileContent, true);

            Assert.IsTrue(ast[0] is ModuleDeclaration);
            ModuleDeclaration moduleDefine = (ModuleDeclaration)ast.First();
            var ports = moduleDefine.Ports;
            Assert.IsTrue(ports.Any());

            foreach (var port in ports)
            {
                if (port is IdentifierDefinition identifier)
                {
                    Assert.IsNotNull(identifier.Type);
                    Assert.IsTrue(identifier.Type is Operator);
                    Operator type = (Operator)identifier.Type;
                    var dim = type[1];
                    Assert.IsNotNull(dim);
                    if (dim is Symbol nullSymbol)
                    {
                        // Single bit
                        Console.WriteLine("The port is single bit");
                    }
                    else 
                    {
                        Assert.IsTrue(dim is Operator);
                        var dimOp = (Operator)dim;
                        if (dimOp.Type == OperatorType.MultiDimension)
                        {
                            // Unpacked and packed
                            Console.WriteLine("The port has both packed and unpacked type");
                        }
                        else if (dimOp.Type == OperatorType.Downto)
                        {
                            // [A:B]
                            Console.WriteLine("The port has a 1D type");
                            Expression msb = dimOp[0];
                            Expression lsb = dimOp[1];

                            string msbString = ExpressionToSharp.ToSharp(msb);
                            string lsbString = ExpressionToSharp.ToSharp(lsb);

                            Console.WriteLine($"MSB: {msbString}");
                            Console.WriteLine($"LSB: {lsbString}");

                            HDLElaborator elaborator = new HDLElaborator();
                            int msbBit = elaborator.EvalToInteger(msbString);
                            int lsbBit = elaborator.EvalToInteger(lsbString); 

                            Console.WriteLine($"MSB: {msbBit}");
                            Console.WriteLine($"LSB: {lsbBit}");
                        }
                    }
                }
            }
        }
    }
}