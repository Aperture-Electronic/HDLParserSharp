using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLElaborateRoslyn;
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
                #(
                    parameter DATA_WIDTH = 20
                )
                (
                    // input         logic                                clk, 
                    // output        logic [1:0]                          dout    [0:(DATA_WIDTH > 1 ? (DATA_WIDTH - 1) : 0)]
                    output logic signed [((DATA_WIDTH > 1) ? 1 : 0):0] dout_c
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
                        // 1
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

                            string msbString = HDLElaborateRoslyn.ExpressionToSharp.ToSharp(msb);
                            string lsbString = HDLElaborateRoslyn.ExpressionToSharp.ToSharp(lsb);

                            Console.WriteLine($"MSB: {msbString}");
                            Console.WriteLine($"LSB: {lsbString}");

                            HDLElaborator elaborator = new HDLElaborator();
                            int msbBit = elaborator.EvalToInteger(msbString);
                            int lsbBit = elaborator.EvalToInteger(lsbString); 

                            Console.WriteLine($"MSB: {msbBit}");
                            Console.WriteLine($"LSB: {lsbBit}");
                        }
                        else
                        {
                            // Single bit
                            Console.WriteLine("The port is single bit");
                        }
                    }
                }
            }
        }
    }
}