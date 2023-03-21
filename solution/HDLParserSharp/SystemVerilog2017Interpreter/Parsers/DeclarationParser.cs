using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Expressions;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Statement;
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
    public class DeclarationParser : HDLParser
    {
        public DeclarationParser(HDLParser other) : base(other) { }

        /// <summary>
        /// data_declaration:
        ///  ( KW_CONST )? ( KW_VAR ( lifetime )? ( data_type_or_implicit )?
        ///                   | ( lifetime )? data_type_or_implicit
        ///                   ) list_of_variable_decl_assignments SEMI
        ///   | type_declaration
        ///   | package_import_declaration
        ///   | net_type_declaration
        /// ;
        /// </summary>
        public void VisitDataDeclaration(Data_declarationContext context, List<HDLObject> objects)
        {
            var variableDeclContext = context.list_of_variable_decl_assignments();
            if (variableDeclContext != null)
            {
                TypeParser typeParser = new TypeParser(this);
                bool isConstant = context.KW_CONST() != null;
                bool isStatic = typeParser.VisitLifeTime(context.lifetime());
                var dataTypeImplContext = context.data_type_or_implicit();
                Expression dataTypeImplicit = typeParser.VisitDataTypeOrImplicit(dataTypeImplContext, null);
                IEnumerable<IdentifierDefinition> definitions = VisitVariableDeclarationAssignments(variableDeclContext, dataTypeImplicit);
                foreach (var def in definitions)
                {
                    def.IsConstant = isConstant;
                    def.IsStatic = isStatic;
                    objects.Add(def);
                }

                return;
            }

            var typeDeclContext = context.type_declaration();
            if (typeDeclContext != null)
            {
                IdentifierDefinition identifier = VisitTypeDeclaration(typeDeclContext);
                objects.Add(identifier);
                return;
            }

            var packageImportContext = context.package_import_declaration();
            if (packageImportContext != null)
            {
                ImportStatement statement = VisitPackageImportDeclaration(packageImportContext);
                objects.Add(statement); 
                return;
            }

            var netTypeDeclContext = context.net_type_declaration();
            if (netTypeDeclContext != null)
            {
                VisitNetTypeDeclaration(netTypeDeclContext, objects);
                return;
            }

            throw new Exception("Expected a data declaration");
        }

        /// <summary>
        /// package_import_declaration:
        ///     KW_IMPORT package_import_item ( COMMA package_import_item )* SEMI;
        /// package_import_item:
        ///     identifier DOUBLE_COLON ( MUL
        ///                              | identifier
        ///                            );
        /// </summary>
        private ImportStatement VisitPackageImportDeclaration(Package_import_declarationContext context)
        {
            List<Expression> path = new List<Expression>();
            foreach (var importItemContext in context.package_import_item())
            {
                path.AddRange(importItemContext.identifier().Select(i => ExpressionParser.VisitIdentifier(i)));

                // import anyclass::*
                if (importItemContext.MUL() != null)
                {
                    path.Add(SymbolType.All.AsNewSymbol());
                }
            }

            return new ImportStatement(path).UpdateCodePosition(context).UpdateDocument(context, CommentParser);
        }

        /// <summary>
        /// list_of_variable_decl_assignments:
	    ///     variable_decl_assignment ( COMMA variable_decl_assignment )*;
        /// </summary>
        private IEnumerable<IdentifierDefinition> VisitVariableDeclarationAssignments(List_of_variable_decl_assignmentsContext context, Expression baseType)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            TypeParser typeParser = new TypeParser(this);

            bool first = false;
            foreach (var varDeclAssignContext in context.variable_decl_assignment())
            {
                // variable_decl_assignment:
                //     identifier (
                //         ASSIGN ( expression | class_new )
                //         | ( variable_dimension )+ ( ASSIGN ( expression | dynamic_array_new ) )?
                //     )?
                // ;

                var idContext = varDeclAssignContext.identifier();
                string name = ExpressionParser.GetIdentifierString(idContext);
                Expression type = first ? baseType : baseType.Clone();

                var varDimensionContext = varDeclAssignContext.variable_dimension();
                type = typeParser.ApplyVariableDimension(type, varDimensionContext);
                Expression? value = null;
                var exprContext = varDeclAssignContext.expression();
                if (exprContext != null)
                {
                    value = expressionParser.VisitExpression(exprContext);
                }
                else
                {
                    var classNewContext = varDeclAssignContext.class_new();
                    if (classNewContext != null)
                    {
#warning New class declaration is not implemented now
                    }
                    else
                    {
                        var dynamicArrayNewContext = varDeclAssignContext.dynamic_array_new();
                        if (dynamicArrayNewContext != null)
                        {
#warning New dynamic array is not implemented now
                        }
                    }
                }

                yield return new IdentifierDefinition(name, type, value)
                    .UpdateCodePosition(varDeclAssignContext);
            }
        }

        /// <summary>
        /// type_declaration:
        ///     KW_TYPEDEF (
        ///         data_type identifier ( variable_dimension )*
        ///         | ( KW_ENUM
        ///             | KW_STRUCT
        ///             | KW_UNION
        ///             | identifier_with_bit_select DOT identifier
        ///             | ( KW_INTERFACE )? KW_CLASS
        ///           )? identifier
        ///     ) SEMI;
        /// </summary>
        private IdentifierDefinition VisitTypeDeclaration(Type_declarationContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            TypeParser typeParser = new TypeParser(this);

            var firstIdContext = context.identifier(0);
            var dataTypeContext = context.data_type();

            Symbol typeSymbol = SymbolType.Type.AsNewSymbol();
            string name;
            Expression value;

            if (dataTypeContext != null) 
            { 
                Expression dataType = typeParser.VisitDataType(dataTypeContext);
                var varDimensionContext = context.variable_dimension();
                dataType = typeParser.ApplyVariableDimension(dataType, varDimensionContext);
                name = ExpressionParser.GetIdentifierString(firstIdContext);
                value = dataType;
            }
            else if ((context.KW_ENUM() != null) || (context.KW_STRUCT() != null) ||
                (context.KW_UNION() != null) || (context.KW_CLASS() != null))
            {
                // Forward typedef without actual type specified
                name = ExpressionParser.GetIdentifierString(firstIdContext);
                value = SymbolType.Null.AsNewSymbol();
            }
            else
            {
                var idWithBitSelectContext = context.identifier_with_bit_select();
                value = expressionParser.VisitIdentifierWithBitSelect(idWithBitSelectContext, null);

                var idContext = context.identifier();
                if (idContext.Length != 2)
                {
                    throw new Exception("Identifier with bit select must be A.B");
                }

                var identifier = ExpressionParser.VisitIdentifier(firstIdContext);
                value = new Operator(OperatorType.Dot, value, identifier)
                        .UpdateCodePosition(idWithBitSelectContext);
                name = ExpressionParser.GetIdentifierString(idContext.Last());
            }

            return new IdentifierDefinition(name, typeSymbol, value)
                .UpdateCodePosition(context)
                .UpdateDocument(context, CommentParser);
        }

        /// <summary>
        /// net_type_declaration:
        ///     KW_NETTYPE ( data_type identifier ( KW_WITH package_or_class_scoped_id )? ) SEMI;

        /// </summary>
        /// <param name="context"></param>
        /// <param name="objects"></param>
        private void VisitNetTypeDeclaration(Net_type_declarationContext context, List<HDLObject> objects)
        {
#warning Net type declaration is not implemented now
        }
    }
}
