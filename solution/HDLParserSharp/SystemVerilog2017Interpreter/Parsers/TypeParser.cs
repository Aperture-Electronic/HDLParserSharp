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
using SystemVerilog2017Interpreter.Types;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class TypeParser : HDLParser
    {
        public TypeParser(HDLParser other) : base(other) 
        { 
        
        }    

        public Expression VisitTypeReference(Type_referenceContext context)
        {
            // type_reference:
            //     KW_TYPE LPAREN (
            //         expression
            //         | data_type
            //     ) RPAREN
            // ;
            Expression expression;
            var exprContext = context.expression();
            if (exprContext != null)
            {
                ExpressionParser expressionParser = new ExpressionParser(this);
                expression = expressionParser.VisitExpression(exprContext);
            }
            else
            {
                var dataTypeContext = context.data_type();
                if (dataTypeContext == null)
                {
                    throw new Exception("Excepted a data type");
                }

                expression = VisitDataType(dataTypeContext);
            }

            return new Operator(OperatorType.TypeOf, expression);
        }

        public Expression VisitIntegerType(Integer_typeContext context)
        {
            var intVectorType = context.integer_vector_type();
            if (intVectorType != null)
            {
                return VisitIntegerVectorType(intVectorType);
            }

            var intAtomType = context.integer_atom_type();
            if (intAtomType != null)
            {
                return VisitIntegerAtomType(intAtomType);
            }

            throw new Exception("Excepted integer type");
        }

        /// <summary>
        ///  non_integer_type:
        ///     KW_SHORTREAL
        ///     | KW_REAL
        ///     | KW_REALTIME
        ///  ;
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Expression VisitNonIntegerType(Non_integer_typeContext context)
            => new Identifier(context.GetText()).UpdateCodePosition(context);

        /// <summary>
        /// // integer_atom_type:
        ///     KW_BYTE
        ///     | KW_SHORTINT
        ///     | KW_INT
        ///     | KW_LONGINT
        ///     | KW_INTEGER
        ///     | KW_TIME
        /// ;
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Expression VisitIntegerAtomType(Integer_atom_typeContext context)
            => new Identifier(context.GetText()).UpdateCodePosition(context);

        /// <summary>
        /// nteger_vector_type:
        ///     KW_BIT
        ///     | KW_LOGIC
        ///     | KW_REG
        /// ;
        /// </summary>
        /// <param name="intVectorType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Expression VisitIntegerVectorType(Integer_vector_typeContext context)
        {
            Identifier identifier = new Identifier(context.GetText()).UpdateCodePosition(context);
            return identifier.AssignWireType(context, SymbolType.Null.AsNewSymbol(), Types.SigningValue.NoSign);
        }

        public Expression VisitDataTypePrimitive(Data_type_primitiveContext context)
        {
            var intTypeContext = context.integer_type();
            if (intTypeContext == null)
            {
                var nonIntTypeContext = context.non_integer_type();
                if (nonIntTypeContext != null)
                {
                    return VisitNonIntegerType(nonIntTypeContext);
                }

                throw new Exception("Excepted integer or non-integer type primitive");
            }

            var intType = VisitIntegerType(intTypeContext);
            // TODO: wire in correct format (parametrization <t, width, sign>)
            var signingContext = context.signing();

            if (signingContext != null)
            {
                SigningValue signing = VisitSigning(signingContext);
                if (signing != SigningValue.NoSign)
                {
                    var sign = ExpressionExtension.Signing(signing);

                    if (intType is Operator op)
                    {
                        if ((op.Type == OperatorType.Parametrization) && (op.Operands.Count == 3))
                        {
                            // Fill up sign flag for wire/reg types
                            op.Operands[2] = sign;
                        }
                    }
                    else
                    {
                        // Specify sign for rest of the types
                        List<Expression> arguments = new List<Expression>
                        {
                            new Operator(OperatorType.MapAssociation, new Identifier("signed"), sign).
                                UpdateCodePosition(context)
                        };

                        intType = Operator.Parametrization(intType, arguments).
                                  UpdateCodePosition(context);
                    }
                }
            }

            return intType;
        }

        public Expression VisitDataType(Data_typeContext context)
        {
            // data_type:
            //     KW_STRING
            //     | KW_CHANDLE
            //     | KW_VIRTUAL ( KW_INTERFACE )? identifier ( parameter_value_assignment )? ( DOT identifier )?
            //     | KW_EVENT
            //     | ( data_type_primitive
            //         | KW_ENUM ( enum_base_type )?
            //            LBRACE enum_name_declaration  ( COMMA enum_name_declaration )* RBRACE
            //         | struct_union ( KW_PACKED ( signing )? )?
            //             LBRACE ( struct_union_member )+ RBRACE
            //         | package_or_class_scoped_path
            //       ) ( variable_dimension )*
            //     | type_reference
            // ;

            var stringContext = context.KW_STRING();
            if (stringContext != null)
            {
                return new Identifier("string").UpdateCodePosition(context);
            }

            var cHandleContext = context.KW_CHANDLE();
            if (cHandleContext != null)
            {
                return new Identifier("chandle").UpdateCodePosition(context);
            }

            if (context.KW_VIRTUAL() != null)
            {
#warning Virtual type is not implemented now
                var idContext = context.identifier();
                Expression idExpression = ExpressionParser.VisitIdentifier(idContext.First());
                ExpressionParser expressionParser = new ExpressionParser(this);
                var paraValueAssignContext = context.parameter_value_assignment();
                if (paraValueAssignContext != null)
                {
                    var paraContext = expressionParser.VisitParameterValueAssignment(paraValueAssignContext);
                    idExpression = Operator.Parametrization(idExpression, paraContext).
                                   UpdateCodePosition(context);
                }

                if (idContext.Length == 2)
                {
                    var identifier = ExpressionParser.VisitIdentifier(idContext.Last());
                    idExpression = new Operator(OperatorType.Dot, idExpression, identifier);
                }
                else if (idContext.Length != 1)
                {
                    throw new Exception("Invalid multiple data type");
                }

                return idExpression;
            }

            var eventContext = context.KW_EVENT();
            if (eventContext != null)
            {
                return new Identifier("event").UpdateCodePosition(eventContext);
            }

            var typeRefContext = context.type_reference();
            if (typeRefContext != null)
            {
                return VisitTypeReference(typeRefContext);
            }

            Expression expression;
            var dTypePrimitive = context.data_type_primitive();
            if (dTypePrimitive != null)
            {
                expression = VisitDataTypePrimitive(dTypePrimitive);
            }
            else if (context.KW_ENUM() != null)
            {
#warning Enum type is not implemented now
                expression = new NotImplemented("enum").UpdateCodePosition(context);
            }
            else if (context.struct_union() != null)
            {
                expression = new NotImplemented("union").UpdateCodePosition(context);
            }
            else
            {
                ExpressionParser expressionParser = new ExpressionParser(this);
                var pkgClassScoped = context.package_or_class_scoped_path();
                if (pkgClassScoped == null)
                {
                    throw new Exception("Expceted a package or class scoped path");   
                }

                expression = expressionParser.VisitPackageOrClassScopedPath(pkgClassScoped);
            }

            TypeParser typeParser = new TypeParser(this);
            var varDimension = context.variable_dimension();
            
            return typeParser.ApplyVariableDimension(expression, varDimension);
        }

        public Expression VisitDataTypeOrImplicit(Data_type_or_implicitContext context, Expression? netType)
        {
            if (context == null)
            {
                return SymbolType.Auto.AsNewSymbol();
            }

            var dataTypeContext = context.data_type();
            if (dataTypeContext != null)
            {
                return VisitDataType(dataTypeContext);
            }

            var implDataTypeContext = context.implicit_data_type();
            if (implDataTypeContext != null)
            {
                return VisitImplicitDataType(implDataTypeContext, netType);
            }

            throw new Exception("Excepted a data type or implicit type");
        }

        public Expression VisitUnpackedDimension(Unpacked_dimensionContext context)
        {
            // unpacked_dimension: LSQUARE_BR range_expression RSQUARE_BR;
            var rangeExprContext = context.range_expression();
            return new ExpressionParser(this).VisitRangeExpression(rangeExprContext)
                .UpdateCodePosition(context);
        }

        public Expression ApplyUnpakcedDimension(Expression baseExpression, IEnumerable<Unpacked_dimensionContext> contexts)
        {
            foreach (var context in contexts)
            {
                var upkDimension = VisitUnpackedDimension(context);
                baseExpression = new Operator(OperatorType.Index, baseExpression, upkDimension)
                    .UpdateCodePosition(context);
            }

            return baseExpression;
        }

        public Expression ApplyVariableDimension(Expression baseExpression, IEnumerable<Variable_dimensionContext> contexts)
        {
            // This function in hdlConvertor has a bug
            // that when we write a 2-D port like
            // logic [msb:lsb] port_name [lsb2d:msb2d]
            //       \-- 1D --/          \--- 2D ---/
            // the expression of 2D dimension will overwrite on the 1D dimension
            // on operands[1]
            // We review the code, see the program write the dimension result fixedly on operands[1]
            // And, the program add a empty operand to the parameterize operator
            // So, we must concat the 1D dimension and 2D dimension with a new operator

            Operator? op = null;
            if (baseExpression is Operator paraOp)
            {
                if ((paraOp.Type == OperatorType.Parametrization) && 
                    (paraOp.Operands.Count == 3)) 
                {
                    op = paraOp;
                }
                else if (paraOp.Operands[1] is Symbol symb)
                {
                    if (symb.Type != SymbolType.Null)
                    {
                        op = paraOp;
                    }
                }
            }

            foreach (var context in contexts)
            {
                if (op != null)
                {
                    Expression? dim = VisitVariableDimension(context);
                    dim ??= SymbolType.Null.AsNewSymbol();
                    // Here is our enhancement.
                    // When the operand is NULL, it means there is no dimension operand here
                    // But if not, we need combine the operand and new dimension into a new operator
                    if (op.Operands[1] is Symbol nullSymbol)
                    {
                        // First assign the dimension operand
                        if (nullSymbol.Type == SymbolType.Null)
                        {
                            op.Operands[1] = dim;
                        }
                    }
                    else
                    {
                        // Combine the new dimension operand
                        Operator multiDimensionOperator = new Operator(OperatorType.MultiDimension,
                            op.Operands[1], dim);
                        op.Operands[1] = multiDimensionOperator;
                    }
                    
                    op = null;
                    continue;
                }

                baseExpression = VisitVariableDimension(context, baseExpression);
            }

            return baseExpression;
        }

        public Expression? VisitPackedDimension(Packed_dimensionContext context)
        {
            var rangeExpression = context.range_expression();
            if (rangeExpression != null)
            {
                return new ExpressionParser(this).VisitRangeExpression(rangeExpression);
            }

            return null;
        }

        internal SigningValue VisitSigning(SigningContext signingContext)
        {
            if (signingContext.KW_SIGNED() != null)
            {
                return SigningValue.Signed;
            }
            else if (signingContext.KW_UNSIGNED() != null)
            {
                return SigningValue.Unsigned;
            }

            throw new Exception("Excepted signed/unsigned identifier");
        }

        private Expression VisitImplicitDataType(Implicit_data_typeContext implDataTypeContext, Expression? netType)
        {
            // implicit_data_type:
            //     signing ( packed_dimension )*
            //     | ( packed_dimension )+
            // ;
            if (implDataTypeContext == null)
            {
                if (netType != null)
                {
                    return netType;
                }
                else
                {
                    return SymbolType.Auto.AsNewSymbol();
                }
            }

            var signingContext = implDataTypeContext.signing();
            SigningValue signing = (signingContext != null) ? VisitSigning(signingContext) : SigningValue.NoSign;
            var packedDimContext = implDataTypeContext.packed_dimension();
            Expression expression;
            var firstDim = packedDimContext.First();
            if (firstDim != packedDimContext.Last())
            {
                Expression packedDim = VisitPackedDimension(firstDim) ?? SymbolType.Null.AsNewSymbol();
                expression = netType.AssignWireType(firstDim, packedDim, signing);
            }
            else
            {
                expression = ((Expression?)null).AssignWireType(null, SymbolType.Null.AsNewSymbol(), signing);
            }

            foreach (var dimContext in packedDimContext[1..])
            {
                Expression? packedDim = VisitPackedDimension(dimContext);
                expression = packedDim != null ?
                    new Operator(OperatorType.Index, expression, packedDim).UpdateCodePosition(dimContext) :
                    new Operator(OperatorType.Index, expression).UpdateCodePosition(dimContext);
            }

            return expression;
        }

        public Expression? VisitVariableDimension(Variable_dimensionContext context)
        {
            // variable_dimension:
            //     LSQUARE_BR ( MUL
            //              | data_type
            //              | array_range_expression
            //               )? RSQUARE_BR
            // ;
            if (context.MUL() != null)
            {
#warning Associative array is not implemented now
                return null;
            }

            Expression? index = null;
            var dataTypeContext = context.data_type();
            if (dataTypeContext != null)
            {
                index = new TypeParser(this).VisitDataType(dataTypeContext);
            }
            else
            {
                var arrRangeExprContext = context.array_range_expression();
                if (arrRangeExprContext != null)
                {
                    index = new ExpressionParser(this).VisitArrayRangeExpression(arrRangeExprContext);
                }
            }

            return index;
        }

        private Expression VisitVariableDimension(Variable_dimensionContext context, Expression selectedName)
        {
            Expression? index = VisitVariableDimension(context);
            return index == null
                ? new Operator(OperatorType.Index, selectedName).UpdateCodePosition(context)
                : (Expression)new Operator(OperatorType.Index, selectedName, index).UpdateCodePosition(context);
        }

        /// <summary>
        /// 	// net_type:
        ///     KW_SUPPLY0
        ///     | KW_SUPPLY1
        ///     | KW_TRI
        ///     | KW_TRIAND
        ///     | KW_TRIOR
        ///     | KW_TRIREG
        ///     | KW_TRI0
        ///     | KW_TRI1
        ///     | KW_UWIRE
        ///     | KW_WIRE
        ///     | KW_WAND
        ///     | KW_WOR
        /// ;
        /// </summary>
        public Expression VisitNetType(Net_typeContext context)
            => new Identifier(context.GetText()).UpdateCodePosition(context);

        public Expression VisitNetPortType(Net_port_typeContext? context)
        {
            if (context == null)
            {
                return SymbolType.Auto.AsNewSymbol();
            }

            if (context.KW_INTERCONNECT() != null)
            {
#warning Interconnect type is not implemented now
                var implDataType = context.implicit_data_type();
                return VisitImplicitDataType(implDataType, null);
            }

            Expression? netType = null;
            var netTypeContext = context.net_type();
            if (netTypeContext != null)
            {
                netType = VisitNetType(netTypeContext);
            }

            var dataTypeImplicitContext = context.data_type_or_implicit();
            return VisitDataTypeOrImplicit(dataTypeImplicitContext, netType);
        }

        public (Expression, bool isLatched) VisitNetOrVariableDataType(Net_or_var_data_typeContext? context)
        {
            // net_or_var_data_type:
            //  KW_INTERCONNECT ( implicit_data_type )?
            //   | KW_VAR ( data_type_or_implicit )?
            //   | net_type ( data_type_or_implicit )?
            //   | data_type_or_implicit
            // ;
            if (context == null)
            {
                return (SymbolType.Auto.AsNewSymbol(), false);
            }

            if (context.KW_INTERCONNECT() != null)
            {
#warning Interconnect type is not implemented now
                var implDataType = context.implicit_data_type();
                return (VisitImplicitDataType(implDataType, null), false);
            }

            var dataTypeImplicitContext = context.data_type_or_implicit();
            if (context.KW_VAR() != null)
            {
                return (VisitDataTypeOrImplicit(dataTypeImplicitContext, null), true);
            }

            var netTypeContext = context.net_type();
            if (netTypeContext != null)
            {
                var subType = VisitNetType(netTypeContext);
                return (VisitDataTypeOrImplicit(dataTypeImplicitContext, subType), false);
            }

            if (dataTypeImplicitContext == null)
            {
                throw new Exception("Invalid type or implicit in context");
            }

            return (VisitDataTypeOrImplicit(dataTypeImplicitContext, null), false);
        }

        public bool VisitLifeTime(LifetimeContext? context)
            => (context != null) && (context.KW_STATIC() != null);

        public Expression VisitDataTypeOrVoid(Data_type_or_voidContext context)
        {
            // data_type_or_void:
            //    KW_VOID
            //    | data_type
            // ;
            if (context.KW_VOID() != null)
            {
                return new Identifier("void").UpdateCodePosition(context);
            }
            else
            {
                var dataType = context.data_type();
                return VisitDataType(dataType);
            }
        }

        public Expression VisitVaribaleDataType(Var_data_typeContext? context)
        {
            // var_data_type:
            //   KW_VAR ( data_type_or_implicit )?
            //    | data_type
            // ;
            if (context == null)
            {
                return SymbolType.Auto.AsNewSymbol();
            }

            var dataTypeImplicitContext = context.data_type_or_implicit();
            if (dataTypeImplicitContext != null)
            {
                return VisitDataTypeOrImplicit(dataTypeImplicitContext, null);
            }

            var dataTypeContext = context.data_type();
            if (dataTypeContext != null)
            {
                return VisitDataType(dataTypeContext);
            }

            if (context.KW_VAR() != null)
            {
                return SymbolType.Auto.AsNewSymbol();
            }

            throw new Exception("Excepted a variable data type");
        }

        public Expression VisitFunctionDataTypeOrImplicit(Function_data_type_or_implicitContext? context)
        {
            if (context == null)
            {
                return SymbolType.Auto.AsNewSymbol();
            }

            var dataTypeVoidContext = context.data_type_or_void();
            if (dataTypeVoidContext != null)
            {
                return VisitDataTypeOrVoid(dataTypeVoidContext);
            }
            else
            {
                var implDataTypeContext = context.implicit_data_type();
                if (implDataTypeContext != null)
                {
                    return VisitImplicitDataType(implDataTypeContext, null);
                }
            }

            throw new Exception("Excepted a function data type");
        }
    }
}
