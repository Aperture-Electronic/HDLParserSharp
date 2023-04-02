using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLElaborateRoslyn.Elaborator;
using HDLElaborateRoslyn.Expressions;
using HDLParserSharp;

namespace TestHDLParserSharp
{
    [TestClass]
    public class TestSystemVerilogParser
    {
        const string fileContent = @"
                // System Verilog Source file
                // Module: A
                // Design for test

                `define MACRO_TEST

                module A
                #(
                    parameter DATA_WIDTH = 20,
                    parameter ISSUE = 128'hFFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF_FFFF
                )
                (
                    input  logic                    clk,
                    output logic [1:0]              dout,
                    output logic [DATA_WIDTH - 1:0] dk    [((ISSUE > 1) ? (DATA_WIDTH - 1) : 0) : 0]
                ); 

                endmodule";

        [TestMethod]
        public void TestSimpleModule()
        {
            Console.WriteLine("Test simple System Verilog module parse");

            List<HDLObject> ast = new List<HDLObject>();
            HDLEvaluator evaluator = new HDLEvaluator();
            SystemVerilogParserContainer systemVerilogParser = new SystemVerilogParserContainer(ast, HDLLanguage.SystemVerilog, evaluator.EvalToBool);
            systemVerilogParser.ParseString(fileContent, true);

            Assert.IsTrue(ast[0] is ModuleDefinition);
            ModuleDefinition moduleDefine = (ModuleDefinition)ast.First();
            Assert.IsNotNull(moduleDefine.Entity);
            ModuleDeclaration entity = moduleDefine.Entity;
            var ports = entity.Ports;
            Assert.IsTrue(ports.Any());

            foreach (var port in ports)
            {
                if (port is IdentifierDefinition identifier)
                {
                    Console.WriteLine($"Port {identifier.Name}");
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

                            int msbBit = evaluator.EvalToInteger(msb);
                            int lsbBit = evaluator.EvalToInteger(lsb); 

                            Console.WriteLine($"MSB: {msbBit}");
                            Console.WriteLine($"LSB: {lsbBit}");
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestElaborateModule()
        {
            

            List<HDLObject> ast = new List<HDLObject>();
            HDLEvaluator evaluator = new HDLEvaluator();
            SystemVerilogParserContainer systemVerilogParser = new SystemVerilogParserContainer(ast, HDLLanguage.SystemVerilog, evaluator.EvalToBool);
            // systemVerilogParser.ParseString(fileContent, true);
            systemVerilogParser.ParseFile("D:/Project/IPCores/Data/SerialDataRepeater/src/data_buffer.sv", true);
            HDLElaborator elaborator = new HDLElaborator(ast);

            elaborator.ElaborateModules();
            elaborator.GenerateModuleGenericsList();
            elaborator.ElaborateModuleGenerics();

            Console.WriteLine("Elaborated module");
            foreach (var module in elaborator.Modules)
            {
                Console.WriteLine($"Module {module.Name} has");
                var generics = module.ElaboratedModuleGenerics;
                Console.WriteLine("Generics:");
                foreach (var generic in generics)
                {
                    Console.WriteLine($"{generic.Name,20}: {generic.DefaultValue,10} ({generic.DefaultValueType})");
                }
                elaborator.ElaborateModulePort();

                Console.WriteLine("Ports:");
                foreach (var port in module.ElaboratedModulePorts)
                {
                    Console.WriteLine($"{port.Name,20}: {port.Size}");
                }
            }
            
        }
    }
}