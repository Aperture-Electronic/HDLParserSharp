using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using static SystemVerilog2017.SystemVerilog2017Parser;
using HDLAbstractSyntaxTree.Definition;
using HDLParserBase;
using HDLAbstractSyntaxTree.HDLElement;
using SystemVerilog2017Interpreter.Extensions;
using SystemVerilog2017Interpreter.Context;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using System.Linq;
using HDLAbstractSyntaxTree.BasicUnit;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class ModuleParser : HDLParser
    {
        public ModuleParser(HDLParser other) : base(other) 
        {
        
        }  

        public ModuleParser(CommentParser commentParser, bool hierarchyOnly)
            : base(commentParser, hierarchyOnly)
        {

        }

        public void VisitModuleHeaderCommon(Module_header_commonContext context, ModuleDeclaration moduleDec)
        {
            // module_header_common:
            //   ( attribute_instance )* module_keyword ( lifetime )? identifier
            //   ( package_import_declaration )* ( parameter_port_list )?;
            new AttributeParser(this).VisitAttributeInstance(context.attribute_instance());

            var lifeTimeContext = context.lifetime();
            if (lifeTimeContext != null)
            {
#warning Module lifetime is not implemented now
            }

            moduleDec.Document = CommentParser.Parse(context);
            moduleDec.Name = context.identifier().GetText();
            foreach (var pkgImportDeclarationContext in context.package_import_declaration())
            {
#warning Package import and declaration is not implemented now
            }

            var modulePortListContext = context.parameter_port_list();
            if (modulePortListContext != null)
            {
                ParameterDefinitionParser parameterDefinitionParser = new ParameterDefinitionParser(this);
                List<HDLObject> generics = new List<HDLObject>();
                parameterDefinitionParser.VisitParameterPortList(modulePortListContext, generics);
                moduleDec.Generics.TryAddRange(generics);
            }
        }

        public void VisitModuleDeclaration(Module_declarationContext context, List<HDLObject> moduleObjects)
        {
            var headerCommonContext = context.module_header_common();
            ModuleDeclaration entity = new ModuleDeclaration().UpdateCodePosition(context);
            ModuleContext moduleContext = new ModuleContext(entity);
            VisitModuleHeaderCommon(headerCommonContext, entity);

            var portListDeclarationContext = context.list_of_port_declarations();
            if (portListDeclarationContext != null)
            {
                PortParser portParser = new PortParser(this, moduleContext.NonANSIPortGroups);
                var ports = portParser.VisitPortDeclarations(portListDeclarationContext);
                entity.Ports.AddRange(ports);
            }
            else
            {
                if (context.MUL() != null)
                {
                    // All port auto connection in System Verilog (.*)
                    IdentifierDefinition allPortIdentifier = new IdentifierDefinition(".*", SymbolType.All.AsNewSymbol(), null)
                        .UpdateCodePosition(context);
                    entity.Ports.Add(allPortIdentifier);
                }
            }

            // External definition
            if (context.KW_EXTERN() != null)
            {
                foreach (var generic in entity.Generics)
                {
                    generic.Type ??= SymbolType.Auto.AsNewSymbol();
                }

                foreach (var port in entity.Ports)
                {
                    port.Type ??= SymbolType.Auto.AsNewSymbol();
                }

                moduleObjects.Add(entity);
                return;
            }

            // Generate the AST for module definition
            ModuleDefinition architecture = new ModuleDefinition();
            moduleContext.Architecture = architecture;
            var timeUnitsDeclarationContext = context.timeunits_declaration();
            if (timeUnitsDeclarationContext != null)
            {
#warning Time units declaration is not implemented now
            }

            foreach (var moduleItemContext in context.module_item())
            {
                VisitModuleItem(moduleItemContext, architecture.Objects, moduleContext);
            }

            architecture.ModuleName = new Identifier(moduleContext.Entity.Name).UpdateCodePosition(context);
            architecture.Entity = entity;

            // Process any non-ANSI ports
            if (moduleContext.NonANSIPortGroups.Any())
            {
                PortParser portParser = new PortParser(this, moduleContext.NonANSIPortGroups);
                portParser.ConvertNonANSIPortsToANSI(context, moduleContext.Entity.Ports,
                    moduleContext.Architecture.Objects);
            }

            bool ConsumeNonANSIPortsVaribales(HDLObject obj)
            {
                if (obj is IdentifierDefinition idDefinition)
                {
                    if (moduleContext.Entity.GetPortByName(idDefinition.Name) is IdentifierDefinition idPort)
                    {
                        // This is a variable which specifies the real type of the non-ansi port
                        idPort.UpdateWith(idDefinition, updateStatic: true);
                        return true;
                    }

                    return false;
                }

                return false;
            }

            moduleContext.Architecture.Objects.RemoveAll((obj) => ConsumeNonANSIPortsVaribales(obj));

            // Generate implicit generics' type
            foreach (var generic in moduleContext.Entity.Generics)
            {
                generic.Type ??= SymbolType.Auto.AsNewSymbol();
            }

            // Generate ports' implicit type and direction
            Direction prevDirection = Direction.Inout;
            foreach (var port in moduleContext.Entity.Ports)
            {
                port.Type ??= SymbolType.Auto.AsNewSymbol();
                if (port.Direction == Direction.Unknown)
                {
                    port.Direction = prevDirection;
                }
                else
                {
                    prevDirection = port.Direction;
                }
            }

            moduleObjects.Add(architecture);
        }

        public void VisitModuleItemItem(Module_item_itemContext context, List<HDLObject> objects, IEnumerable<IdentifierDefinition> parameters)
        {
            // module_item_item:
            //     module_or_generate_item
            //     | specparam_declaration
            // ;

            var moduleGenerateItemContext = context.module_or_generate_item();
            if (moduleGenerateItemContext != null)
            {
                GenerateParser generateParser = new GenerateParser(this);
                generateParser.VisitModuleOrGenerateItem(moduleGenerateItemContext, objects, parameters);

                return;
            }

            var specparamDeclarationContext = context.specparam_declaration();
            if (specparamDeclarationContext == null)
            {
                throw new Exception("Excepted specparam declaration");
            }

#warning Specparam declaration is not implemented now
        }

        public void VisitModuleItem(Module_itemContext context, List<HDLObject> objects, ModuleContext moduleContext)
        {
            // module_item:
            //     generate_region
            //     | ( attribute_instance )* module_item_item
            //     | specify_block
            //     | program_declaration
            //     | module_declaration
            //     | interface_declaration
            //     | timeunits_declaration
            //     | nonansi_port_declaration SEMI
            // ;

            string document = CommentParser.Parse(context);
            var generateRegionContext = context.generate_region();
            if (generateRegionContext != null)
            {
                GenerateParser generateParser = new GenerateParser(this);
                generateParser.VisitGenerateRegion(generateRegionContext, objects);
                return;
            }

            var moduleItemItemContext = context.module_item_item();
            if (moduleItemItemContext != null)
            {
                var attributeInstanceContext = context.attribute_instance();
                new AttributeParser(this).VisitAttributeInstance(attributeInstanceContext);
                int previousSize = objects.Count;
                VisitModuleItemItem(moduleItemItemContext, objects, moduleContext.Entity.Generics);

                // Any new item
                if (previousSize != objects.Count)
                {
                    if (objects[previousSize] is IDocumented documented)
                    {
                        documented.Document = document + documented.Document;
                    }
                }

                return;
            }

            var specifyBlockContext = context.specify_block();
            if (specifyBlockContext != null)
            {
#warning Specify block is not implemented now
                return;
            }

            var programDeclarationContext = context.program_declaration();
            if (programDeclarationContext != null)
            {
#warning Program declaration is not implemented now
                return;
            }

            var moduleDeclaration = context.module_declaration();
            if (moduleDeclaration != null)
            {
                ModuleParser moduleParser = new ModuleParser(this);
                moduleParser.VisitModuleDeclaration(moduleDeclaration, objects);
                return;
            }

            var interfaceDeclarationContext = context.interface_declaration();
            if (interfaceDeclarationContext != null)
            {
#warning Interface declaration is not implemented now
                return;
            }

            var timeUnitsDeclarationContext = context.timeunits_declaration();
            if (timeUnitsDeclarationContext != null)
            {
#warning Time units declaration is not implemented now
                return;
            }

            var nonANSIPortDeclarationContext = context.nonansi_port_declaration();
            if (nonANSIPortDeclarationContext != null)
            {
                PortParser portParser = new PortParser(this, moduleContext.NonANSIPortGroups);
                List<IdentifierDefinition> ports = new List<IdentifierDefinition>();
                portParser.VisitNonANSIPortDeclaration(nonANSIPortDeclarationContext, ports);
                bool first = true;
                foreach (var port in ports)
                {
                    IdentifierDefinition? portEntity = moduleContext.Entity.GetPortByName(port.Name);
                    if (portEntity != null)
                    {
                        port.UpdateWith(portEntity);
                        if (first)
                        {
                            portEntity.Document += document;
                            first = false;
                        }
                    }
                    else
                    {
                        if (first)
                        {
                            port.Document += document;
                            first = false;
                        }

                        if (moduleContext.Entity.Ports.Any() && moduleContext.Entity.Ports.Last().Name == ".*")
                        {
                            moduleContext.Entity.Ports.Insert(moduleContext.Entity.Ports.Count - 1, port);
                        }
                        else
                        {
                            moduleContext.Entity.Ports.Add(port);
                        }
                    }
                }
                return;
            }

            throw new Exception("Expected any module item in context");
        }

        public void VisitNetDeclaration(Net_declarationContext context, List<HDLObject> identifiers) 
        {
            // net_declaration:
            //  ( KW_INTERCONNECT ( implicit_data_type )? ( HASH delay_value )? identifier ( unpacked_dimension )* (
            //   COMMA identifier ( unpacked_dimension )* )?
            //   | ( net_type ( drive_strength
            //                   | charge_strength
            //                   )? ( KW_VECTORED
            //                           | KW_SCALARED
            //                           )? ( data_type_or_implicit )? ( delay3 )?
            //       | identifier ( delay_control )?
            //       ) list_of_net_decl_assignments
            //   ) SEMI;
            if (context.KW_INTERCONNECT() != null)
            {
#warning Interconnect type is not implemented now
                return;
            }

            TypeParser typeParser = new TypeParser(this);
            Expression? netType = null;
            var netTypeContext = context.net_type();
            if (netTypeContext != null)
            {
                netType = typeParser.VisitNetType(netTypeContext);
            }

            if (context.drive_strength() != null)
            {
#warning Drive strength is not implemented now
                return;
            }

            if (context.charge_strength() != null)
            {
#warning Charge strength is not implemented now
                return;
            }

            if (context.KW_VECTORED() != null) 
            {
#warning Vectored net declaration is not implemented now
                return;
            }

            if (context.delay3() != null)
            {
#warning Delay3 sentence is not implemented now
            }

            var dataTypeImplicitContext = context.data_type_or_implicit();
            Expression typeOrImplicit = typeParser.VisitDataTypeOrImplicit(dataTypeImplicitContext, netType);
            var netDeclarationAssignContext = context.list_of_net_decl_assignments();
            VisitNetDeclarationAssignments(netDeclarationAssignContext, typeOrImplicit, false, identifiers);
        }

        /// <summary>
        /// Same as <see cref="VisitNetIdentifiers"/> but without the dimensions and with the default value
        /// </summary>
        private void VisitNetDeclarationAssignments(List_of_net_decl_assignmentsContext context, 
            Expression baseType, bool isLatched, List<HDLObject> identifiers)
        {
            TypeParser typeParser = new TypeParser(this);
            bool first = true;
            foreach (var netDeclarationContext in context.net_decl_assignment())
            {
                var identifierContext = netDeclarationContext.identifier();
                string idString = ExpressionParser.GetIdentifierString(identifierContext);
                Expression? defaultValue = null;
                var expressionContext = netDeclarationContext.expression();
                if (expressionContext != null)
                {
                    defaultValue = new ExpressionParser(this).VisitExpression(expressionContext);
                }

                Expression type;
                if (first)
                {
                    type = baseType;
                }
                else
                {
                    type = baseType.Clone();
                }

                var unpackedDimContext = netDeclarationContext.unpacked_dimension();
                type = typeParser.ApplyUnpakcedDimension(type, unpackedDimContext);
                var netIdentifier = new IdentifierDefinition(idString, type, defaultValue).
                    UpdateCodePosition(netDeclarationContext);

                first &= false;
                
                netIdentifier.IsLatched = isLatched;
                identifiers.Add(netIdentifier);
            }
        }
    }
}
