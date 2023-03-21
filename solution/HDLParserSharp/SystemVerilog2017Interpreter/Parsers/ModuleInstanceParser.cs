using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class ModuleInstanceParser : HDLParser
    {
        public ModuleInstanceParser(HDLParser other) : base(other) { }

        /// <summary>
        /// module_or_interface_or_program_or_udp_instantiation:
        ///     identifier ( parameter_value_assignment )?
        ///     hierarchical_instance ( COMMA hierarchical_instance )* SEMI;
        /// </summary>
        public void VisitAnyModuleInstantiation(Module_or_interface_or_program_or_udp_instantiationContext context,
            List<ComponentInstance> instances)
        {
            var moduleNameContext = context.identifier();
            string moduleName = moduleNameContext.GetText();

            var paramValueAssignContext = context.parameter_value_assignment();
            List<Expression>? genericMap = new List<Expression>();
            if (paramValueAssignContext != null)
            {
                // parameter_value_assignment: HASH LPAREN ( list_of_parameter_value_assignments )? RPAREN;
                var paramValueAssignListContext = paramValueAssignContext.list_of_parameter_value_assignments();
                if (paramValueAssignListContext != null)
                {
                    genericMap.AddRange(VisitParameterValueAssignments(paramValueAssignListContext));
                }
            }

            var hierarchicalInstanceContext = context.hierarchical_instance();
            ComponentInstance? first = null;

            foreach (var hier in hierarchicalInstanceContext)
            {
                ComponentInstance component;
                Identifier identifier = new Identifier(moduleName).UpdateCodePosition(moduleNameContext);
                if (first != null)
                {
                    component = VisitHierarchicalInstance(hier, identifier, genericMap);
                    first = component;
                }
                else
                {
                    var genericMapCopy = genericMap.Select(x => x.Clone()).ToList();
                    component = VisitHierarchicalInstance(hier, identifier, genericMapCopy);
                }

                instances.Add(component);
            }
        }


        private ComponentInstance VisitHierarchicalInstance(Hierarchical_instanceContext context, Identifier moduleIdentifier, List<Expression> genericMap)
        {
            // NOTE: generic map became the owner of all Expression instances in the list
            //       that means that instances can not be shared
            // hierarchical_instance: name_of_instance LPAREN list_of_port_connections RPAREN;
            // name_of_instance: identifier ( unpacked_dimension )*;

            var nameOfInstanceContext = context.name_of_instance();
            var unpkDimensionContext = nameOfInstanceContext.unpacked_dimension();

            TypeParser typeParser = new TypeParser(this);
            Expression name = ExpressionParser.VisitIdentifier(nameOfInstanceContext.identifier());
            
            name = typeParser.ApplyUnpakcedDimension(name, unpkDimensionContext);
            IEnumerable<Expression> portMap = VisitPortConnections(context.list_of_port_connections());

            ComponentInstance component = new ComponentInstance(name, moduleIdentifier);
            component.SetGenericMap(genericMap);
            component.SetPortMap(portMap);

            return component;
        }

        /// <summary>
        /// list_of_port_connections
        ///    : ordered_port_connection (',' ordered_port_connection)*
        ///    | named_port_connection (',' named_port_connection)*
        ///    ;
        ///
        /// </summary>
        private IEnumerable<Expression> VisitPortConnections(List_of_port_connectionsContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            var orderedPortConnectionContext = context.ordered_port_connection();
            
            if (orderedPortConnectionContext.Any())
            {
                return orderedPortConnectionContext.Select(delegate (Ordered_port_connectionContext portContext)
                {
                    if (portContext.attribute_instance().Any())
                    {
#warning Attribute instance of ordered port connection is not implemented now
                    }

                    var expressionContext = portContext.expression();
                    return (expressionContext != null) ?
                            expressionParser.VisitExpression(expressionContext) :
                            SymbolType.Null.AsNewSymbol();
                });
            }
            else
            {
                //
                // named_port_connection:
                //  ( attribute_instance )* DOT ( MUL
                //                                | identifier ( LPAREN ( expression )? RPAREN )?
                //                               );
                // named_port_connection
                //    : attribute_instance* '.' identifier '(' (expression)? ')'
                //    ;
                // port_identifier : identifier;

                var namedPortConnection = context.named_port_connection();
                return namedPortConnection.Select(delegate (Named_port_connectionContext portContext)
                {
                    if (portContext.attribute_instance().Any())
                    {
#warning Attribute instance of named port connection is not implemented now
                    }

                    var identifierContext = portContext.identifier();
                    var valueContext = portContext.expression();

                    Expression identifier = (identifierContext != null) ?
                                            ExpressionParser.VisitIdentifier(identifierContext) :   // .name
                                            SymbolType.All.AsNewSymbol();                           // .*

                    Expression value = (valueContext != null) ?
                                       expressionParser.VisitExpression(valueContext) :             // .name(value)
                                       SymbolType.Null.AsNewSymbol();                               // .name()

                    return new Operator(OperatorType.MapAssociation, identifier, value);
                });
            }
        }

        /// <summary>
        /// list_of_parameter_value_assignments:
        ///     param_expression ( COMMA param_expression )*
        ///     | named_parameter_assignment ( COMMA named_parameter_assignment )*
        /// ;
        /// list_of_parameter_assignments
        ///    : ordered_parameter_assignment (',' ordered_parameter_assignment)*
        ///    | named_parameter_assignment (',' named_parameter_assignment)*
        ///    ;
        /// </summary>
        public IEnumerable<Expression> VisitParameterValueAssignments(List_of_parameter_value_assignmentsContext context)
        {
            var paramExpressionContext = context.param_expression();
            ParameterDefinitionParser parameterDefinitionParser = new ParameterDefinitionParser(this);
            if (paramExpressionContext.Any())
            {
                return paramExpressionContext.Select(pe => parameterDefinitionParser.VisitParameterExpression(pe));
            }
            else
            {
                var namedParameterAssignContext = context.named_parameter_assignment();

                return namedParameterAssignContext.Select(delegate (Named_parameter_assignmentContext npaContext)
                {
                    // named_parameter_assignment: DOT identifier LPAREN ( param_expression )? RPAREN;
                    Expression idenfitier = ExpressionParser.VisitIdentifier(npaContext.identifier());
                    var expressionContext = npaContext.param_expression();
                    Expression value = (expressionContext != null) ?
                                       parameterDefinitionParser.VisitParameterExpression(expressionContext) :
                                       SymbolType.Null.AsNewSymbol();

                    return new Operator(OperatorType.MapAssociation, idenfitier, value).UpdateCodePosition(npaContext);
                });
            }
        }
    }
}
