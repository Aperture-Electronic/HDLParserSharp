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
    public class ExpressionPrimaryParser : HDLParser
    {
        public ExpressionPrimaryParser(HDLParser other) : base(other) { }

        public Expression VisitPrimary(PrimaryContext context)
        {
            #region Primary expression comment
            // primary:
            //     primary_literal                               #primaryLit
            //     | package_or_class_scoped_path                #primaryPath
            //     | LPAREN mintypmax_expression RPAREN          #primaryPar
            //     | ( KW_STRING
            //         | KW_CONST
            //         | integer_type
            //         | non_integer_type
            //         | signing
            //         ) APOSTROPHE LPAREN expression RPAREN     #primaryCast
            //     | primary APOSTROPHE LPAREN expression RPAREN #primaryCast2
            //     | primary bit_select                          #primaryBitSelect
            //     | primary DOT identifier                      #primaryDot
            //     | primary LSQUARE_BR array_range_expression RSQUARE_BR #primaryIndex
            //     | concatenation                                        #primaryConcat
            //     | streaming_concatenation                              #primaryStreaming_concatenation
            //     | any_system_tf_identifier ( LPAREN data_type COMMA list_of_arguments
            //          ( COMMA clocking_event )? RPAREN
            //          | LPAREN list_of_arguments COMMA clocking_event  RPAREN
            //          )?                                       #primaryTfCall
            //     | ( KW_STD DOUBLE_COLON )?  randomize_call    #primaryRandomize
            //     | primary DOT randomize_call                  #primaryRandomize2
            //     /*| let_expression                            #primaryLet (same as call)*/
            //     | assignment_pattern_expression               #primaryAssig
            //     | type_reference                              #primaryTypeRef
            //     | primary ( DOT array_method_name )? ( attribute_instance )*
            //                   LPAREN ( list_of_arguments )? RPAREN
            //                   ( KW_WITH LPAREN expression RPAREN )? #primaryCall
            //     | primary DOT array_method_name               # primaryCallArrayMethodNoArgs
            //     | primary ( DOT array_method_name )? ( attribute_instance )*
            //              KW_WITH LPAREN expression RPAREN     #primaryCallWith
            // ;
            #endregion

            if (context is PrimaryLitContext litContext)
            {
                return LiteralParser.VisitPrimaryLiteral(litContext.primary_literal());
            }

            if (context is PrimaryPathContext pathContext)
            {
                return new ExpressionParser(this)
                    .VisitPackageOrClassScopedPath(pathContext.package_or_class_scoped_path());
            }

            if (context is PrimaryParContext mintypmaxContext)
            {
                return new ExpressionParser(this)
                    .VisitMintypmaxExpression(mintypmaxContext.mintypmax_expression());
            }

            if (context is PrimaryCastContext castContext)
            {
                return VisitPrimaryCast(castContext);
            }

            if (context is PrimaryCast2Context cast2Context)
            {
                return VisitPrimaryCast2(cast2Context);
            }

            if (context is PrimaryBitSelectContext bitSelectContext)
            {
                return VisitPrimaryBitSelect(bitSelectContext);
            }

            if (context is PrimaryDotContext dotContext)
            {
                return VisitPrimaryDot(dotContext);
            }

            if (context is PrimaryIndexContext indexContext)
            {
                return VisitPrimaryIndex(indexContext);
            }

            if(context is PrimaryConcatContext concatContext)
            {
                return VisitPrimaryConcat(concatContext);
            }

            if (context is PrimaryStreaming_concatenationContext streamConcatContext)
            {
                return VisitPrimaryStreamingConcatenation(streamConcatContext);
            }

            if (context is PrimaryTfCallContext taskFunctionCallContext)
            {
                return VisitPrimaryTaskFunctionCall(taskFunctionCallContext);
            }

            if (context is PrimaryRandomizeContext randomizeContext)
            {
                return VisitPrimaryRandomize(randomizeContext);
            }

            if (context is PrimaryRandomize2Context randomize2Context)
            {
                return VisitPrimaryRandomize2(randomize2Context); 
            }

            if (context is PrimaryAssigContext assignContext)
            {
                return VisitPrimaryAssign(assignContext);
            }

            if (context is PrimaryTypeRefContext typeReferenceContext)
            {
                return VisitPrimaryTypeReference(typeReferenceContext);
            }

            if (context is PrimaryCallContext callContext)
            {
                return VisitPrimaryCall(callContext);
            }

            if (context is PrimaryCallArrayMethodNoArgsContext callArrayMethodNoArgsContext)
            {
                return VisitPrimaryCallArrayMethodNoArgs(callArrayMethodNoArgsContext); 
            }

            if (context is PrimaryCallWithContext callWithContext)
            {
                return VisitPrimaryCallWith(callWithContext);
            }

            throw new Exception("Unexpected primary expression item in this context");
        }

        /// <summary>
        ///     | primary ( DOT array_method_name )? ( attribute_instance )*
	    ///              KW_WITH LPAREN expression RPAREN     #PrimaryCallWith
        /// </summary>
        private Expression VisitPrimaryCallWith(PrimaryCallWithContext context)
        {
            var primaryContext = context.primary();
            Expression primary = VisitPrimary(primaryContext);

            var arrayMethodNameContext = context.array_method_name();
            if (arrayMethodNameContext != null)
            {
#warning Array method name is not implemented now
            }

#warning Visit the attriubte, but the attribute instance is not implemented now
            AttributeParser.VisitAttributeInstance(context.attribute_instance());

            if (context.KW_WITH() != null)
            {
#warning With primary call is not implemented now
            }

            // No arguments
            return Operator.Call(primary, new List<Expression>()).UpdateCodePosition(context);
        }

        /// <summary>
        ///     | primary DOT array_method_name               #PrimaryCallArrayMethodNoArgs 
        /// </summary>
        private Expression VisitPrimaryCallArrayMethodNoArgs(PrimaryCallArrayMethodNoArgsContext context)
        {
            var primaryContext = context.primary();
            Expression primary = VisitPrimary(primaryContext);

            var arrayMethodNameContext = context.array_method_name();
            if (arrayMethodNameContext != null)
            {
#warning Array method name is not implemented now
            }

            // No arguments
            return Operator.Call(primary, new List<Expression>()).UpdateCodePosition(context);
        }

        /// <summary>
        ///     | primary ( DOT array_method_name )? ( attribute_instance )*
        ///                   LPAREN ( list_of_arguments )? RPAREN
        ///                   ( KW_WITH LPAREN expression RPAREN )? #PrimaryCall
        /// </summary>
        private Expression VisitPrimaryCall(PrimaryCallContext context)
        {
            var primaryContext = context.primary();
            Expression primary = VisitPrimary(primaryContext);

            var arrayMethodNameContext = context.array_method_name();
            if (arrayMethodNameContext != null)
            {
#warning Array method name is not implemented now
            }

#warning Visit the attriubte, but the attribute instance is not implemented now
            AttributeParser.VisitAttributeInstance(context.attribute_instance());

            if (context.KW_WITH() != null)
            {
#warning With primary call is not implemented now
            }

            ExpressionParser expressionParser = new ExpressionParser(this);
            List<Expression> arguments = new List<Expression>();
            var argumentsContext = context.list_of_arguments();
            if (argumentsContext != null)
            {
                expressionParser.VisitArguments(argumentsContext, arguments);
                return Operator.Call(primary, arguments).UpdateCodePosition(context);
            }

            return primary;
        }

        /// <summary>
        ///     | type_reference                              #PrimaryTypeRef 
        /// </summary>
        private Expression VisitPrimaryTypeReference(PrimaryTypeRefContext context)
        {
            var typeRefContext = context.type_reference();
            return new TypeParser(this).VisitTypeReference(typeRefContext);
        }
        
        /// <summary>
        ///     | assignment_pattern_expression               #PrimaryAssig
	    /// assignment_pattern_expression:
	    ///  ( assignment_pattern_expression_type )? assignment_pattern;
        /// </summary>
        private Expression VisitPrimaryAssign(PrimaryAssigContext context)
        {
            var assignmentPatternExprContext = context.assignment_pattern_expression();
            if (assignmentPatternExprContext.assignment_pattern_expression_type() != null)
            {
#warning Assignment pattern expression type is not implemented now
            }

            return VisitAssignmentPattern(assignmentPatternExprContext.assignment_pattern());
        }

        /// <summary>
        /// assignment_pattern:
        ///     APOSTROPHE_LBRACE (
        ///         expression ( COMMA expression )*
        ///         | structure_pattern_key COLON expression
        ///            ( COMMA structure_pattern_key COLON expression )*
        ///         | array_pattern_key COLON expression
        ///            ( COMMA array_pattern_key COLON expression )*
        ///         | constant_expression LBRACE expression ( COMMA expression )* RBRACE
        ///     )? RBRACE;
        /// </summary>
        private Expression VisitAssignmentPattern(Assignment_patternContext context)
        {
            List<Expression> keys = new List<Expression>();
            List<Expression> expressions = new List<Expression>();
            ExpressionParser expressionParser = new ExpressionParser(this);
            foreach (var expr in context.expression())
            {
                expressions.Add(expressionParser.VisitExpression(expr));
            }

            var structPatternKeyContext = context.structure_pattern_key();
            if (structPatternKeyContext?.Length > 0)
            {
                keys.AddRange(structPatternKeyContext.Select(sp => VisitStructurePatternKey(sp)));
            }
            else
            {
                var arrayPatternKeyContext = context.array_pattern_key();
                if (arrayPatternKeyContext?.Length > 0)
                {
                    keys.AddRange(arrayPatternKeyContext.Select(ap => VisitArrayPatternKey(ap)));
                }
            }

            var constExprContext = context.constant_expression();
            if (constExprContext != null)
            {
#warning Constant expression assignment pattern is not implemented now
                return new NotImplemented().UpdateCodePosition(context);
            }
            
            if (keys.Any())
            {
                // Map keys with items
                if (keys.Count != expressions.Count)
                {
                    throw new Exception("The count of key and expression is not matched");
                }

                var associations =
                    keys.Zip(expressions, (key, expr) => (new Operator(OperatorType.MapAssociation, key, expr)) as Expression);

                expressions = associations.ToList();
            }

            return new HDLAbstractSyntaxTree.Value.Array(expressions);
        }

        /// <summary>
        /// array_pattern_key:
        ///  constant_expression
        ///   | assignment_pattern_key
        /// ;
        /// </summary>
        private Expression VisitArrayPatternKey(Array_pattern_keyContext context)
        {
            var constantExprContext = context.constant_expression();
            if (constantExprContext != null)
            {
                return new ExpressionParser(this).VisitConstantExpression(constantExprContext);
            }
            else
            {
                var assignPatternKeyContext = context.assignment_pattern_key();
                if (assignPatternKeyContext != null)
                {
                    return VisitAssignmentPatternKey(assignPatternKeyContext);
                }
            }

            throw new Exception("Excepted an assignment pattern key (constant or expression)");
        }

        /// <summary>
        /// assignment_pattern_key:
        ///  KW_DEFAULT
        ///   | integer_type
        ///   | non_integer_type
        ///   | package_or_class_scoped_path
        /// ;
        /// </summary>
        private Expression VisitAssignmentPatternKey(Assignment_pattern_keyContext context)
        {
            if (context.KW_DEFAULT() != null)
            {
                return new Identifier("default");
            }

            var intTypeContext = context.integer_type();
            if (intTypeContext != null)
            {
#warning Integer type of assignment pattern key is not implemented now
                return new NotImplemented().UpdateCodePosition(context);
            }

            var nonIntTypeContext = context.non_integer_type();
            if (nonIntTypeContext != null)
            {
#warning Non-integer type of assignment pattern key is not implemented now
                return new NotImplemented().UpdateCodePosition(context);
            }

            var packageClassScopedContext = context.package_or_class_scoped_path();
            if (packageClassScopedContext != null)
            {
                return new ExpressionParser(this).VisitPackageOrClassScopedPath(packageClassScopedContext);
            }

            throw new Exception("Excepted an assignment pattern key (default, int or non-int type, or package/class)");
        }

        /// <summary>
        /// structure_pattern_key:
        ///  identifier
        ///   | assignment_pattern_key
        /// ;
        /// </summary>
        private Expression VisitStructurePatternKey(Structure_pattern_keyContext context)
        {
            var idContext = context.identifier();
            if (idContext != null)
            {
                return ExpressionParser.VisitIdentifier(idContext);
            }
            else
            {
                return VisitAssignmentPatternKey(context.assignment_pattern_key());
            }
        }

        /// <summary>
        ///     | primary DOT randomize_call                  #PrimaryRandomize2
        /// </summary>
        private Expression VisitPrimaryRandomize2(PrimaryRandomize2Context context)
        {
#warning Randomize2 is not implemented now
            return new NotImplemented().UpdateCodePosition(context);
        }

        /// <summary>
        ///     | ( KW_STD DOUBLE_COLON )?  randomize_call    #PrimaryRandomize
        /// </summary>
        private Expression VisitPrimaryRandomize(PrimaryRandomizeContext context)
        {
#warning Randomize is not implemented now
            return new NotImplemented().UpdateCodePosition(context);
        }

        /// <summary>
        ///     | any_system_tf_identifier ( LPAREN data_type COMMA list_of_arguments
        ///          ( COMMA clocking_event )? RPAREN
        ///          | LPAREN list_of_arguments ( COMMA clocking_event )?  RPAREN
        ///          )?    
        /// </summary>
        private Expression VisitPrimaryTaskFunctionCall(PrimaryTfCallContext context)
        {
            // System functions like $clog2 (prefix $)
            var anySystemTFIdContext = context.any_system_tf_identifier();
            var systemTFIdentifier = LiteralParser.VisitAnySystemTaskFunctionIdentifier(anySystemTFIdContext);

            ExpressionParser expressionParser = new ExpressionParser(this);
            TypeParser typeParser = new TypeParser(this);
            List<Expression> arguments = new List<Expression>();
            var dataTypeContext = context.data_type();
            if (dataTypeContext != null )
            {
                arguments.Add(typeParser.VisitDataType(dataTypeContext));   
            }

            var argumentsContext = context.list_of_arguments();
            if (argumentsContext!= null )
            {
                expressionParser.VisitArguments(argumentsContext, arguments);
            }

            var clockingEventContext = context.clocking_event();
            if (clockingEventContext != null )
            {
#warning Clocking event is not implemented now
            }

            return Operator.Call(systemTFIdentifier, arguments).UpdateCodePosition(context);
        }

        /// <summary>
        ///     | streaming_concatenation                              #PrimaryStreaming_concatenation 
        /// </summary>
        /// <param name="streamConcatContext"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private Expression VisitPrimaryStreamingConcatenation(PrimaryStreaming_concatenationContext context)
        {
#warning Primary streaming concatenation is not implemented now
            return new NotImplemented().UpdateCodePosition(context);
        }

        /// <summary>
        ///     | concatenation                                        #PrimaryConcat
        /// </summary>
        private Expression VisitPrimaryConcat(PrimaryConcatContext context)
        {
            var concatenationContext = context.concatenation();
            return new ExpressionParser(this).VisitConcatenation(concatenationContext);
        }

        /// <summary>
        /// | primary LSQUARE_BR array_range_expression RSQUARE_BR #PrimaryIndex
        /// </summary>
        private Expression VisitPrimaryIndex(PrimaryIndexContext context)
        {
            var primaryContext = context.primary();
            var arrayRangeContext = context.array_range_expression();
            Expression primary = VisitPrimary(primaryContext);
            Expression arrayRange = new ExpressionParser(this).VisitArrayRangeExpression(arrayRangeContext);    
            return new Operator(OperatorType.Index, primary, arrayRange)
                .UpdateCodePosition(primaryContext);
        }

        /// <summary>
        ///     | primary DOT identifier                      #PrimaryDot
        /// </summary>
        private Expression VisitPrimaryDot(PrimaryDotContext context)
        {
            var primaryContext = context.primary();
            var idContext = context.identifier();
            Expression primary = VisitPrimary(primaryContext);
            Expression identifier = ExpressionParser.VisitIdentifier(idContext);
            return new Operator(OperatorType.Dot, primary, identifier)
                .UpdateCodePosition(primaryContext);
        }

        /// <summary>
        ///     | primary bit_select                          #PrimaryBitSelect
        /// </summary>
        private Expression VisitPrimaryBitSelect(PrimaryBitSelectContext context)
        {
            var primaryContext = context.primary();
            var bitSelectContext = context.bit_select();
            Expression primary = VisitPrimary(primaryContext);
            return new ExpressionParser(this).VisitBitSelect(bitSelectContext, primary);
        }

        /// <summary>
        ///     | primary APOSTROPHE LPAREN expression RPAREN #PrimaryCast2
        /// </summary>
        private Expression VisitPrimaryCast2(PrimaryCast2Context context)
        {
            var primaryContext = context.primary();
            var expressionContext = context.expression();
            Expression primary = VisitPrimary(primaryContext);
            Expression expression = new ExpressionParser(this).VisitExpression(expressionContext);
            return Operator.Call(primary, expression).UpdateCodePosition(context);
        }

        /// <summary>
        ///     | ( KW_STRING
        ///         | KW_CONST
        ///         | integer_type
        ///         | non_integer_type
        ///         | signing
        ///         ) APOSTROPHE LPAREN expression RPAREN     #PrimaryCast
        /// </summary>
        private Expression VisitPrimaryCast(PrimaryCastContext context)
        {
#warning Primary cast is not implemented now
            return new NotImplemented().UpdateCodePosition(context);
        }
    }
}
