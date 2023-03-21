using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.HDLElement;
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

        /// <summary>
        /// tf_item_declaration:
        ///     block_item_declaration
        ///     | tf_port_declaration
        /// ;
        /// </summary>
        public void VisitTaskFunctionItemDeclaration(Tf_item_declarationContext context, List<HDLObject> objects, List<IdentifierDefinition> ports)
        {
            var blockItemDeclContext = context.block_item_declaration();
            if (blockItemDeclContext != null )
            {
                // The item may specify the type for port, we need to check it
                // and merge it with te port definitions
                int prevObjectsCount = objects.Count;
                
            }
        }

        public FunctionDefinition VisitFunctionDeclaration(Function_declarationContext context)
        {

            throw new NotImplementedException();
        }
    }
}
