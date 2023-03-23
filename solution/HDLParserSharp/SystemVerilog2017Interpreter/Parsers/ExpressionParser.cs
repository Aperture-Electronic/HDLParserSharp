using Antlr4.Runtime.Tree;
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
    public class ExpressionParser : HDLParser
    {
        public ExpressionParser(HDLParser other) : base(other)
        {

        }

        public Expression VisitConstantExpression(Constant_expressionContext context)
            => VisitExpression(context.expression());

        public Expression VisitRangeExpression(Range_expressionContext context)
        {
            var expressionContext = context.expression();
            if (expressionContext.Any())
            {
                var rangeLower = VisitExpression(expressionContext.First());
                if (expressionContext.Length == 1)
                {
                    return rangeLower;
                }
                else if (expressionContext.Length == 2)
                {
                    var rangeHigher = VisitExpression(expressionContext.Last());
                    return new Operator(HDLAbstractSyntaxTree.Types.OperatorType.Downto,
                        rangeLower, rangeHigher).UpdateCodePosition(context);
                }
            }

            throw new Exception("No valid range expression in context");
        }

        public Expression VisitNetLeftValue(Net_lvalueContext context)
        {
            var variableContext = context.variable_lvalue();
            if (variableContext != null)
            {
                return VisitVariableLeftValue(variableContext);
            }

            throw new Exception("No valid left value in context");
        }

        public static Expression VisitPackageScope(Package_scopeContext context)
        {
            var systemCall = context.KW_DOLAR_UNIT();
            if (systemCall != null)
            {
                return new Identifier("$unit").UpdateCodePosition(context);
            }
            else
            {
                var identifier = context.identifier();
                if (identifier != null)
                {
                    return VisitIdentifier(identifier);
                }
            }

            throw new Exception("No valid package scope in context");
        }

        public static Expression VisitPackageScopeIdentifier(Ps_identifierContext context)
        {
            var idContext = context.identifier();
            if (idContext == null)
            {
                throw new Exception("No valid package scope identifier in context");
            }

            var identifier = VisitIdentifier(idContext);
            var psContext = context.package_scope();
            if (psContext != null)
            {
                var packageScope = VisitPackageScope(psContext);
                // Sentences like a::b
                identifier = new Operator(OperatorType.DoubleColon, packageScope, identifier);
            }

            return identifier;
        }

        public Expression VisitArrayRangeExpression(Array_range_expressionContext context)
        {
            var exprContext = context.expression();
            var lowerExpression = VisitExpression(exprContext.First());
            if (exprContext.Length == 1)
            {
                return lowerExpression;
            }
            else if (exprContext.Length != 2) 
            {
                throw new Exception("The array range expression is not allowed over 2 items");
            }
            
            var higherExpression = VisitExpression(exprContext.Last());
            var plusMinusContext = context.operator_plus_minus();
            OperatorType opType = OperatorType.Downto;
            if (plusMinusContext != null)
            {
                // Sentences like [A +: B] or [A -: B]
                // A is the offset and B is the width

                var plusMinus = LiteralParser.VisitOperatorPlusMinus(plusMinusContext);
                opType = plusMinus switch
                {
                    OperatorType.Add => OperatorType.PartSelectPost,
                    _ => OperatorType.PartSelectPre
                };
            }

            return new Operator(opType, lowerExpression, higherExpression).UpdateCodePosition(context); 
        }

        /// <summary>
        /// identifier_doted_index_at_end:
	    ///    identifier ( DOT identifier )?  ( LSQUARE_BR range_expression RSQUARE_BR )*;
        /// </summary>
        public Expression VisitIdentifierDotedIndexAtEnd(Identifier_doted_index_at_endContext context)
        {
            var identifierContext = context.identifier();
            Expression identifier = VisitIdentifier(identifierContext.First());
            if (identifierContext.Length >= 2)
            {
                identifier = identifier.Append(context, OperatorType.Dot, VisitIdentifier(identifierContext[1]));
            }

            var rangeExprContexts = context.range_expression();
            foreach (var rangeExprContext in rangeExprContexts)
            {
                var range = VisitRangeExpression(rangeExprContext);
                identifier = identifier.Append(rangeExprContext, OperatorType.Index, range);
            }

            return identifier;
        }

        /// <summary>
        /// inc_or_dec_expression:
        ///      inc_or_dec_operator ( attribute_instance )* variable_lvalue #Inc_or_dec_expressionPre
        ///     | variable_lvalue ( attribute_instance )* inc_or_dec_operator  #Inc_or_dec_expressionPost
        /// ;
        /// </summary>
        public Expression VisitIncreaseDecreaseExpression(Inc_or_dec_expressionContext context)
        {
            OperatorType op;
            Expression expression;
            if (context is Inc_or_dec_expressionPreContext preContext)
            {
                // ++i or --i
                var operatorContext = preContext.inc_or_dec_operator();
                op = LiteralParser.VisitIncreaseDecreaseOperator(operatorContext, true);
                expression = VisitVariableLeftValue(preContext.variable_lvalue());
#warning Attributes are not implemented now
                AttributeParser.VisitAttributeInstance(preContext.attribute_instance());
            }
            else if (context is Inc_or_dec_expressionPostContext postContext)
            {
                // i++ or i--
                var operatorContext = postContext.inc_or_dec_operator();
                op = LiteralParser.VisitIncreaseDecreaseOperator(operatorContext, false);
                expression = VisitVariableLeftValue(postContext.variable_lvalue());
#warning Attributes are not implemented now
                AttributeParser.VisitAttributeInstance(postContext.attribute_instance());
            }
            else
            {
                throw new Exception("Expected an increase/decrease operator context");
            }

            return new Operator(op, expression).UpdateCodePosition(context);
        }

        /// <summary>
        /// expression:
        ///   primary
        ///   | LPAREN operator_assignment RPAREN
        ///   | KW_TAGGED identifier ( expression )?
        ///   | unary_operator ( attribute_instance )* primary
        ///   | inc_or_dec_expression
        ///   | expression DOUBLESTAR           ( attribute_instance )* expression
        ///   | expression operator_mul_div_mod ( attribute_instance )* expression
        ///   | expression operator_plus_minus  ( attribute_instance )* expression
        ///   | expression operator_shift       ( attribute_instance )* expression
        ///   | expression operator_cmp         ( attribute_instance )* expression
        ///   | expression KW_INSIDE LBRACE open_range_list RBRACE
        ///   | expression operator_eq_neq      ( attribute_instance )* expression
        ///   | expression AMPERSAND            ( attribute_instance )* expression
        ///   | expression operator_xor         ( attribute_instance )* expression
        ///   | expression BAR                  ( attribute_instance )* expression
        ///   | expression AND_LOG              ( attribute_instance )* expression
        ///   | expression OR_LOG               ( attribute_instance )* expression
        ///   | expression ( KW_MATCHES pattern )? TRIPLE_AND expression ( KW_MATCHES pattern )?
        ///   | expression ( KW_MATCHES pattern )? (QUESTIONMARK ( attribute_instance )* expression COLON expression)+ // right associative
        ///   | expression (operator_impl        ( attribute_instance )* expression)+ // right associative
        /// ;
        /// </summary>
        public Expression VisitExpression(ExpressionContext context)
        {
            var opAssignContext = context.operator_assignment();
            if (opAssignContext != null)
            {
#warning Operator assignment is not implemented now
                return new NotImplemented("Operator assignment").UpdateCodePosition(context);
            }

            if (context.KW_TAGGED() != null)
            {
#warning Tagged expression is not implemented now
                return new NotImplemented("Tagged expression").UpdateCodePosition(context);
            }

            var attrInstanceContext = context.attribute_instance();
            foreach (var attributeContext in attrInstanceContext)
            {
#warning Attributes are not implemented now
                AttributeParser.VisitAttributeInstance(attributeContext);
            }

            // Primary expression
            var primaryContext = context.primary();
            if (primaryContext != null)
            {
                ExpressionPrimaryParser primaryParser = new ExpressionPrimaryParser(this);
                var primary = primaryParser.VisitPrimary(primaryContext);
                var unaryOperatorContext = context.unary_operator();
                if (unaryOperatorContext != null)
                {
                    OperatorType unaryOperator = LiteralParser.VisitUnaryOperator(unaryOperatorContext);
                    return new Operator(unaryOperator, primary).UpdateCodePosition(context);
                }
                else
                {
                    return primary;
                }
            }

            // Increase/decrease expression
            var incDecExprContext = context.inc_or_dec_expression();
            if (incDecExprContext != null)
            {
                return VisitIncreaseDecreaseExpression(incDecExprContext);
            }

            //   | expression (operator_impl        ( attribute_instance )* expression)+ // right associative
            var expressionContext = context.expression();
            if (expressionContext.Length == 2)
            {
                #region 2 items expressions
                //   | expression DOUBLESTAR           ( attribute_instance )* expression
                //   | expression operator_mul_div_mod ( attribute_instance )* expression
                //   | expression operator_plus_minus  ( attribute_instance )* expression
                //   | expression operator_shift       ( attribute_instance )* expression
                //   | expression operator_cmp         ( attribute_instance )* expression
                //   | expression operator_eq_neq      ( attribute_instance )* expression
                //   | expression AMPERSAND            ( attribute_instance )* expression
                //   | expression operator_xor         ( attribute_instance )* expression
                //   | expression BAR                  ( attribute_instance )* expression
                //   | expression AND_LOG              ( attribute_instance )* expression
                //   | expression OR_LOG               ( attribute_instance )* expression

                OperatorType op = false switch
                {
                    _ when (context.DOUBLESTAR() != null) => OperatorType.Pow,
                    _ when (context.operator_mul_div_mod() != null) => LiteralParser.VisitOperatorMulDivMod(context.operator_mul_div_mod()),
                    _ when (context.operator_plus_minus() != null) => LiteralParser.VisitOperatorPlusMinus(context.operator_plus_minus()),  
                    _ when (context.operator_shift() != null) => LiteralParser.VisitOperatorShift(context.operator_shift()),
                    _ when (context.operator_cmp() != null) => LiteralParser.VisitOperatorCompare(context.operator_cmp()),
                    _ when (context.operator_eq_neq() != null) => LiteralParser.VisitOperatorEquality(context.operator_eq_neq()),  
                    _ when (context.AMPERSAND() != null) => OperatorType.And,
                    _ when (context.operator_xor() != null) => LiteralParser.VisitOperatorXor(context.operator_xor()),
                    _ when (context.BAR() != null) => OperatorType.Or,
                    _ when (context.AND_LOG() != null) => OperatorType.AndLog,
                    _ when (context.OR_LOG() != null) => OperatorType.OrLog,
                    _ when (context.operator_impl() != null) => LiteralParser.VisitOperatorImplement(context.operator_impl()),
                    _ => throw new Exception("Unknown binary operator")
                };

                var exprFirst = VisitExpression(expressionContext.First());
                var exprLast = VisitExpression(expressionContext.Last());
                return new Operator(op, exprFirst, exprLast).UpdateCodePosition(context);

                #endregion
            }

            
            if (context.KW_INSIDE() != null)
            {
                //   | expression KW_INSIDE LBRACE open_range_list RBRACE
#warning Inside expression is not implemented now
                return new NotImplemented("Inside expression").UpdateCodePosition(context);
            }

            if (context.KW_MATCHES().Any())
            {
#warning Matches expression is not implemented now
                return new NotImplemented("Matches expression").UpdateCodePosition(context);
            }

            if (context.TRIPLE_AND() != null)
            {
                //   | expression ( KW_MATCHES pattern )? TRIPLE_AND expression ( KW_MATCHES pattern )?
#warning Triple AND expression is not implemented now
                return new NotImplemented("Triple AND expression").UpdateCodePosition(context);
            }

            if (expressionContext.Length != 3)
            {
                throw new Exception("Invalid expression over 3 items to parse");
            }

            // Ternary
            //   | expression (( KW_MATCHES pattern )? QUESTIONMARK ( attribute_instance )* expression COLON expression)+
            var questionMarkContext = context.QUESTIONMARK();
            if (questionMarkContext != null)
            {
                var conditionExpression = VisitExpression(expressionContext[0]);
                var trueExpression = VisitExpression(expressionContext[1]);
                var falseExpression = VisitExpression(expressionContext[2]);
                return Operator.Ternary(conditionExpression, trueExpression, falseExpression);
            }

            throw new Exception("Invalid expression");
        }

        /// <summary>
        /// concatenation:
	    ///    LBRACE (expression ( concatenation | ( COMMA expression )+)?)? RBRACE;
        /// </summary>
        public Expression VisitConcatenation(ConcatenationContext context)
        {
            var concatContext = context.concatenation();
            if (concatContext != null)
            {
                var firstExprContext = context.expression(0);
                Expression firstExpression = VisitExpression(firstExprContext);
                var concatNext = VisitConcatenation(concatContext);
                return new Operator(OperatorType.ReplConcat, firstExpression, concatNext);
            }

            
            var expressionContext = context.expression();
            IEnumerable<(ExpressionContext exprContext, Expression expr)> expressions = 
                expressionContext.Select(e => (e, VisitExpression(e)));

            Expression result = expressions.First().expr;
            foreach ((ExpressionContext exprContext, Expression expr) in expressions)
            {
                if (expr != result)
                {
                    result = new Operator(OperatorType.Concat, result, expr).UpdateCodePosition(exprContext);
                }
            }

            return result;
        }

        /// <summary>
        /// hierarchical_identifier:
	    ///    ( KW_DOLAR_ROOT DOT )? ( identifier_with_bit_select DOT )* identifier;
        /// </summary>
        public Expression VisitHierarchicalIdentifier(Hierarchical_identifierContext context)
        {
            var rootContext = context.KW_DOLAR_ROOT();

            Expression? GetBitSelectIdentifier()
            {
                var bitSelectIdContext = context.identifier_with_bit_select();
                Expression? identifier = null;
                foreach (var idContext in bitSelectIdContext)
                {
                    identifier = VisitIdentifierWithBitSelect(idContext, identifier);
                }
                return identifier;
            }

            Expression? selectedName = (rootContext != null) ?
                                      new Identifier("$root").UpdateCodePosition(rootContext) :
                                      GetBitSelectIdentifier();

            var identifierContext = context.identifier();
            Expression identifier = VisitIdentifier(identifierContext);
            return selectedName.Append(context, OperatorType.Dot, identifier);
        }

        public static string GetIdentifierString(IdentifierContext context)
        {
            // identifier:
            //     C_IDENTIFIER
            //     | SIMPLE_IDENTIFIER
            //     | ESCAPED_IDENTIFIER
            //     | KW_SAMPLE
            //     | KW_RANDOMIZE
            //     | KW_TYPE_OPTION
            //     | KW_OPTION
            //     | KW_STD
            // ;
            var escapedIdentifierContext = context.ESCAPED_IDENTIFIER();
            if (escapedIdentifierContext != null)
            {
                return LiteralParser.VisitEscapedIdentifier(escapedIdentifierContext);
            }
            else
            {
                return context.GetText();
            }
        }

        public static Expression VisitIdentifier(IdentifierContext context)
            => new Identifier(GetIdentifierString(context)).UpdateCodePosition(context);

        /// <summary>
        /// mintypmax_expression: expression ( COLON expression COLON expression )?;
        /// </summary>
        public Expression VisitMintypmaxExpression(Mintypmax_expressionContext context)
        {
            if (context.expression().Length > 1)
            {
#warning Mintypmax expression (type and max specified) is not implemented now
            }

            return VisitExpression(context.expression(0));
        }

        /// <summary>
        ///variable_lvalue:
        /// LBRACE variable_lvalue ( COMMA variable_lvalue )* RBRACE
        ///  | package_or_class_scoped_hier_id_with_select
        ///  | ( assignment_pattern_expression_type )? assignment_pattern_variable_lvalue
        ///  | streaming_concatenation
        ///;
        /// </summary>
        public Expression VisitVariableLeftValue(Variable_lvalueContext context)
        {
            var varLeftValueContext = context.variable_lvalue();
            if (varLeftValueContext.Any())
            {
                if (varLeftValueContext.Length == 1)
                {
                    return VisitVariableLeftValue(varLeftValueContext.First());
                }

                IEnumerable<Expression> parts = varLeftValueContext.Select(x => VisitVariableLeftValue(x));
                return Operator.Reduce(parts);
            }

            var pkgClassScopedHierIdWithSelectContext = context.package_or_class_scoped_hier_id_with_select();
            if (pkgClassScopedHierIdWithSelectContext != null)
            {
                return VisitPackageOrClassScopedHierIdWithSelect(pkgClassScopedHierIdWithSelectContext);
            }

            var assignPatternVariableContext = context.assignment_pattern_variable_lvalue();
            if (assignPatternVariableContext != null)
            {
#warning Assignment pattern variable left value is not implemented now
                return new NotImplemented().UpdateCodePosition(context);
            }

            var streamingConcatenationContext = context.streaming_concatenation();
            if (streamingConcatenationContext != null)
            {
#warning Streaming concatenation is not implemented now
                return new NotImplemented().UpdateCodePosition(context);
            }

            throw new Exception("Invalid variable left value");
        }

        /// <summary>
        /// event_trigger:
        ///  ( ARROW
        ///    | DOUBLE_RIGHT_ARROW ( delay_or_event_control )?
        ///  ) hierarchical_identifier SEMI;
        /// </summary>
        public Expression VisitEventTrigger(Event_triggerContext context)
        {
            if (context.delay_or_event_control() != null)
            {
#warning Delay or event control is not implemented now
            }

            var hierIdentifierContext = context.hierarchical_identifier();
            var hierIdentifier = VisitHierarchicalIdentifier(hierIdentifierContext);
            return new Operator(OperatorType.Arrow, hierIdentifier).UpdateCodePosition(hierIdentifierContext);
        }

        /// <summary>
        /// bit_select: LSQUARE_BR expression RSQUARE_BR;
        /// </summary>
        public Expression VisitBitSelect(Bit_selectContext context, Expression selectedName)
        {
            var expressionContext = context.expression();
            Expression expression = VisitExpression(expressionContext);
            return selectedName.Append(expressionContext, OperatorType.Index, expression);
        }

        /// <summary>
        /// identifier_with_bit_select: identifier ( bit_select )*;
        /// </summary>
        public Expression VisitIdentifierWithBitSelect(Identifier_with_bit_selectContext context, Expression? selectedName)
        {
            var identifierContext = context.identifier();
            Expression identifier = VisitIdentifier(identifierContext);
            identifier = selectedName.Append(context, OperatorType.Dot, identifier);
            var bitSelectContext = context.bit_select();
            foreach (var bitSelect in bitSelectContext)
            {
                identifier = VisitBitSelect(bitSelect, identifier);
            }

            return identifier;
        }

        /// <summary>
        /// '::' separated then '.' separated
        /// package_or_class_scoped_hier_id_with_select:
        ///     package_or_class_scoped_path ( bit_select )*
        ///     ( DOT identifier_with_bit_select )*
        ///     ( LSQUARE_BR expression ( operator_plus_minus )? COLON expression RSQUARE_BR )?;
        /// </summary>
        public Expression VisitPackageOrClassScopedHierIdWithSelect(Package_or_class_scoped_hier_id_with_selectContext context)
        {
            var pkgClassScopedPathContext = context.package_or_class_scoped_path();
            Expression identifier = VisitPackageOrClassScopedPath(pkgClassScopedPathContext);
            var bitSelectContext = context.bit_select();
            foreach (var bitSelect in bitSelectContext)
            {
                identifier = VisitBitSelect(bitSelect, identifier);
            }

            var expressionContext = context.expression();

            if (!expressionContext.Any())
            {
                return identifier;
            }

            if (expressionContext.Length != 2)
            {
                throw new Exception("Invalid package/class scoped hierarchical identifier");
            }

            Expression firstExpression = VisitExpression(expressionContext.First());
            Expression lastExpression = VisitExpression(expressionContext.Last());
            var plusMinusContext = context.operator_plus_minus();
            OperatorType op = OperatorType.Downto;
            if (plusMinusContext != null)
            {
                // [offset+:width] or [offset-:width]
                var plusMinus = LiteralParser.VisitOperatorPlusMinus(plusMinusContext);
                op = plusMinus switch
                {
                    OperatorType.Add => OperatorType.PartSelectPost,
                    _ => OperatorType.PartSelectPre
                };
            }

            Operator bitSelectOperator = new Operator(op, firstExpression, lastExpression).UpdateCodePosition(context);

            return identifier.Append(context, OperatorType.Index, bitSelectOperator);
        }

        /// <summary>
        /// parameter_value_assignment:
	    ///   HASH LPAREN ( list_of_parameter_value_assignments )? RPAREN;
        /// </summary>
        public IEnumerable<Expression> VisitParameterValueAssignment(Parameter_value_assignmentContext context)
        {
            var paramValueAssignContext = context.list_of_parameter_value_assignments();
            if (paramValueAssignContext != null)
            {
                ModuleInstanceParser moduleInstanceParser = new ModuleInstanceParser(this);
                var assignments = moduleInstanceParser.VisitParameterValueAssignments(paramValueAssignContext);
                foreach (var assignment in assignments)
                {
                    yield return assignment;
                }
            }
        }

        /// <summary>
        /// package_or_class_scoped_path_item:
        /// 	identifier ( parameter_value_assignment )?
        /// ;
        /// </summary>
        private Expression VisitPackageOrClassScopedPathItem(Package_or_class_scoped_path_itemContext context,
            Expression? selectedName, OperatorType subnameAccessType)
        {
            var identifierContext = context.identifier();
            Expression identifier = VisitIdentifier(identifierContext);

            if (selectedName != null)
            {
                identifier = new Operator(subnameAccessType, selectedName, identifier).UpdateCodePosition(context);
            }

            var paramValueAssignContext = context.parameter_value_assignment();
            if (paramValueAssignContext != null)
            {
                var paramValueAssign = VisitParameterValueAssignment(paramValueAssignContext);
                identifier = Operator.Parametrization(identifier, paramValueAssign).UpdateCodePosition(paramValueAssignContext);
            }

            return identifier;
        }

        /// <summary>
        /// implicit_class_handle:
        ///  KW_THIS ( DOT KW_SUPER )?
        ///   | KW_SUPER
        ///  ;
        /// </summary>
        public Expression VisitImplicitClassHandle(Implicit_class_handleContext context,
            Expression? selectedName, OperatorType subnameAccessType)
        {
            var thisContext = context.KW_THIS();
            if (thisContext != null)
            {
                var thisIdentifier = new Identifier("this").UpdateCodePosition(thisContext);
                selectedName = selectedName.Append(context, subnameAccessType, thisIdentifier);
                subnameAccessType = OperatorType.Dot;
            }

            var superContext = context.KW_SUPER();
            if (superContext != null)
            {
                var superIdentifier = new Identifier("super").UpdateCodePosition(superContext);
                selectedName = selectedName.Append(context, subnameAccessType, superIdentifier);
            }

            if (selectedName == null)
            {
                throw new Exception("Invalid implicit class handle");
            }

            return selectedName;
        }

        /// <summary>
        /// '::' separated
        /// package_or_class_scoped_path:
        ///    ( KW_LOCAL DOUBLE_COLON )? (
        ///   		KW_DOLAR_ROOT
        ///         | implicit_class_handle
        ///         | KW_DOLAR_UNIT
        ///         | package_or_class_scoped_path_item
        /// 	) ( DOUBLE_COLON package_or_class_scoped_path_item)*;
        /// </summary>
        public Expression VisitPackageOrClassScopedPath(Package_or_class_scoped_pathContext context)
        {
            Expression? expression = null;

            Expression VisitPackageOrClassScopedPathItems()
            {
                var pkgClassScopedPathItemContext = context.package_or_class_scoped_path_item();
                if (!pkgClassScopedPathItemContext.Any())
                {
                    throw new Exception("Invalid package/class scoped path");
                }

                foreach (var pkgContext in pkgClassScopedPathItemContext)
                {
                    expression = VisitPackageOrClassScopedPathItem(pkgContext, expression, OperatorType.DoubleColon);
                }

                if (expression == null)
                {
                    throw new Exception("Invalid package/class scoped path");
                }

                return expression;
            }

            var localContext = context.KW_LOCAL();
            if (localContext != null)
            {
                expression = new Identifier("local").UpdateCodePosition(localContext);
            }

            var rootContext = context.KW_DOLAR_ROOT();
            if (rootContext != null)
            {
                expression = expression.Append(context, OperatorType.DoubleColon, 
                    new Identifier("$root").UpdateCodePosition(rootContext));
            }
            else
            {
                var implClassHandleContext = context.implicit_class_handle();
                if (implClassHandleContext != null)
                {
                    expression = VisitImplicitClassHandle(implClassHandleContext, expression, OperatorType.DoubleColon); ;
                }
                else
                {
                    var unitContext = context.KW_DOLAR_UNIT();
                    if (unitContext != null)
                    {
                        var unit = new Identifier("$unit").UpdateCodePosition(unitContext);
                        expression = expression.Append(context, OperatorType.DoubleColon, unit);
                    }
                    else
                    {
                        return VisitPackageOrClassScopedPathItems();
                    }
                }
            }

            return VisitPackageOrClassScopedPathItems();
        }

        /// <summary>
        /// cond_predicate:
	    ///     expression ( KW_MATCHES pattern )? ( TRIPLE_AND expression ( KW_MATCHES pattern )? )*;
        /// </summary>
        public Expression VisitConditionPredicate(Cond_predicateContext context)
        {
            if (context.KW_MATCHES() != null)
            {
#warning Mathces condition predicate is not implemented now
            }

            if (context.TRIPLE_AND() != null)
            {
#warning Triple AND condition predicate is not implemented now
            }

            return VisitExpression(context.expression(0));
        }

        /// <summary>
        /// list_of_arguments_named_item: DOT identifier LPAREN ( expression )? RPAREN;
        /// list_of_arguments:
        ///     ( list_of_arguments_named_item
        ///      | COMMA list_of_arguments_named_item
        ///      | expression ( COMMA ( expression )? )*
        ///      | ( COMMA ( expression )? )+
        ///     )
        ///     ( COMMA list_of_arguments_named_item )*;
        /// </summary>
        public void VisitArguments(List_of_argumentsContext context, List<Expression> arguments)
        {
            bool expectingValue = true;
            var childrenContext = context.children;
            foreach (var child in childrenContext)
            {
                bool isComma = false;
                if (child is ITerminalNode node)
                {
                    isComma = (node.Symbol.Type == COMMA);
                }

                if (expectingValue && isComma)
                {
                    arguments.Add(new Identifier("void").UpdateCodePosition(child));
                }

                if (isComma)
                {
                    expectingValue = true;
                }
                else
                {
                    if (child is ExpressionContext expressionContext)
                    {
                        Expression expression = VisitExpression(expressionContext);
                        arguments.Add(expression);
                    }
                    else
                    {
                        if (child is List_of_arguments_named_itemContext argNamedItemContext)
                        {
                            Expression name = VisitIdentifier(argNamedItemContext.identifier());
                            var valueContext = argNamedItemContext.expression();
                            Expression value = (valueContext != null) ?
                                               VisitExpression(valueContext) :
                                               new Identifier("void").UpdateCodePosition(child);

                            Expression expression = new Operator(OperatorType.MapAssociation, name, value);
                            arguments.Add(expression);
                        }
                    }

                    expectingValue = false;
                }
            }
        }

        /// <summary>
        /// operator_assignment: variable_lvalue assignment_operator expression;
        /// </summary>
        public Expression VisitOperatorAssignment(Operator_assignmentContext context)
        {
            var varLeftValueContext = context.variable_lvalue();
            var expressionContext = context.expression();
            Expression destination = VisitVariableLeftValue(varLeftValueContext);
            Expression source = VisitExpression(expressionContext);

            OperatorType op = LiteralParser.VisitAssignmentOperator(context.assignment_operator());

            return new Operator(op, destination, source).UpdateCodePosition(context);
        }
    }
}
