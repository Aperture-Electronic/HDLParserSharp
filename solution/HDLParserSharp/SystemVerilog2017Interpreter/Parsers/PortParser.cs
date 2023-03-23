using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using HDLAbstractSyntaxTree.Value;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class PortParser : HDLParser
    {
        public Dictionary<string, IdentifierDefinition> PortGroups { get; }

        public PortParser(HDLParser other, Dictionary<string, IdentifierDefinition> portGroups)
            : base(other)
        {
            PortGroups = portGroups;
        }

        private (string, Expression?) SplitTypeBaseNameAndArraySize(Expression expression, string typeName)
        {
            var top = expression;
            for (; ; )
            {
                if (top is Operator op)
                {
                    top = op.Operands.First();
                }
                else
                {
                    if (top is Identifier literal)
                    {
                        if (top == expression)
                        {
                            return (literal.Name, null);
                        }

                        string tempName = literal.Name;
                        literal.Name = typeName;
                        return (tempName, expression);
                    }
                    else
                    {
                        throw new InvalidCastException("Can not extract a string from a expression which is not a string.");
                    }
                }

            }
        }

        public IdentifierDefinition VisitNonANSIPort(Nonansi_portContext context)
        {
            var expressionContext = context.nonansi_port__expr();
            var identifierContext = context.identifier();
            IdentifierDefinition identifierDef;
            if (identifierContext != null)
            {
                string identifierName = ExpressionParser.GetIdentifierString(identifierContext);
                Expression? value = expressionContext == null ? null : VisitNonANSIPortExpressionAsExpression(expressionContext);
                identifierDef = new IdentifierDefinition(identifierName, null, value).UpdateCodePosition(identifierContext);
                PortGroups.Add(identifierName, identifierDef);
            }
            else
            {
                identifierDef = VisitNonANSIPortExpressionAsValue(expressionContext);
            }

            identifierDef.Document = CommentParser.Parse(context);
            return identifierDef;
        }

        public Expression VisitNonANSIPortExpressionAsExpression(Nonansi_port__exprContext context)
        {
            ExpressionParser exprParser = new ExpressionParser(this);
            if (context.LBRACE() != null)
            {
                var operators = from c in context.identifier_doted_index_at_end()
                                select exprParser.VisitIdentifierDotedIndexAtEnd(c) as Operator;

                return Operator.Reduce(operators);
            }

            var idContext = context.identifier_doted_index_at_end(0);
            return exprParser.VisitIdentifierDotedIndexAtEnd(idContext);
        }

        public IdentifierDefinition VisitNonANSIPortExpressionAsValue(Nonansi_port__exprContext context)
        {
            var expression = VisitNonANSIPortExpressionAsExpression(context);
            if (context.LBRACE() != null)
            {
                return new IdentifierDefinition("", null, expression).UpdateCodePosition(context);
            }

            // The port may has an abbr. type of 'wire'
            (string identifier, Expression? size) = SplitTypeBaseNameAndArraySize(expression, "wire");

            return new IdentifierDefinition(identifier, size, null).UpdateCodePosition(context);
        }

        /// <summary>
        /// list_of_tf_variable_identifiers:
        ///     list_of_tf_variable_identifiers_item
        ///     ( COMMA list_of_tf_variable_identifiers_item )*
        /// ;
        /// </summary>
        public void VisitTaskFunctionVariableIdentifiers(List_of_tf_variable_identifiersContext context, 
            Expression baseType, bool isLatched, Direction direction, string document, List<IdentifierDefinition> ports)
        {
            TypeParser typeParser = new TypeParser(this);
            ExpressionParser expressionParser = new ExpressionParser(this);

            bool first = true;

            var tfVariableIdentifierContext = context.list_of_tf_variable_identifiers_item();
            foreach (var tfVariableIdentifier in tfVariableIdentifierContext)
            {
                // list_of_tf_variable_identifiers_item: identifier ( variable_dimension )* ( ASSIGN expression )?
                Expression type = first ? baseType : baseType.Clone();

                var varDimensionContext = tfVariableIdentifier.variable_dimension();
                type = typeParser.ApplyVariableDimension(type, varDimensionContext);
                var identifierContext = tfVariableIdentifier.identifier();
                string name = ExpressionParser.GetIdentifierString(identifierContext);
                var defaultValueContext = tfVariableIdentifier.expression();
                Expression? defaultValue = (defaultValueContext != null) ?
                                           expressionParser.VisitExpression(defaultValueContext) :
                                           null;

                IdentifierDefinition portDefinition = 
                    new IdentifierDefinition(name, type, defaultValue);

                if (first)
                {
                    portDefinition.Document = document;
                }

                portDefinition.IsLatched = isLatched;
                portDefinition.Direction = direction;

                first &= false;

                ports.Add(portDefinition);
            }
        }

        /// <summary>
        /// tf_port_list: tf_port_item ( COMMA tf_port_item )*;
        /// </summary>
        public void VisitTaskFunctionPort(Tf_port_listContext context, List<IdentifierDefinition> arguments)
            => arguments.AddRange(context.tf_port_item().Select(i => VisitTaskFunctionPortItem(i)));

        /// <summary>
        /// tf_port_item:
        ///     ( attribute_instance )* ( tf_port_direction )? ( KW_VAR )? ( data_type_or_implicit )?
        ///     ( identifier ( variable_dimension )* ( ASSIGN expression )? )?;
        /// </summary>
        private IdentifierDefinition VisitTaskFunctionPortItem(Tf_port_itemContext context)
        {
            AttributeParser.VisitAttributeInstance(context.attribute_instance());
            TypeParser typeParser = new TypeParser(this);
            ExpressionParser expressionParser = new ExpressionParser(this);
            
            Direction direction = Direction.In;

            var dataTypeImplicitContext = context.data_type_or_implicit();
            var varDimensionContext = context.variable_dimension();

            Expression dataType = typeParser.VisitDataTypeOrImplicit(dataTypeImplicitContext, null);
            dataType = typeParser.ApplyVariableDimension(dataType, varDimensionContext);

            var identifierContext = context.identifier();
            string name = (identifierContext != null) ?
                          ExpressionParser.GetIdentifierString(identifierContext) :
                          string.Empty;

            var valueContext = context.expression();
            Expression? defaultValue = (valueContext != null) ?
                                       expressionParser.VisitExpression(valueContext) :
                                       null;

            bool isLatched = context.KW_VAR() != null;

            return new IdentifierDefinition(name, dataType, defaultValue, direction, isLatched)
                .UpdateCodePosition(context);
        }

        public IEnumerable<IdentifierDefinition> VisitPortDeclarations(List_of_port_declarationsContext context)
        {
            foreach (var port in context.nonansi_port())
            {
                var portIdentifier = VisitNonANSIPort(port);
                yield return portIdentifier;
            }

            (IdentifierDefinition? identifier, Expression? expression) previous = (null, null);
            foreach (var port in context.list_of_port_declarations_ansi_item())
            {
                AttributeParser.VisitAttributeInstance(port.attribute_instance());
                var declaration = port.ansi_port_declaration();
                var portDeclaration = VisitANSIPortDeclaration(declaration, (previous.identifier, previous.expression));
                portDeclaration.identifier.Document = CommentParser.Parse(port);
                previous = portDeclaration;
                yield return portDeclaration.identifier;
            }
        }

        public (IdentifierDefinition identifier, Expression? expression) VisitANSIPortDeclaration(
            Ansi_port_declarationContext context, (IdentifierDefinition? identifier, Expression? expression) previous)
        {
            // ansi_port_declaration:
            //   ( port_direction ( net_or_var_data_type )? // net_port_header
            //     | net_or_var_data_type                   // net_port_header or variable_port_header
            //     | (identifier | KW_INTERFACE) (DOT identifier )? // interface_port_header
            //   )? port_identifier ( variable_dimension )*
            //    (ASSIGN constant_expression)?
            //   | (port_direction)? DOT port_identifier LPAREN (expression)? RPAREN
            // ;       
            Direction direction = Direction.Unknown;
            if (previous.identifier != null)
            {
                direction = previous.identifier.Direction;
            }

            var portDirectionContext = context.port_direction();
            if (portDirectionContext != null)
            {
                previous.expression = null;
                direction = VisitPortDirection(portDirectionContext);
            }

            Expression? typeExpression = null;
            Expression? defaultValue = null;
            IdentifierDefinition identifierDef;
            bool isLatched = false;
            var netOrVariableDataType = context.net_or_var_data_type();
            var portIdentifier = context.port_identifier().identifier();
            var portName = ExpressionParser.GetIdentifierString(portIdentifier);

            ExpressionParser expressionParser = new ExpressionParser(this);
            TypeParser typeParser = new TypeParser(this);

            if (netOrVariableDataType != null)
            {
                (typeExpression, isLatched) = typeParser.VisitNetOrVariableDataType(netOrVariableDataType);
            }
            else
            {
                var interfaceNode = context.KW_INTERFACE();
                if (interfaceNode != null)
                {
                    typeExpression = new Identifier("interface").UpdateCodePosition(interfaceNode);
                }

                foreach (var identifierContext in context.identifier())
                {
                    var identifier = ExpressionParser.VisitIdentifier(identifierContext);
                    typeExpression = typeExpression.Append(context, OperatorType.Dot, identifier);
                }
            }
        
            if ((typeExpression == null) && (context.LPAREN() != null))
            {
                // | (port_direction)? DOT port_identifier LPAREN (expression)? RPAREN
                var expressionContext = context.expression();
                defaultValue = expressionContext != null ? expressionParser.VisitExpression(expressionContext) : null;
                identifierDef = new IdentifierDefinition(portName, SymbolType.Auto.AsNewSymbol(), 
                                    defaultValue, direction, isLatched).UpdateCodePosition(context);
                return (identifierDef, null);
            }
            else if (typeExpression == null)
            {
                if (previous.expression != null)
                {
                    typeExpression = previous.expression.Clone();
                }
                else
                {
                    typeExpression = ExpressionExtension.WireIdentifier;
                }
            }

            var variableDimensionContext = context.variable_dimension();
            typeExpression = typeParser.ApplyVariableDimension(typeExpression, variableDimensionContext);
            var constExpressionContext = context.constant_expression();
            if (constExpressionContext != null)
            {
                defaultValue = expressionParser.VisitConstantExpression(constExpressionContext);
            }

            identifierDef = new IdentifierDefinition(portName, typeExpression, defaultValue, direction, isLatched).
                UpdateCodePosition(context);
            return (identifierDef, typeExpression);
        }

        /// <summary>
        /// nonansi_port_declaration:
        ///    ( attribute_instance )* (
        ///    KW_INOUT ( net_port_type )? list_of_variable_identifiers
        ///   | KW_INPUT ( net_or_var_data_type )? list_of_variable_identifiers
        ///   | KW_OUTPUT ( net_or_var_data_type )? list_of_variable_port_identifiers
        ///   | identifier ( DOT identifier )? list_of_variable_identifiers // identifier=interface_identifier
        ///   | KW_REF ( var_data_type )? list_of_variable_identifiers
        ///   )
        /// ;
        /// </summary>
        public void VisitNonANSIPortDeclaration(Nonansi_port_declarationContext context, List<IdentifierDefinition> identifiers)
        {
            string document = CommentParser.Parse(context);
            var attributeContext = context.attribute_instance();
            AttributeParser.VisitAttributeInstance(attributeContext);

            TypeParser typeParser = new TypeParser(this);

            var varIdentifierContext = context.list_of_variable_identifiers();
            if (context.KW_INOUT() != null)
            {
                var netPortType = typeParser.VisitNetPortType(context.net_port_type());
                VisitVariableIdentifiers(varIdentifierContext, netPortType, false, Direction.Inout, document, identifiers);
            }
            else if (context.KW_INPUT() != null)
            {
                (Expression type, bool isLatched) = typeParser.VisitNetOrVariableDataType(context.net_or_var_data_type()); ;
                VisitVariableIdentifiers(varIdentifierContext, type, isLatched, Direction.In, document, identifiers);
            }
            else if (context.KW_OUTPUT() != null)
            {
                (Expression type, bool isLatched) = typeParser.VisitNetOrVariableDataType(context.net_or_var_data_type()); ;
                var varPortIdentifierContext = context.list_of_variable_port_identifiers();
                VisitVariablePortIdentifiers(varPortIdentifierContext, type, isLatched, Direction.Out, document, identifiers);
            }
            else
            {
                var identifierContext = context.identifier();
                if (identifierContext.Any())
                {
                    Expression type = ExpressionParser.VisitIdentifier(identifierContext.First());
                    if (identifierContext.Length > 1)
                    {
                        if (identifierContext.Length != 2)
                        {
                            throw new Exception("Invalid type identifier for non-ANSI port");
                        }

                        type = new Operator(OperatorType.Dot, type, ExpressionParser.VisitIdentifier(identifierContext.Last()));
                    }

                    VisitVariableIdentifiers(varIdentifierContext, type, false, Direction.Linkage, document, identifiers);
                }
                else
                {
                    var varDataTypeContext = context.var_data_type();
                    Expression variableDataType = typeParser.VisitVaribaleDataType(varDataTypeContext);
                    VisitVariableIdentifiers(varIdentifierContext, variableDataType, false,
                        Direction.Linkage, document, identifiers);
                }
            }
        }

        /// <summary>
        /// list_of_variable_port_identifiers: list_of_tf_variable_identifiers;
        /// </summary>
        public void VisitVariablePortIdentifiers(List_of_variable_port_identifiersContext context,
            Expression baseType, bool isLatched, Direction direction, string document, List<IdentifierDefinition> identifiers)
            => VisitTaskFunctionVariableIdentifiers(context.list_of_tf_variable_identifiers(), 
                baseType, isLatched, direction, document, identifiers);

        public void ConvertNonANSIPortsToANSI(Module_declarationContext context, List<IdentifierDefinition> identifiers,
            List<HDLObject> body)
        {
#warning Conversion of non-ANSI ports are not implemented
        }

        public void VisitVariableIdentifiers(List_of_variable_identifiersContext context, Expression baseType,
            bool isLatched, Direction direction, string document, List<IdentifierDefinition> identifiers)
        {
            TypeParser typeParser = new TypeParser(this);
            bool first = true;

            foreach (var varIdentifier in context.list_of_variable_identifiers_item())
            {
                Expression typeExpr = first ? baseType : baseType.Clone();
                var dimensionContext = varIdentifier.variable_dimension();
                typeExpr = typeParser.ApplyVariableDimension(typeExpr, dimensionContext);
                var identifierContext = varIdentifier.identifier();
                string identifierString = ExpressionParser.GetIdentifierString(identifierContext);
                IdentifierDefinition definition = new IdentifierDefinition(identifierString, typeExpr, null).
                    UpdateCodePosition(varIdentifier);
                if (first)
                {
                    first = false;
                    definition.Document = document;
                }

                definition.IsLatched = isLatched;
                definition.Direction = direction;
                identifiers.Add(definition);
            }
        }

        public Direction VisitPortDirection(Port_directionContext context)
            => false switch
            {
                _ when (context.KW_INPUT() != null) => Direction.In,
                _ when (context.KW_OUTPUT() != null) => Direction.Out,
                _ when (context.KW_INOUT() != null) => Direction.Inout,
                _ when (context.KW_REF() != null) => Direction.Linkage,
                _ =>  throw new Exception("Invalid port direction definition")
            };

        /// <summary>
        /// tf_port_direction:
        ///     KW_CONST KW_REF
        ///     | port_direction
        /// ;
        /// </summary>
        public Direction VisitTaskFunctionPortDirection(Tf_port_directionContext context)
            => false switch
            {
                _ when (context.KW_REF() != null) => Direction.Linkage,
                _ => VisitPortDirection(context.port_direction())
            };
    }
}
