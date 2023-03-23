using Antlr4.Runtime;
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
    public class StatementParser : HDLParser
    {
        public StatementParser(HDLParser other) : base(other) { }

        /// <summary>
        /// always_construct:
        ///     always_keyword statement;
        ///
        /// </summary>
        /// <remarks>
        /// The actual sensitivity list of always construct is deeper (in <see cref="VisitProceduralTimingControlStatement(Procedural_timing_control_statementContext)"/>)
        /// </remarks>
        public HDLStatement VisitAlwaysConstruct(Always_constructContext context)
        {
            var statementContext = context.statement();
            var alwaysContext = context.always_keyword();

            HDLStatement statement = VisitStatement(statementContext);
            if (alwaysContext.KW_ALWAYS() != null)
            {
                // Nothing to do
            }
            else
            {
                if (statement is ProcessStatement procStatement) { }
                else
                {
                    ProcessTriggerConstrain trigger = false switch
                    {
                        _ when (alwaysContext.KW_ALWAYS_COMB() != null) => ProcessTriggerConstrain.AlwaysCombinational,
                        _ when (alwaysContext.KW_ALWAYS_FF() != null) => ProcessTriggerConstrain.AlwaysFlipFlop,
                        _ when (alwaysContext.KW_ALWAYS_LATCH() != null) => ProcessTriggerConstrain.AlwaysLatch,
                        _ => throw new Exception("Invalid process trigger constrain of always statement")
                    };

                    statement = new ProcessStatement(statement, trigger)
                        .UpdateCodePosition(context)
                        .UpdateDocument(context, CommentParser);
                }
            }

            return statement;
        }

        /// <summary>
        /// subroutine_call_statement:
	    ///     ( KW_VOID APOSTROPHE LPAREN expression RPAREN ) SEMI;
        /// </summary>
        public HDLStatement VisitSubroutineCallStatement(Subroutine_call_statementContext context)
        {
            var expressionContext = context.expression();
            Expression expression = new ExpressionParser(this).VisitExpression(expressionContext);
            Operator op = new Operator(OperatorType.Call,
                new Identifier("void").UpdateCodePosition(context.KW_VOID()),
                expression);

            return new ExpressionStatement(op).UpdateCodePosition(context).UpdateDocument(context, CommentParser);
        }

        /// <summary>
        /// jump_statement:
        ///     ( KW_RETURN ( expression )?
        ///       | KW_BREAK
        ///       | KW_CONTINUE
        ///     ) SEMI;
        /// </summary>
        public HDLStatement VisitJumpStatement(Jump_statementContext context)
        {
            if (context.KW_RETURN() != null)
            {
                var returnExprContext = context.expression();
                if (returnExprContext != null)
                {
                    // return <expression>;
                    Expression returnExpression = new ExpressionParser(this).VisitExpression(returnExprContext);
                    return new ReturnStatement(returnExpression)
                        .UpdateCodePosition(context)
                        .UpdateDocument(context, CommentParser);
                }
                else
                {
                    // return;
                    return new ReturnStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
                }
            }
            else if (context.KW_BREAK() != null)
            {
                return new BreakStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }
            else if (context.KW_CONTINUE() != null)
            {
                return new ContinueStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            throw new Exception("Excepted a jump statement");
        }

        /// <summary>
        /// statement_item:
        ///   ( blocking_assignment
        ///     | nonblocking_assignment
        ///     | procedural_continuous_assignment
        ///     | inc_or_dec_expression
        ///     | primary
        ///     | clocking_drive
        ///   ) SEMI
        ///   | case_statement
        ///   | conditional_statement
        ///   | subroutine_call_statement
        ///   | disable_statement
        ///   | event_trigger
        ///   | loop_statement
        ///   | jump_statement
        ///   | par_block
        ///   | procedural_timing_control_statement
        ///   | seq_block
        ///   | wait_statement
        ///   | procedural_assertion_statement
        ///   | randsequence_statement
        ///   | randcase_statement
        ///   | expect_property_statement
        /// ;
        /// </summary>
        public HDLStatement VisitStatementItem(Statement_itemContext context)
        {
            var blockingAssignContext = context.blocking_assignment();
            if (blockingAssignContext != null)
            {
                return VisitBlockingAssignment(blockingAssignContext);
            }

            var nonBlockingAssignContext = context.nonblocking_assignment();
            if (nonBlockingAssignContext != null)
            {
                return VisitNonBlockingAssignment(nonBlockingAssignContext);
            }

            var procContinuousAssignContext = context.procedural_assertion_statement();
            if (procContinuousAssignContext != null)
            {
#warning Procedural continuous assignment is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var increaseDecreaseExprContext = context.inc_or_dec_expression();
            if (increaseDecreaseExprContext != null)
            {
                Expression expression = new ExpressionParser(this).VisitIncreaseDecreaseExpression(increaseDecreaseExprContext);
                return new ExpressionStatement(expression)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }

            var primaryContext = context.primary();
            if (primaryContext != null)
            {
                Expression expression = new ExpressionPrimaryParser(this).VisitPrimary(primaryContext);
                return new ExpressionStatement(expression)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }

            var clockingDriveContext = context.clocking_drive();
            if (clockingDriveContext != null)
            {
#warning Clocking drive statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var caseContext = context.case_statement();
            if (caseContext != null)
            {
                return VisitCaseStatement(caseContext);
            }

            var conditionalContext = context.conditional_statement();
            if (conditionalContext != null)
            {
                return VisitConditionalStatement(conditionalContext);
            }

            var subroutineCallContext = context.subroutine_call_statement();
            if (subroutineCallContext != null)
            {
                return VisitSubroutineCallStatement(subroutineCallContext);
            }

            var disableContext = context.disable_statement();
            if (disableContext != null)
            {
#warning Disable statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var eventTriggerContext = context.event_trigger();
            if (eventTriggerContext != null)
            {
#warning Event trigger statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var loopContext = context.loop_statement();
            if (loopContext != null)
            {
                return VisitLoopStatement(loopContext);
            }

            var jumpContext = context.jump_statement();
            if (jumpContext != null)
            {
                return VisitJumpStatement(jumpContext);
            }

            var parallelBlockContext = context.par_block();
            if (parallelBlockContext != null)
            {
                return VisitParallelBlock(parallelBlockContext);
            }

            var procTimingControlContext = context.procedural_timing_control_statement();
            if (procTimingControlContext != null)
            {
                return VisitProceduralTimingControlStatement(procTimingControlContext);
            }

            var sequenceBlockContext = context.seq_block();
            if (sequenceBlockContext != null)
            {
                return VisitSequenceBlock(sequenceBlockContext);
            }

            var waitContext = context.wait_statement();
            if (waitContext != null)
            {
#warning Wait statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var procAssertionContext = context.procedural_assertion_statement();
            if (procAssertionContext != null)
            {
#warning Procedural assertion statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var randSequenceContext = context.randsequence_statement();
            if (randSequenceContext != null)
            {
#warning Random sequence statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var randCaseContext = context.randcase_statement();
            if (randCaseContext != null)
            {
#warning Random case statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            var expectPropertyContext = context.expect_property_statement();
            if (expectPropertyContext != null)
            {
#warning Expect property statement is not implemented now
                return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }

            throw new Exception("Expected a statement");
        }

        /// <summary>
        /// statement: ( identifier COLON )? ( attribute_instance )* statement_item;
        /// </summary>
        public HDLStatement VisitStatement(StatementContext context)
        {
            var attrInstanceContext = context.attribute_instance();
            AttributeParser.VisitAttributeInstance(attrInstanceContext);

            var statementItemContext = context.statement_item();
            HDLStatement statement = VisitStatementItem(statementItemContext);
            var labelContext = context.identifier();
            if (labelContext != null)
            {
                string label = labelContext.GetText();
                statement.Labels.Insert(0, label);
            }

            return statement;
        }

        /// <summary>
        /// blocking_assignment:
        ///     variable_lvalue ASSIGN ( delay_or_event_control expression | dynamic_array_new )
        ///     | package_or_class_scoped_hier_id_with_select ASSIGN class_new
        ///     | operator_assignment
        /// ;
        /// </summary>
        private HDLStatement VisitBlockingAssignment(Blocking_assignmentContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            var varLeftValueContext = context.variable_lvalue();
            if (varLeftValueContext != null)
            {
                Expression destination = expressionParser.VisitVariableLeftValue(varLeftValueContext);
                var sourceContext = context.expression();

                if (sourceContext != null)
                {
                    Expression source = expressionParser.VisitExpression(sourceContext);
                    var delayEventContext = context.delay_or_event_control();
                    (Expression? delay, IEnumerable<Expression>? ev) = 
                        new DelayParser(this).VisitDelayOrEventControl(delayEventContext);

                    return new AssignStatement(source, destination, delay, ev, true)
                        .UpdateCodePosition(context)
                        .UpdateDocument(context, CommentParser);
                }
                else
                {
                    var dynamicArrayContext = context.dynamic_array_new();
                    if (dynamicArrayContext == null)
                    {
                        throw new Exception("Excepted an right value of blocking assignemt");
                    }

#warning New dynamic array of blocking assignment is not implemented now
                    return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
                }
            }
            else
            {
                var newClassContext = context.class_new();
                if (newClassContext != null)
                {
#warning New class blocking assignment is not implemented now
                    return new NopStatement().UpdateCodePosition(context).UpdateDocument(context, CommentParser);
                }
                else
                {
                    var opAssignContext = context.operator_assignment();
                    if (opAssignContext == null)
                    {
                        throw new Exception("Excepted an left value of blocking assignment");
                    }

                    Expression opAssign = expressionParser.VisitOperatorAssignment(opAssignContext);
                    return new ExpressionStatement(opAssign)
                        .UpdateCodePosition(context)
                        .UpdateDocument(context, CommentParser);
                }
            }
        }

        /// <summary>
        /// case_statement:
        ///     ( unique_priority )?
        ///     ( KW_CASE LPAREN expression RPAREN KW_INSIDE ( case_inside_item )+
        ///       | case_keyword LPAREN expression RPAREN
        ///         ( KW_MATCHES ( case_pattern_item )+
        ///           | ( case_item )+
        ///         )
        ///     ) KW_ENDCASE;
        /// case_keyword:
        ///     KW_CASE
        ///     | KW_CASEZ
        ///     | KW_CASEX
        /// ;
        /// </summary>
        private HDLStatement VisitCaseStatement(Case_statementContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);

            // case(<switchExpression>)  (also casez, casex)
            Expression selectOn = expressionParser.VisitExpression(context.expression());

            Dictionary<Expression, HDLObject?> cases = new Dictionary<Expression, HDLObject?>();
            HDLObject? defaultCase = null;
            CaseType caseType = CaseType.Case;

            if (context.KW_INSIDE() != null)
            {
#warning Case statement inside is not implemented now
            }
            else
            {
                var caseKeywordContext = context.case_keyword();
                caseType = false switch
                {
                    _ when (caseKeywordContext.KW_CASE() != null) => CaseType.Case,
                    _ when (caseKeywordContext.KW_CASEX() != null) => CaseType.Casex,
                    _ when (caseKeywordContext.KW_CASEZ() != null) => CaseType.Casez,
                    _ => throw new Exception("Invalid case type (out of case, casex and casez)")
                };

                var casesContext = context.case_item();
                var caseItems = casesContext.SelectMany(c => VisitCaseItem(c));

                foreach ((Expression? expression, HDLObject? obj) caseItem in caseItems)
                {
                    if (caseItem.expression != null)
                    {
                        cases.Add(caseItem.expression, caseItem.obj);
                    }
                    else
                    {
                        if (defaultCase != null)
                        {
                            throw new Exception("Case with multiple default");
                        }

                        defaultCase = caseItem.obj;
                    }
                }
            }

            CaseStatement caseStatement = new CaseStatement(caseType, selectOn, cases, defaultCase)
                .UpdateCodePosition(context)
                .UpdateDocument(context, CommentParser);

            var uniquePriorityContext = context.unique_priority();
            if (uniquePriorityContext != null)
            {
                caseStatement.UniqueConstrain = false switch
                {
                    _ when (uniquePriorityContext.KW_PRIORITY() != null) => CaseUniqueConstrain.Priority,
                    _ when (uniquePriorityContext.KW_UNIQUE() != null) => CaseUniqueConstrain.Unique,
                    _ when (uniquePriorityContext.KW_UNIQUE0() != null) => CaseUniqueConstrain.Unique0,
                    _ => throw new Exception("Invalid unique/priority constrain for case statement")
                };
            }

            return caseStatement;
        }

        /// <summary>
        /// case_item:
        ///     ( KW_DEFAULT ( COLON )?
        ///       | expression ( COMMA expression )* COLON
        ///     ) statement_or_null;
        /// </summary>
        private IEnumerable<(Expression?, HDLObject?)> VisitCaseItem(Case_itemContext context)
        {
            var conditionsContext = context.expression();
            var statementsContext = context.statement_or_null();

            HDLObject? statements = VisitStatementOrNull(statementsContext);
            
            if (conditionsContext.Any())
            {
                // Case any
                ExpressionParser expressionParser = new ExpressionParser(this);

                foreach (var condContext in conditionsContext)
                {
                    Expression caseExpression = expressionParser.VisitExpression(condContext);
                    yield return (caseExpression, statements);
                }                
            }
            else
            {
                // Default
                yield return (null, statements);
            }
        }

        /// <summary>
        /// variable_assignment:
	    ///     variable_lvalue ASSIGN expression;
        /// </summary>
        public AssignStatement VisitVariableAssignment(Variable_assignmentContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            var varLeftValueContext = context.variable_lvalue();
            var sourceContext = context.expression();

            var varLeftValue = expressionParser.VisitVariableLeftValue(varLeftValueContext);
            var source = expressionParser.VisitExpression(sourceContext);

            return new AssignStatement(source, varLeftValue, true)
                .UpdateCodePosition(context)
                .UpdateDocument(context, CommentParser);
        }

        /// <summary>
        /// loop_statement:
        ///  ( KW_FOREVER
        ///       | ( ( KW_REPEAT
        ///               | KW_WHILE
        ///               ) LPAREN expression
        ///           | KW_FOR LPAREN ( for_initialization )? SEMI ( expression )? SEMI ( for_step )?
        ///           ) RPAREN
        ///       ) statement_or_null
        ///   | KW_DO statement_or_null KW_WHILE LPAREN expression RPAREN SEMI
        ///   | KW_FOREACH LPAREN package_or_class_scoped_hier_id_with_select LSQUARE_BR loop_variables
        ///   RSQUARE_BR RPAREN statement
        /// ;
        /// </summary>
        private HDLStatement VisitLoopStatement(Loop_statementContext context)
        {
            var statementContext = context.statement();
            HDLStatement? body = (statementContext != null) ?
                                 VisitStatement(statementContext) :
                                 VisitStatementOrNull(context.statement_or_null());

            ExpressionParser expressionParser = new ExpressionParser(this);

            if (context.KW_FOREVER() != null)
            {
                return new WhileStatement(new Integer(1, 1), body)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }
            
            if (context.KW_REPEAT() != null)
            {
                var expressionContext = context.expression();
                Expression repeatCount = expressionParser.VisitExpression(expressionContext);
                return new RepeatStatement(repeatCount, body)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }

            if (context.KW_WHILE() != null)
            {
                var expressionContext = context.expression();
                Expression condition = expressionParser.VisitExpression(expressionContext);
                return new WhileStatement(condition, body)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }

            if (context.KW_DO() != null)
            {
                var expressionContext = context.expression();
                Expression condition = expressionParser.VisitExpression(expressionContext);
                return new DoWhileStatement(condition, body)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }

            if (context.KW_FOR() != null)
            {
                var initContext = context.for_initialization();
                var condContext = context.expression();
                var stepContext = context.for_step();

                BlockStatement initBlock = new BlockStatement().UpdateCodePosition(initContext);

                if (initContext != null)
                {
                    VisitForLoopInitialization(initContext, initBlock.Statements);
                }

                Expression condition = (condContext != null) ?
                                       expressionParser.VisitExpression(condContext) :
                                       new Integer(1, 1); // Always true

                BlockStatement stepBlock = new BlockStatement().UpdateCodePosition(stepContext);
                if (stepContext != null)
                {
                    VisitForLoopStep(stepContext, stepBlock.Statements);
                }

                return new ForStatement(initBlock, condition, stepBlock, body)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }

            if (context.KW_FOREACH() != null)
            {
                // KW_FOREACH LPAREN package_or_class_scoped_hier_id_with_select
                //                   LSQUARE_BR loop_variables RSQUARE_BR RPAREN statement
                var collectionContext = context.package_or_class_scoped_hier_id_with_select();
                var collection = expressionParser.VisitPackageOrClassScopedHierIdWithSelect(collectionContext);
                var variableContext = context.loop_variables();

                IEnumerable<HDLObject> variableDefinitions = VisitLoopVariables(variableContext);

                return new ForEachStatement(variableDefinitions, collection, body)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }

            throw new Exception("Excepted a loop statement");
        }

        /// <summary>
        /// for_variable_declaration:
        ///     ( KW_VAR )? data_type for_variable_declaration_var_assign
        ///     ( COMMA for_variable_declaration_var_assign )*
        /// ;
        /// </summary>
        private IEnumerable<IdentifierDefinition> VisitForLoopVariableDeclaration(For_variable_declarationContext context)
        {
            TypeParser typeParser = new TypeParser(this);
            ExpressionParser expressionParser = new ExpressionParser(this);

            var dataTypeContext = context.data_type();
            var forVariableDeclarationContext = context.for_variable_declaration_var_assign();

            Expression dataType = typeParser.VisitDataType(dataTypeContext);
            bool isLatched = context.KW_VAR() != null;
            bool first = true;

            foreach (var variableContext in forVariableDeclarationContext)
            {
                // for_variable_declaration_var_assign: identifier ASSIGN expression;
                string name = ExpressionParser.GetIdentifierString(variableContext.identifier());

                if (!first)
                {
                    dataType = dataType.Clone();
                }

                var defaultValueContext = variableContext.expression();
                Expression defaultValue = expressionParser.VisitExpression(defaultValueContext);

                first &= false;

                yield return new IdentifierDefinition(name, dataType, defaultValue, Direction.Internal, isLatched)
                    .UpdateCodePosition(variableContext);
            }
        }

        /// <summary>
        /// for_initialization:
        ///     list_of_variable_assignments
        ///     | for_variable_declaration ( COMMA for_variable_declaration )*
        /// ;
        /// </summary>
        private void VisitForLoopInitialization(For_initializationContext context, List<HDLObject> statements)
        {
            var varAssignContext = context.list_of_variable_assignments();
            if (varAssignContext != null)
            {
                // list_of_variable_assignments: variable_assignment ( COMMA variable_assignment )*;
                statements.AddRange(varAssignContext.variable_assignment()
                    .Select((v) => VisitVariableAssignment(v)));
            }
            else
            {
                statements.AddRange(context.for_variable_declaration()
                    .SelectMany((v) => VisitForLoopVariableDeclaration(v)));
            }
        }

        /// <summary>
        /// for_step: sequence_match_item ( COMMA sequence_match_item )*;
        /// </summary>
        private void VisitForLoopStep(For_stepContext context, List<HDLObject> statements)
            => statements.AddRange(context.sequence_match_item()
                .Select((i) => VisitSequenceMatchItem(i)));

        /// <summary>
        /// loop_variables: ( identifier )? ( COMMA ( identifier )? )*;
        /// </summary>
        private IEnumerable<HDLObject> VisitLoopVariables(Loop_variablesContext context)
            => context.identifier().Select(i =>
                new ExpressionStatement(ExpressionParser.VisitIdentifier(i)).UpdateCodePosition(i));

        /// <summary>
        /// nonblocking_assignment
        ///    : variable_lvalue '<=' (delay_or_event_control)? expression
        ///    ;
        /// </summary>
        private HDLStatement VisitNonBlockingAssignment(Nonblocking_assignmentContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this); 

            var varLeftValueContext = context.variable_lvalue();
            var sourceContext = context.expression();

            Expression varLeftValue = expressionParser.VisitVariableLeftValue(varLeftValueContext);
            Expression source = expressionParser.VisitExpression(sourceContext);    

            var delayEventContext = context.delay_or_event_control();
            if (delayEventContext != null)
            {
                (Expression? delay, IEnumerable<Expression>? ev) = 
                    new DelayParser(this).VisitDelayOrEventControl(delayEventContext);
                return new AssignStatement(source, varLeftValue, delay, ev, false);
            }
            else
            {
                return new AssignStatement(source, varLeftValue, false)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }
        }

        /// <summary>
        /// seq_block:
        ///     KW_BEGIN ( COLON identifier | {_input->LA(1) != COLON}? )
        ///         ( block_item_declaration )* ( statement_or_null )*
        ///     KW_END (COLON identifier |  {_input->LA(1) != COLON}?);
        /// </summary>
        private HDLStatement VisitSequenceBlock(Seq_blockContext context)
            => VisitBlock(context);

        /// <summary>
        /// join_keyword:
        ///     KW_JOIN
        ///     | KW_JOIN_ANY
        ///     | KW_JOIN_NONE
        /// ;
        /// </summary>
        private BlockJoinType VisitJoinKeyword(Join_keywordContext context)
            => false switch
            {
                _ when (context.KW_JOIN() != null) => BlockJoinType.ParallelJoin,
                _ when (context.KW_JOIN_ANY() != null) => BlockJoinType.ParallelJoinAny,
                _ when (context.KW_JOIN_NONE() != null) => BlockJoinType.ParallelJoinNone,
                _ => throw new Exception("Excepted a parallel join keyword")
            };

        /// <summary>
        /// parallel_block:
        ///     KW_FORK ( COLON identifier | {_input->LA(1) != COLON}? )
        ///         ( block_item_declaration )* ( statement_or_null )*
        ///     join_keyword ( COLON identifier |  {_input->LA(1) != COLON}? );
        /// </summary>
        private HDLStatement VisitParallelBlock(Par_blockContext context)
        {
            BlockStatement block = VisitBlock(context);
            block.JoinType = VisitJoinKeyword(context.join_keyword());
            return block;
        }

        /// <summary>
        /// block_item_declaration:
        ///     ( attribute_instance )* (
        ///         data_declaration
        ///         | ( local_parameter_declaration
        ///             | parameter_declaration
        ///             ) SEMI
        ///         | let_declaration
        ///     )
        /// ;
        /// </summary>
        public void VisitBlockItemDeclaration(Block_item_declarationContext context, List<HDLObject> objects)
        {
            AttributeParser.VisitAttributeInstance(context.attribute_instance());

            var dataDeclarationContext = context.data_declaration();
            if (dataDeclarationContext != null)
            {
                DeclarationParser declarationParser = new DeclarationParser(this);
                declarationParser.VisitDataDeclaration(dataDeclarationContext, objects);
                return;
            }

            ParameterDefinitionParser parameterDefinitionParser = new ParameterDefinitionParser(this);
            var localParamDeclarationContext = context.local_parameter_declaration();
            if (localParamDeclarationContext != null)
            {
                List<HDLObject> localParameters = new List<HDLObject>();
                parameterDefinitionParser.VisitLocalParameterDeclaration(localParamDeclarationContext, localParameters);
                objects.AddRange(localParameters);
                return;
            }

            var paramDeclarationContext = context.parameter_declaration();
            if (paramDeclarationContext != null)
            {
                List<HDLObject> parameters = new List<HDLObject>();
                parameterDefinitionParser.VisitParameterDeclaration(paramDeclarationContext, parameters);
                objects.AddRange(parameters);
                return;
            }

            var letDeclarationContext = context.let_declaration();
            if (letDeclarationContext != null)
            {
#warning Let declaration is not implemented now
                return;
            }

            throw new Exception("Excepted a block item declaration");
        }

        /// <summary>
        /// conditional_statement:
        ///    ( unique_priority )? KW_IF LPAREN cond_predicate RPAREN statement_or_null
        ///    ( KW_ELSE statement_or_null | {_input->LA(1) != KW_ELSE}? );
        /// </summary>
        private HDLStatement VisitConditionalStatement(Conditional_statementContext context)
        {
            var condPredicateContext = context.cond_predicate();
            var statementContext = context.statement_or_null();

            ExpressionParser expressionParser = new ExpressionParser(this);

            Expression condition = expressionParser.VisitConditionPredicate(condPredicateContext);
            HDLStatement? trueBody = VisitStatementOrNull(statementContext.First());
            IfStatement ifStatement;

            HDLStatement? falseBody;
            if (statementContext.Length == 2)
            {
                falseBody = VisitStatementOrNull(statementContext.Last());
                ifStatement = new IfStatement(condition, trueBody, falseBody);
            }
            else
            {
                ifStatement = new IfStatement(condition, trueBody);
            }

            ifStatement.Document = CommentParser.Parse(context);
            ifStatement.CollapseElseIf();

            return ifStatement;
        }

        /// <summary>
        /// procedural_timing_control_statement: procedural_timing_control statement_or_null;
        /// </summary>
        private HDLStatement VisitProceduralTimingControlStatement(Procedural_timing_control_statementContext context)
        {
            var procTimingControlContext = context.procedural_timing_control();

            DelayParser delayParser = new DelayParser(this);
            (Expression? delay, IEnumerable<Expression>? ev) = delayParser.VisitProceduralTimingControl(procTimingControlContext);
            HDLStatement? body = VisitStatementOrNull(context.statement_or_null());  

            if (ev != null)
            {
                if (delay != null)
                {
                    throw new Exception("No event and delay at once");
                }

                return new ProcessStatement(ev, body)
                    .UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }
            else if (delay != null)
            {
                WaitStatement waitStatement = new WaitStatement(delay).UpdateCodePosition(context);
                // If not in the block, wrap it in the block statement
                if (body is BlockStatement blockStatement)
                {

                }
                else
                {
                    blockStatement = new BlockStatement(body).UpdateCodePosition(context);
                    body = blockStatement;
                }

                // Push front
                blockStatement.Statements.Insert(0, waitStatement);

                return new ProcessStatement(body)
                    .UpdateCodePosition(context).UpdateDocument(context, CommentParser);
            }
            
            return new ProcessStatement(body)
                .UpdateCodePosition(context).UpdateDocument(context, CommentParser);
        }

        /// <summary>
        /// statement_or_null:
        ///     statement
        ///     | ( attribute_instance )* SEMI
        /// ;
        /// </summary>
        public HDLStatement? VisitStatementOrNull(Statement_or_nullContext context)
        {
            if (context == null)
                return null;

            var statementContext = context.statement();
            if (statementContext != null)
            {
                return VisitStatement(statementContext);
            }

            AttributeParser.VisitAttributeInstance(context.attribute_instance());
            return new NopStatement().UpdateCodePosition(context);
        }

        /// <summary>
        /// continuous_assign:
        ///  KW_ASSIGN ( ( drive_strength )? ( delay3 )? list_of_variable_assignments
        ///               | delay_control list_of_variable_assignments
        ///               ) SEMI;
        /// </summary>
        public IEnumerable<HDLStatement> VisitContinuousAssign(Continuous_assignContext context)
        {
            var driveStrengthContext = context.drive_strength();
            if (driveStrengthContext != null)
            {
#warning Drive strength is not implemented now
            }

            var delay3Context = context.delay3();
            if (delay3Context != null)
            {
#warning Delay3 is not implemented now
            }

            var delayContext = context.delay_control();
            if (delayContext != null)
            {
#warning Delay control is not implemented now
            }

            var varAssignContext = context.list_of_variable_assignments();
            var variableAssignments = varAssignContext.variable_assignment();
            // list_of_variable_assignments: variable_assignment ( COMMA variable_assignment )*;
            // Document assigned from top

            foreach (var varContext in variableAssignments)
            {
                AssignStatement varAssign = VisitVariableAssignment(varContext);
                varAssign.IsBlocking = false;
                yield return varAssign;
            }
        }

        /// <summary>
        /// initial_construct: KW_INITIAL statement_or_null;
        /// </summary>
        public ProcessStatement VisitInitialConstruct(Initial_constructContext context)
        {
            var bodyContext = context.statement_or_null();
            HDLStatement? body = VisitStatementOrNull(bodyContext);

            // We are adding the wait statement at the end
            // to note that this is an initial process (construct)
            if (body is BlockStatement blockStatement)
            {

            }
            else
            {
                blockStatement = new BlockStatement(body).UpdateCodePosition(context);
                body = blockStatement;
            }

            WaitStatement endWaitStatement = new WaitStatement();
            endWaitStatement.UpdateCodePosition(context);

            blockStatement.Statements.Add(endWaitStatement);

            return new ProcessStatement(body).UpdateCodePosition(context);
        }

        /// <summary>
        /// sequence_match_item:
        ///     operator_assignment
        ///     | expression
        /// ;
        /// </summary>
        private HDLObject VisitSequenceMatchItem(Sequence_match_itemContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            var opAssignContext = context.operator_assignment();

            Expression expression = (opAssignContext != null) ?
                                    expressionParser.VisitOperatorAssignment(opAssignContext) :
                                    expressionParser.VisitExpression(context.expression());

            return new ExpressionStatement(expression).UpdateCodePosition(context);
        }

        /// <summary>
        /// elaboration_system_task:
        ///     ( KW_DOLAR_FATAL ( LPAREN UNSIGNED_NUMBER ( COMMA ( list_of_arguments )? )? RPAREN )?
        ///      | ( KW_DOLAR_ERROR
        ///          | KW_DOLAR_WARNING
        ///          | KW_DOLAR_INFO
        ///          ) ( LPAREN ( list_of_arguments )? RPAREN )?
        ///     ) SEMI
        /// ;
        /// </summary>
        public HDLStatement VisitElaborationSystemTask(Elaboration_system_taskContext context)
        {
            string name = false switch
            {
                _ when (context.KW_DOLAR_FATAL() != null) => "$fatal",
                _ when (context.KW_DOLAR_ERROR() != null) => "$error",
                _ when (context.KW_DOLAR_WARNING() != null) => "$warning",
                _ when (context.KW_DOLAR_INFO() != null) => "$info",
                _ => throw new Exception("Excepted an elaboration system task")
            };

            List<Expression> arguments = new List<Expression>();

            var uintContext = context.UNSIGNED_NUMBER();
            if (uintContext != null)
            {
                arguments.Add(LiteralParser.VisitUnsignedNumber(uintContext));
            }

            var argumentContext = context.list_of_arguments();
            if (argumentContext != null)
            {
                new ExpressionParser(this).VisitArguments(argumentContext, arguments);
            }

            Operator callOperator = Operator.Call(new Identifier(name).UpdateCodePosition(context),
                arguments);

            return new ExpressionStatement(callOperator).UpdateCodePosition(context);
        }

        /// <summary>
        ///     X ( COLON identifier | {_input->LA(1) != COLON}? )
        ///         ( block_item_declaration )* ( statement_or_null )*
        ///     X (COLON identifier |  {_input->LA(1) != COLON}?);
        /// </summary>
        private BlockStatement VisitBlock<T>(T context) where T : ParserRuleContext
        {
            var labelContext = context.GetRuleContext<IdentifierContext>(0);
            var blockItemDeclarationContext = context.GetRuleContexts<Block_item_declarationContext>();
            var statementOrNullContext = context.GetRuleContexts<Statement_or_nullContext>();

            List<HDLObject> items = new List<HDLObject>();
            foreach (var blkItem in blockItemDeclarationContext)
            {
                VisitBlockItemDeclaration(blkItem, items);
            }

            foreach (var statementOrNull in statementOrNullContext)
            {
                HDLStatement? statement = VisitStatementOrNull(statementOrNull);
                if (statement != null)
                {
                    items.Add(statement);
                }
            }

            BlockStatement blockStatement = new BlockStatement(items)
                                            .UpdateCodePosition(context)
                                            .UpdateDocument(context, CommentParser);

            if (labelContext != null)
            {
                blockStatement.Labels.Add(ExpressionParser.GetIdentifierString(labelContext));
            }

            return blockStatement;
        }
    }
}
