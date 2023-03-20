using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.HDLElement;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Text;
using SystemVerilog2017;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class GenerateParser : HDLParser
    {
        public GenerateParser(HDLParser other) : base(other) { }

        internal void VisitGenerateRegion(Generate_regionContext context, List<HDLObject> objects)
        {
            // generate_region: KW_GENERATE ( generate_item )* KW_ENDGENERATE;
            List<HDLObject> generateItems = new List<HDLObject>();
            foreach (var generateItemContext in context.generate_item())
            {
                VisitGenerateItem(generateItemContext, generateItems);
                foreach (var item in generateItems)
                {
                    if (item is HDLStatement statement)
                    {
                        statement.IsInPreprocess = true;
                    }

                    objects.Add(item);
                }

                generateItems.Clear();
            }
        }

        public void VisitModuleOrGenerateOrInterfaceOrCheckerItem(Module_or_generate_or_interface_or_checker_itemContext context,
            List<HDLObject> objects)
        {
            // module_or_generate_or_interface_or_checker_item:
            //     function_declaration
            //     | checker_declaration
            //     | property_declaration
            //     | sequence_declaration
            //     | let_declaration
            //     | covergroup_declaration
            //     | genvar_declaration
            //     | clocking_declaration
            //     | initial_construct
            //     | always_construct
            //     | final_construct
            //     | assertion_item
            //     | continuous_assign
            // ;

            var funcDeclarationContext = context.function_declaration();
            if (funcDeclarationContext != null)
            {
                ProgramParser programParser = new ProgramParser(this);
                FunctionDefinition funcDefinition = programParser.VisitFunctionDeclaration(funcDeclarationContext);
                objects.Add(funcDefinition);
                return;
            }

            if (context.checker_declaration() != null)
            {
#warning Checker declaration is not implemented now
                return;
            }

            if (context.covergroup_declaration() != null)
            {
#warning Cover group declaration is not implemented now
                return;
            }

            if (context.property_declaration() != null)
            {
#warning Property declaration is not implemented now
                return;
            }

            if (context.sequence_declaration() != null)
            {
#warning Sequence declaration is not implemented now
                return;
            }

            if (context.let_declaration() != null)
            {
#warning Let declaration is not implemented now
                return;
            }

            var genvarDeclarationContext = context.genvar_declaration();
            if (genvarDeclarationContext != null)
            {
                VisitGenvarDeclaration(genvarDeclarationContext, objects);
                return;
            }

            if (context.clocking_declaration() != null)
            {
#warning Clocking declaration is not implemented now
                return;
            }

            if (context.assertion_item() != null)
            {
#warning Assertion item is not implemented now
            }

            var contAssingContext = context.continuous_assign();
            if (contAssingContext != null)
            {
                // TODO
            }
        }

        private void VisitGenvarDeclaration(Genvar_declarationContext genvarDeclarationContext, List<HDLObject> objects) => throw new NotImplementedException();

        public void VisitModuleOrGenerateItem(Module_or_generate_itemContext context, List<HDLObject> objects, IEnumerable<IdentifierDefinition> parameters)
        {
            
        }

        private void VisitGenerateItem(Generate_itemContext generateItemContext, List<HDLObject> generateItems) => throw new NotImplementedException();
    }
}
