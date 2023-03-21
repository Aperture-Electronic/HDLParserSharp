using Antlr4.Runtime;
using HDLAbstractSyntaxTree.HDLElement;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Text;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class SourceTextParser : HDLParser
    {
        public SourceTextParser(ITokenStream stream, List<HDLObject> context, bool hierarchyOnly) : base(stream, context, hierarchyOnly)
        {
        }

        public void VisitSourceText(Source_textContext context)
        {
            if (!HierarchyOnly)
            {
                var tuDeclarationContext = context.timeunits_declaration();
                if (tuDeclarationContext != null)
                {
                    VisitTimeUnitsDeclaration(tuDeclarationContext);
                }
            }

            foreach (var descContext in context.description())
            {
                VisitDescription(descContext);
            }
        }

        private void VisitTimeUnitsDeclaration(Timeunits_declarationContext tuDeclarationContext)
        {
            // timeunits_declaration:
            //  KW_TIMEUNIT TIME_LITERAL ( ( DIV
            //                               | SEMI KW_TIMEPRECISION
            //                               ) TIME_LITERAL )? SEMI
            //   | KW_TIMEPRECISION TIME_LITERAL SEMI ( KW_TIMEUNIT TIME_LITERAL SEMI )?
            //  ;
#warning Time units declaration is not implemented now
        }


        private void VisitDescription(DescriptionContext context)
        {
            // description:
            //    module_declaration
            //    | udp_declaration
            //    | interface_declaration
            //    | program_declaration
            //    | package_declaration
            //    | ( attribute_instance )* ( package_item | bind_directive )
            //    | config_declaration
            // ;

            if (Tokens == null)
            {
                throw new NullReferenceException("Tokens stream is null");
            }

            if (HDLContext == null)
            {
                throw new NullReferenceException("HDL context list is null");
            }

            CommentParser commentParser = new CommentParser(Tokens);
            ModuleParser moduleParser = new ModuleParser(commentParser, HierarchyOnly);
            var moduleDeclarationContext = context.module_declaration();
            if (moduleDeclarationContext != null)
            {
                moduleParser.VisitModuleDeclaration(moduleDeclarationContext, HDLContext);
                return;
            }

            var udpDeclarationContext = context.udp_declaration();
            if (udpDeclarationContext != null)
            {
#warning User defined primtive declaration is not implemented now
                return;
            }

            var ifDeclarationContext = context.interface_declaration();
            if (ifDeclarationContext != null)
            {
#warning Interface declaration is not implemented now
                return;
            }

            var progDeclarationContext = context.program_declaration();
            if (progDeclarationContext != null)
            {
#warning Program declaration is not implemented now
                return;
            }

            var packageContext = context.package_declaration();
            if (packageContext != null)
            {
#warning Package declaration is not implemented now
                return;
            }

            var pkgItemContext = context.package_item();
            if (pkgItemContext != null)
            {
#warning Package item declaration is not implemented now
                return;
            }

            var bindDirectiveContext = context.bind_directive();
            if (bindDirectiveContext != null)
            {
#warning Bind Directive is not implemented now
                return;
            }

            var configDeclarationContext = context.config_declaration();
            if (configDeclarationContext != null)
            {
#warning Configuration declaration is not implemented now
                return;
            }

            throw new Exception("Unexpected module declaration");
        }
    }
}
