using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemVerilog2017;
using SystemVerilog2017Interpreter.Extensions;
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
                new StatementParser(this).VisitBlockItemDeclaration(blockItemDeclContext, objects);

                // Process only new items
                for (int i = prevObjectsCount; i < objects.Count; )
                {
                    HDLObject item = objects[i];
                    bool merged = false;
                    if (item is IdentifierDefinition identifierDefinition)
                    {
                        // Find same name port to merge
                        var sameNameQuery = from port in ports
                                            where port.Name == identifierDefinition.Name
                                            select port;
                        
                        if (sameNameQuery.Any())
                        {
                            IdentifierDefinition mergePort = sameNameQuery.First();

                            mergePort.Type = identifierDefinition.Type;
                            mergePort.Value = identifierDefinition.Value;
                            objects.Remove(item);

                            merged = true;
                        }
                    }

                    // We need to see the same index when merged,
                    // because we removed the item
                    i += merged ? 0 : 1;
                }
            }
            else
            {
                var tfPortDeclarationContext = context.tf_port_declaration();
                VisitTaskFunctionPortDeclaration(tfPortDeclarationContext, ports);
            }
        }

        /// <summary>
        /// tf_port_declaration:
        ///     ( attribute_instance )* tf_port_direction ( KW_VAR )? ( data_type_or_implicit )?
        ///     list_of_tf_variable_identifiers SEMI
        /// ;
        /// </summary>
        private void VisitTaskFunctionPortDeclaration(Tf_port_declarationContext context, List<IdentifierDefinition> ports)
        {
            AttributeParser.VisitAttributeInstance(context.attribute_instance());
            var tfPortDirectionContext = context.tf_port_direction();

            Dictionary<string, IdentifierDefinition> nonANSIPortGroups = new Dictionary<string, IdentifierDefinition>();
            PortParser portParser = new PortParser(this, nonANSIPortGroups);

            Direction portDirection = portParser.VisitTaskFunctionPortDirection(tfPortDirectionContext);
            if (context.KW_VAR() != null)
            {
                portDirection = Direction.Linkage;
            }

            TypeParser typeParser = new TypeParser(this);
            var dataTypeImplicitContext = context.data_type_or_implicit();
            Expression type = typeParser.VisitDataTypeOrImplicit(dataTypeImplicitContext, null);
            var tfVariableIdentifierContext = context.list_of_tf_variable_identifiers();
            string document = CommentParser.Parse(context);

            portParser.VisitTaskFunctionVariableIdentifiers(tfVariableIdentifierContext, type, false, portDirection, document, ports);
        }

        /// <summary>
        /// task_and_function_declaration_common:
        ///     ( identifier DOT | class_scope )? identifier
        ///     ( SEMI ( tf_item_declaration )*
        ///       | LPAREN tf_port_list RPAREN SEMI ( block_item_declaration )*
        ///     )
        ///     ( statement_or_null )*
        /// ;
        /// </summary>
        public FunctionDefinition VisitTaskFunctionDeclarationCommon(Task_and_function_declaration_commonContext context,
            Expression returnType, bool isStatic, bool isTask)
        {
            var identifierContext = context.identifier();
            string name = (identifierContext.Length == 2) ? 
                          ExpressionParser.GetIdentifierString(identifierContext.Last()) :
                          ExpressionParser.GetIdentifierString(identifierContext.First());

#warning Hierarchical name of task/function declaration is not implemented now
#warning Class scope name of task/function declaration is not implemented now

            FunctionDefinition functionDefinition = new FunctionDefinition(name, false, returnType)
                                                    .UpdateCodePosition(context);

            functionDefinition.IsStatic = isStatic;
            functionDefinition.IsTask = isTask;
            functionDefinition.IsDeclarationOnly = false;

            Dictionary<string, IdentifierDefinition> nonANSIPortGroup = new Dictionary<string, IdentifierDefinition>();
            var portListContext = context.tf_port_list();
            if (portListContext != null)
            {
                PortParser portParser = new PortParser(this, nonANSIPortGroup);
                portParser.VisitTaskFunctionPort(portListContext, functionDefinition.Arguments);
            }
            else
            {
                var itemDeclarationContext = context.tf_item_declaration();
                foreach (var itemContext in itemDeclarationContext)
                {
                    VisitTaskFunctionItemDeclaration(itemContext, functionDefinition.Body, functionDefinition.Arguments);
                }
            }

            StatementParser statementParser = new StatementParser(this);
            var blkItemDeclarationContext = context.block_item_declaration();
            var statementOrNullContext = context.statement_or_null();

            foreach (var blkItemContext in blkItemDeclarationContext)
            {
                statementParser.VisitBlockItemDeclaration(blkItemContext, functionDefinition.Body);
            }

            foreach (var statementContext in statementOrNullContext)
            {
                var statement = statementParser.VisitStatementOrNull(statementContext);
                if (statement != null)
                {
                    functionDefinition.Body.Add(statement); 
                }
            }

            if (nonANSIPortGroup.Any())
            {
#warning Non-ANSI ports of task/function are not implemented now
            }

            return functionDefinition;
        }

        /// <summary>
        /// task_declaration:
        ///     KW_TASK ( lifetime )?
        /// 	   task_and_function_declaration_common
        ///     KW_ENDTASK ( COLON identifier | {_input->LA(1) != COLON}? )
        /// ;
        /// </summary>
        public FunctionDefinition VisitTaskDeclaration(Task_declarationContext context)
        {
            TypeParser typeParser = new TypeParser(this);
            bool isStatic = typeParser.VisitLifeTime(context.lifetime());
            Identifier returnType = new Identifier("void").UpdateCodePosition(context);
            var commonContext = context.task_and_function_declaration_common();
            return VisitTaskFunctionDeclarationCommon(commonContext, returnType, isStatic, true);
        }

        /// <summary>
        /// function_declaration:
        ///    KW_FUNCTION ( lifetime )?
        ///    ( function_data_type_or_implicit )?
        ///    task_and_function_declaration_common
        ///    KW_ENDFUNCTION ( COLON identifier | {_input->LA(1) != COLON}? )
        /// ;
        /// </summary>
        public FunctionDefinition VisitFunctionDeclaration(Function_declarationContext context)
        {
            TypeParser typeParser = new TypeParser(this);
            bool isStatic = typeParser.VisitLifeTime(context.lifetime());
            var dataTypeImplicitContext = context.function_data_type_or_implicit();
            Expression returnType = typeParser.VisitFunctionDataTypeOrImplicit(dataTypeImplicitContext);
            var commonContext = context.task_and_function_declaration_common();
            return VisitTaskFunctionDeclarationCommon(commonContext, returnType, isStatic, false);
        }
    }
}
