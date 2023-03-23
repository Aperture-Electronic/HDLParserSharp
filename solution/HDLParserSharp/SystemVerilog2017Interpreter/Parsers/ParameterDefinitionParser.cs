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
    public class ParameterDefinitionParser : HDLParser
    {
        public ParameterDefinitionParser(HDLParser other) : base(other) { }

        internal void VisitParameterPortList(Parameter_port_listContext context, List<HDLObject> generics)
        {
            // parameter_port_list:
            //     HASH LPAREN (
            //        ( list_of_param_assignments
            //          | parameter_port_declaration
            //         ) ( COMMA parameter_port_declaration )* )? RPAREN;

            var paramAssignContext = context.list_of_param_assignments();
            if (paramAssignContext != null )
            {
                VisitParameterAssignments(paramAssignContext, generics);
                return;
            }

            foreach (var paramDeclarationContext in context.parameter_port_declaration())
            {
                VisitParameterPortDeclaration(paramDeclarationContext, generics);
            }
        }

        public void VisitParameterPortDeclaration(Parameter_port_declarationContext context, List<HDLObject> generics)
        {
            var typeAssignContext = context.list_of_type_assignments();
            if (typeAssignContext != null )
            {
                VisitTypeAssignments(typeAssignContext, generics);
                return;
            }

            var paramDeclarationContext = context.parameter_declaration();
            if (paramDeclarationContext != null )
            {
                VisitParameterDeclaration(paramDeclarationContext, generics);
                return;
            }

            var lParamDeclarationContext = context.local_parameter_declaration();
            if (lParamDeclarationContext != null)
            {
                VisitLocalParameterDeclaration(lParamDeclarationContext, generics);
                return;
            }

            var dataTypeContext = context.data_type();
            if (dataTypeContext == null)
            {
                throw new Exception("Expected a data type");
            }

            var paramAssignContext = context.list_of_param_assignments();
            if (paramAssignContext == null)
            {
                throw new Exception("Excepted a/some parameter assignment(s)");
            }

            Expression dataType = new TypeParser(this).VisitDataType(dataTypeContext);
            string document = CommentParser.Parse(context);
            VisitTypedParameterAssignments(dataType, paramAssignContext, document, generics);
        }

        public void VisitTypedParameterAssignments(Expression dataType, List_of_param_assignmentsContext context, string document, List<HDLObject> generics)
        {
            List<HDLObject> paramAssignments = new List<HDLObject>();
            VisitParameterAssignments(context, paramAssignments);
            bool first = true;
            foreach (IdentifierDefinition pAssign in paramAssignments)
            {
                if (first)
                {
                    pAssign.Type = dataType;
                    pAssign.Document = document + pAssign.Document;
                    first = false;
                }
                else
                {
                    pAssign.Type = dataType.Clone();
                }

                generics.Add(pAssign);
            }
        }

        public void VisitParameterAssignments(List_of_param_assignmentsContext context, List<HDLObject> generics)
            => generics.AddRange(from pAssign in context.param_assignment() 
                                 select VisitParameterAssignment(pAssign));

        /// <summary>
        /// constant_param_expression: param_expression;
        /// </summary>
        public Expression VisitConstantParameterExpression(Constant_param_expressionContext context)
            => VisitParameterExpression(context.param_expression());

        public Expression VisitParameterExpression(Param_expressionContext context)
        {
            // param_expression:
            //     mintypmax_expression
            //     | data_type
            // ;
            var mintypemaxContext = context.mintypmax_expression();
            if (mintypemaxContext != null)
            {
                return new ExpressionParser(this).VisitMintypmaxExpression(mintypemaxContext);
            }
            else
            {
                var dataTypeContext = context.data_type();
                if (dataTypeContext != null)
                {
                    return new TypeParser(this).VisitDataType(dataTypeContext);
                }
            }

            throw new Exception("Excepted a mintypmax or a data type");
        }

        /// <summary>
        /// list_of_type_assignments: type_assignment ( COMMA type_assignment )*;
	    /// type_assignment: identifier ( ASSIGN data_type )?;
        /// </summary>
        public void VisitTypeAssignments(List_of_type_assignmentsContext context, List<HDLObject> generics)
        {
            TypeParser typeParser = new TypeParser(this);
            foreach (var typeAssignContext in context.type_assignment())
            {
                string idName = ExpressionParser.GetIdentifierString(typeAssignContext.identifier());
                Expression? typeExpression = null;
                var typeContext = typeAssignContext.data_type();
                if (typeContext != null)
                {
                    typeExpression = typeParser.VisitDataType(typeContext);
                }

                Symbol typeSymbol = SymbolType.Type.AsNewSymbol().UpdateCodePosition(typeAssignContext);
                IdentifierDefinition definition = new IdentifierDefinition(idName, typeSymbol, typeExpression).
                    UpdateCodePosition(typeAssignContext);

                generics.Add(definition);
            }
        }

        /// <summary>
        /// param_assignment:
	    ///     identifier ( unpacked_dimension )* ( ASSIGN constant_param_expression )?;
        /// </summary>
        public IdentifierDefinition VisitParameterAssignment(Param_assignmentContext context)
        {
            var constParamExprContext = context.constant_param_expression();
            Expression? value = null;
            if (constParamExprContext != null)
            {
                value = VisitConstantParameterExpression(constParamExprContext);
            }

            string idName = ExpressionParser.GetIdentifierString(context.identifier());
            Symbol autoSymbol = SymbolType.Auto.AsNewSymbol().UpdateCodePosition(context);
            IdentifierDefinition definition = new IdentifierDefinition(idName, autoSymbol, value).
                UpdateCodePosition(context);
            definition.Document += CommentParser.Parse(context);
            return definition;
        }

        /// <summary>
        /// parameter_declaration:
        ///     KW_PARAMETER
        ///     ( KW_TYPE list_of_type_assignments
        ///       | ( data_type_or_implicit )? list_of_param_assignments
        ///     );
        /// </summary>
        public void VisitParameterDeclaration(Parameter_declarationContext context, List<HDLObject> generics)
        {
            var typeAssignContext = context.list_of_type_assignments();
            if (typeAssignContext != null)
            {
                VisitTypeAssignments(typeAssignContext, generics);
                return;
            }

            var dataTypeImplicitContext = context.data_type_or_implicit();
            TypeParser typeParser = new TypeParser(this);

            // NOTE: Use 'context' instead of dataTypeImplicitContext(null)
            // for new auto symbol position updating
            Expression type = (dataTypeImplicitContext != null) ?
                              typeParser.VisitDataTypeOrImplicit(dataTypeImplicitContext, null) :
                              SymbolType.Auto.AsNewSymbol().UpdateCodePosition(context);

            var paramAssignment = context.list_of_param_assignments();
            string document = CommentParser.Parse(context);
            VisitTypedParameterAssignments(type, paramAssignment, document, generics);
        }

        /// <summary>
        /// local_parameter_declaration:
        ///  KW_LOCALPARAM ( KW_TYPE list_of_type_assignments
        ///                   | ( data_type_or_implicit )? list_of_param_assignments
        ///                   );
        /// </summary>
        public void VisitLocalParameterDeclaration(Local_parameter_declarationContext context, List<HDLObject> generics)
        {
            int originalGenericsCount = generics.Count;
            if (context.KW_TYPE() != null)
            {
                var typeAssignContext = context.list_of_type_assignments();
                VisitTypeAssignments(typeAssignContext, generics);
            }
            else
            {
                var dataTypeImplicitContext = context.data_type_or_implicit();

                // NOTE: Use 'context' instead of dataTypeImplicitContext(null)
                // for new auto symbol position updating
                Expression type = (dataTypeImplicitContext != null) ?
                                  new TypeParser(this).VisitDataTypeOrImplicit(dataTypeImplicitContext, null) :
                                  SymbolType.Auto.AsNewSymbol().UpdateCodePosition(context);

                var paramAssignment = context.list_of_param_assignments();
                string document = CommentParser.Parse(context);
                VisitTypedParameterAssignments(type, paramAssignment, document, generics);
            }

            // Any new parameters
            int newGenericsCount = generics.Count;
            if (originalGenericsCount < newGenericsCount)
            {
                for (int i = originalGenericsCount; i < newGenericsCount; i++)
                {
                    HDLObject generic = generics[i];
                    if (generic is IdentifierDefinition identifierDefinition)
                    {
                        identifierDefinition.IsConstant = true;
                    }
                }
            }
        }
     }
}
