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
using SystemVerilog2017;
using SystemVerilog2017Interpreter.Extensions;
using static SystemVerilog2017.SystemVerilog2017Parser;

namespace SystemVerilog2017Interpreter.Parsers
{
    public class GenerateParser : HDLParser
    {
        public GenerateParser(HDLParser other) : base(other) { }

        internal void VisitGenerateRegion(Generate_regionContext context, List<HDLObject> objects)
        {
            // generate_region: KW_GENERATE ( generate_item )* KW_ENDGENERATE;
            List<HDLObject> generateItems = new List<HDLObject>();
            foreach (var generateItemContext in context.generate_item())
            {
                VisitGenerateItem(generateItemContext, generateItems);
                foreach (var item in generateItems)
                {
                    if (item is HDLStatement statement)
                    {
                        statement.IsInPreprocess = true;
                    }

                    objects.Add(item);
                }

                generateItems.Clear();
            }
        }

        /// <summary>
        /// module_or_generate_or_interface_or_checker_item:
        ///     function_declaration
        ///     | checker_declaration
        ///     | property_declaration
        ///     | sequence_declaration
        ///     | let_declaration
        ///     | covergroup_declaration
        ///     | genvar_declaration
        ///     | clocking_declaration
        ///     | initial_construct
        ///     | always_construct
        ///     | final_construct
        ///     | assertion_item
        ///     | continuous_assign
        /// ;
        /// </summary>
        public void VisitModuleOrGenerateOrInterfaceOrCheckerItem(Module_or_generate_or_interface_or_checker_itemContext context,
            List<HDLObject> objects)
        {
            StatementParser statementParser = new StatementParser(this);

            var funcDeclarationContext = context.function_declaration();
            if (funcDeclarationContext != null)
            {
                ProgramParser programParser = new ProgramParser(this);
                FunctionDefinition funcDefinition = programParser.VisitFunctionDeclaration(funcDeclarationContext);
                objects.Add(funcDefinition);
                return;
            }

            if (context.checker_declaration() != null)
            {
#warning Checker declaration is not implemented now
                return;
            }

            if (context.covergroup_declaration() != null)
            {
#warning Cover group declaration is not implemented now
                return;
            }

            if (context.property_declaration() != null)
            {
#warning Property declaration is not implemented now
                return;
            }

            if (context.sequence_declaration() != null)
            {
#warning Sequence declaration is not implemented now
                return;
            }

            if (context.let_declaration() != null)
            {
#warning Let declaration is not implemented now
                return;
            }

            var genvarDeclarationContext = context.genvar_declaration();
            if (genvarDeclarationContext != null)
            {
                VisitGenvarDeclaration(genvarDeclarationContext, objects);
                return;
            }

            if (context.clocking_declaration() != null)
            {
#warning Clocking declaration is not implemented now
                return;
            }

            if (context.assertion_item() != null)
            {
#warning Assertion item is not implemented now
            }

            var contAssignContext = context.continuous_assign();
            if (contAssignContext != null)
            {
                objects.AddRange(statementParser.VisitContinuousAssign(contAssignContext));
                return;
            }

            var initialConstructContext = context.initial_construct();
            if (initialConstructContext != null)
            {
                objects.Add(statementParser.VisitInitialConstruct(initialConstructContext));
                return;
            }

            var finalConstructContext = context.final_construct();
            if (finalConstructContext != null)
            {
#warning Final construct is not implemented now
                return;
            }

            var alwaysConstructContext = context.always_construct();
            if (alwaysConstructContext != null)
            {
                objects.Add(statementParser.VisitAlwaysConstruct(alwaysConstructContext));
                return;
            }

            throw new Exception("Excepted a module/interface/generate/checker item");
        }

        /// <summary>
        /// module_or_generate_or_interface_item:
        ///     module_or_interface_or_program_or_udp_instantiation
        ///     | ( default_clocking_or_dissable_construct
        ///         | local_parameter_declaration
        ///         | parameter_declaration
        ///     )? SEMI
        ///     | net_declaration
        ///     | data_declaration
        ///     | task_declaration
        ///     | module_or_generate_or_interface_or_checker_item
        ///     | dpi_import_export
        ///     | extern_constraint_declaration
        ///     | class_declaration
        ///     | interface_class_declaration
        ///     | class_constructor_declaration
        ///     | bind_directive
        ///     | net_alias
        ///     | loop_generate_construct
        ///     | conditional_generate_construct
        ///     | elaboration_system_task
        /// ;
        /// </summary>
        public void VisitModuleOrGenerateOrInterfaceItem(Module_or_generate_or_interface_itemContext context,
            List<HDLObject> objects)
        {
            ModuleInstanceParser moduleInstanceParser = new ModuleInstanceParser(this);
            ParameterDefinitionParser parameterDefinitionParser = new ParameterDefinitionParser(this);
            ModuleParser moduleParser = new ModuleParser(this);
            DeclarationParser declarationParser = new DeclarationParser(this);
            ProgramParser programParser = new ProgramParser(this);
            StatementParser statementParser = new StatementParser(this);

            var mdlInterfaceProgramUDPInstContext = context.module_or_interface_or_program_or_udp_instantiation();
            if (mdlInterfaceProgramUDPInstContext != null)
            {
                moduleInstanceParser.VisitAnyModuleInstantiation(mdlInterfaceProgramUDPInstContext, objects);
                return;
            }

            var defClockingDisableConstructContext = context.default_clocking_or_dissable_construct();
            if (defClockingDisableConstructContext != null)
            {
#warning Default clocking/disable construct is not implemented now
                return;
            }

            var localParameterDeclarationContext = context.local_parameter_declaration();
            if (localParameterDeclarationContext != null)
            {
                parameterDefinitionParser.VisitLocalParameterDeclaration(localParameterDeclarationContext, objects);
                return;
            }

            var paramDeclarationContext = context.parameter_declaration();
            if (paramDeclarationContext != null)
            {
                parameterDefinitionParser.VisitParameterDeclaration(paramDeclarationContext, objects);
                return;
            }

            if (context.SEMI() != null)
            {
                objects.Add(new NopStatement().UpdateCodePosition(context));
                return;
            }

            var netDeclarationContext = context.net_declaration();
            if (netDeclarationContext != null)
            {
                moduleParser.VisitNetDeclaration(netDeclarationContext, objects);
                return;
            }

            var dataDeclarationContext = context.data_declaration();
            if (dataDeclarationContext != null)
            {
                declarationParser.VisitDataDeclaration(dataDeclarationContext, objects);
                return;
            }

            var taskDeclarationContext = context.task_declaration();
            if (taskDeclarationContext != null)
            {
                objects.Add(programParser.VisitTaskDeclaration(taskDeclarationContext));
                return;
            }

            var anyModuleItemContext = context.module_or_generate_or_interface_or_checker_item();
            if (anyModuleItemContext != null)
            {
                VisitModuleOrGenerateOrInterfaceOrCheckerItem(anyModuleItemContext, objects);
                return;
            }

            var dpiImportExportContext = context.dpi_import_export();
            if (dpiImportExportContext != null)
            {
#warning DPI import/export in not implemented now
                return;
            }

            var extConstraintDeclarationContext = context.extern_constraint_declaration();
            if (extConstraintDeclarationContext != null)
            {
#warning Extern constraint declaration in not implemented now
                return;
            }

            var classDeclarationContext = context.class_declaration();
            if (classDeclarationContext != null)
            {
#warning Class declaration is not implemented now
                return;
            }

            var ifClassDeclarationContext = context.interface_class_declaration();
            if (ifClassDeclarationContext != null)
            {
#warning Interface/class declaration is not implemented now
                return;
            }

            var clsConstructorDeclarationContext = context.class_constructor_declaration();
            if (clsConstructorDeclarationContext != null)
            {
#warning Class constructor declaration is not implemented now
                return;
            }

            var bindDirectiveContext = context.bind_directive();
            if (bindDirectiveContext != null)
            {
#warning Bind directive is not implemented now
                return;
            }

            var netAliasContext = context.net_alias();
            if (netAliasContext != null)
            {
#warning Net alias is not implemented now
                return;
            }

            // generate for
            var loopGenerateConstructContext = context.loop_generate_construct();
            if (loopGenerateConstructContext != null)
            {
                objects.Add(VisitLoopGenerateConstruct(loopGenerateConstructContext));
                return;
            }

            // generate if
            var conditionalGenerateConstructContext = context.conditional_generate_construct();
            if (conditionalGenerateConstructContext != null)
            {
                objects.Add(VisitConditionalGenerateConstruct(conditionalGenerateConstructContext));
                return;
            }

            var elaborationSystemTaskContext = context.elaboration_system_task();
            if (elaborationSystemTaskContext != null)
            {
                objects.Add(statementParser.VisitElaborationSystemTask(elaborationSystemTaskContext));
                return;
            }

            throw new Exception("Excepted a module/generate/interface item");
        }

        /// <summary>
        /// module_or_generate_item:
        ///     parameter_override
        ///     | gate_instantiation
        ///     | udp_instantiation
        ///     | module_or_generate_or_interface_item
        /// ;
        /// </summary>
        public void VisitModuleOrGenerateItem(Module_or_generate_itemContext context, List<HDLObject> objects, IEnumerable<IdentifierDefinition> parameters)
        {
            var paramOverrideContext = context.parameter_override();
            if (paramOverrideContext != null)
            {
#warning Parameter override is not implemented now
                return;
            }

            var gateInstantiationContext = context.gate_instantiation();
            if (gateInstantiationContext != null)
            {
                new GateParser(this).VisitGateInstantiation(gateInstantiationContext, objects);
                return;
            }

            var udpInstantiationContext = context.udp_instantiation();
            if (udpInstantiationContext != null)
            {
#warning UDP instantiation is not implemented now
                return;
            }

            var anyModuleContext = context.module_or_generate_or_interface_item();
            if (anyModuleContext != null)
            {
                VisitModuleOrGenerateOrInterfaceItem(anyModuleContext, objects);
                return;
            }

            throw new Exception("Excepted a module/generate item");
        }

        /// <summary>
        /// genvar_declaration:
        ///  KW_GENVAR identifier_list SEMI;
        /// identifier_list: identifier ( COMMA identifier )*;
        /// </summary>
        private void VisitGenvarDeclaration(Genvar_declarationContext context, List<HDLObject> objects)
        {
            var identifierContext = context.identifier_list();
            foreach (var idContext in identifierContext.identifier())
            {
                string name = ExpressionParser.GetIdentifierString(idContext);
                Identifier identifier = new Identifier("genvar").UpdateCodePosition(idContext);
                IdentifierDefinition identifierDefinition =
                    new IdentifierDefinition(name, identifier, null).UpdateCodePosition(idContext);
                objects.Add(identifierDefinition);
            }
        }

        /// <summary>
        /// genvar_initialization:
	    ///     ( KW_GENVAR )? identifier ASSIGN constant_expression;
        /// </summary>
        private HDLObject VisitGenvarInitialization(Genvar_initializationContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            var defValueContext = context.constant_expression();
            var defaultValue = expressionParser.VisitConstantExpression(defValueContext);
            if (context.KW_GENVAR() != null)
            {
                string name = ExpressionParser.GetIdentifierString(context.identifier());
                Symbol dataType = SymbolType.Auto.AsNewSymbol();
                return new IdentifierDefinition(name, dataType, defaultValue, Direction.Internal, true)
                    .UpdateCodePosition(context);
            }
            else
            {
                var destination = ExpressionParser.VisitIdentifier(context.identifier());
                return new AssignStatement(defaultValue, destination, true)
                    .UpdateCodePosition(context)
                    .UpdateDocument(context, CommentParser);
            }
        }

        /// <summary>
        /// genvar_iteration:
        ///     identifier ( assignment_operator genvar_expression
        ///                  | inc_or_dec_operator
        ///                  )
        ///     | inc_or_dec_operator identifier
        /// ;
        /// </summary>
        public ExpressionStatement VisitGenvarIteration(Genvar_iterationContext context)
        {
            var identifierContext = context.identifier();
            var incDecreaseContext = context.inc_or_dec_operator();

            ExpressionParser expressionParser = new ExpressionParser(this);

            Expression identifier = ExpressionParser.VisitIdentifier(identifierContext);
            Expression expression;

            if (incDecreaseContext != null)
            {
                bool isPrefix = context.children.First() == incDecreaseContext;
                OperatorType op = LiteralParser.VisitIncreaseDecreaseOperator(incDecreaseContext, isPrefix);
                expression = new Operator(op, identifier).UpdateCodePosition(context);
            }
            else
            {
                var assignContext = context.assignment_operator();
                OperatorType op = LiteralParser.VisitAssignmentOperator(assignContext);
                var genvarExprContext = context.genvar_expression();
                Expression genvarExpression = VisitGenvarExpression(genvarExprContext);
                expression = new Operator(op, identifier, genvarExpression)
                    .UpdateCodePosition(context);
            }

            return new ExpressionStatement(expression).UpdateCodePosition(context);
        }

        /// <summary>
        /// genvar_expression: constant_expression;
        /// </summary>
        private Expression VisitGenvarExpression(Genvar_expressionContext context)
            => new ExpressionParser(this).VisitConstantExpression(context.constant_expression());

        /// <summary>
        /// loop_generate_construct:
        ///     KW_FOR LPAREN genvar_initialization SEMI genvar_expression SEMI genvar_iteration RPAREN
        ///       generate_item;
        /// </summary>
        private HDLObject VisitLoopGenerateConstruct(Loop_generate_constructContext context)
        {
            var initContext = context.genvar_initialization();
            var condContext = context.genvar_expression();
            var stepContext = context.genvar_iteration();

            HDLObject initStatement = VisitGenvarInitialization(initContext);
            BlockStatement initBlock = new BlockStatement(initStatement).UpdateCodePosition(context);

            Expression condExpression = VisitGenvarExpression(condContext);
            ExpressionStatement stepBlock = VisitGenvarIteration(stepContext);

            var generateItemContext = context.generate_item();
            BlockStatement generateBlock = new BlockStatement().UpdateCodePosition(generateItemContext);
            VisitGenerateItem(generateItemContext, generateBlock.Statements);
            HDLObject body = (generateBlock.Statements.Count == 1) ?
                             generateBlock.Statements.First() :
                             generateBlock;

            return new ForStatement(initBlock, condExpression, stepBlock, body)
                .UpdateCodePosition(context);
        }

        /// <summary>
        /// generate_begin_end_block:
        ///     ( identifier COLON )? KW_BEGIN ( COLON identifier | {_input->LA(1) != COLON}? )
        ///         ( generate_item )*
        ///     KW_END ( COLON identifier | {_input->LA(1) != COLON}? )
        /// ;
        ///     X ( COLON identifier | {_input->LA(1) != COLON}? )
        ///         ( block_item_declaration )* ( statement_or_null )*
        ///     X (COLON identifier |  {_input->LA(1) != COLON}?);
        /// </summary>
        private void VisitGenerateBeginEndBlock(Generate_begin_end_blockContext context, List<HDLObject> objects)
        {
            var generateItemContext = context.generate_item();
            List<HDLObject> items = new List<HDLObject>();

            foreach (var itemContext in generateItemContext)
            {
                VisitGenerateItem(itemContext, items);
            }

            var generateBlock = new BlockStatement(items).UpdateCodePosition(context);
            
            foreach (var labelContext in context.identifier())
            {
                generateBlock.Labels.Add(ExpressionParser.GetIdentifierString(labelContext));
            }

            objects.Add(generateBlock);
        }

        /// <summary>
        /// conditional_generate_construct:
        ///     if_generate_construct
        ///     | case_generate_construct
        /// ;
        /// </summary>
        private HDLObject VisitConditionalGenerateConstruct(Conditional_generate_constructContext context)
        {
            var ifGenerateContext = context.if_generate_construct();
            var caseGenerateContext = context.case_generate_construct();

            if (ifGenerateContext != null)
            {
                return VisitIfGenerateConstruct(ifGenerateContext);
            }
            
            if (caseGenerateContext != null)
            {
                return VisitCaseGenerateConstruct(caseGenerateContext);
            }

            throw new Exception("Expected a conditional generate construct");
        }

        /// <summary>
        /// case_generate_item:
        ///     ( KW_DEFAULT ( COLON )?
        ///       | constant_expression ( COMMA constant_expression )* COLON
        ///     ) generate_item;
        /// </summary>
        private IEnumerable<(Expression?, HDLObject?)> VisitCaseGenerateItem(Case_generate_itemContext context)
        {
            var constExprContext = context.constant_expression();
            var generateItemContext = context.generate_item();

            if (constExprContext.Any())
            {
                ExpressionParser expressionParser = new ExpressionParser(this);
                // List<HDLObject> generateItems = new List<HDLObject>();
                // VisitGenerateItem(generateItemContext, generateItems);

                // if (generateItems.Count != 1)
                // {
                //     throw new Exception("More than one generate item in context");
                // }

                foreach (var itemContext in constExprContext)
                {
                    Expression constExpression = expressionParser.VisitConstantExpression(itemContext);

                    // TODO: Clone the generate item (We have not a clone method now)
                    // If we have clone, we only need to visit it once
                    List<HDLObject> generateItems = new List<HDLObject>();
                    VisitGenerateItem(generateItemContext, generateItems);

                    if (generateItems.Count != 1)
                    {
                        throw new Exception("More than one generate item in context");
                    }

                    HDLObject generateObject = generateItems.First();
                    yield return (constExpression, generateObject);
                }
            }
            else
            {
                List<HDLObject> generateItems = new List<HDLObject>();
                VisitGenerateItem(generateItemContext, generateItems);
                if (generateItems.Count != 1)
                {
                    throw new Exception("More than one generate item in context");
                }

                yield return (null, generateItems.First());
            }
        }

        /// <summary>
        /// case_generate_construct:
	    ///     KW_CASE LPAREN constant_expression RPAREN ( case_generate_item )+ KW_ENDCASE;
        /// </summary>
        private HDLObject VisitCaseGenerateConstruct(Case_generate_constructContext context)
        {
            ExpressionParser expressionParser = new ExpressionParser(this);
            Expression selectOn = expressionParser.VisitConstantExpression(context.constant_expression());

            Dictionary<Expression, HDLObject?> cases = new Dictionary<Expression, HDLObject?>();
            HDLObject? defaultCase = null;

            var caseGenerateItemContext = context.case_generate_item();
            var caseItems = caseGenerateItemContext.SelectMany(i => VisitCaseGenerateItem(i));
            foreach ((Expression? expression, HDLObject? body) in caseItems)
            {
                if (expression != null)
                {
                    cases.Add(expression, body);
                }
                else
                {
                    if (defaultCase != null)
                    {
                        throw new Exception("Case with multiple default");
                    }

                    defaultCase = body;
                }
            }

            return new CaseStatement(CaseType.Case, selectOn, cases, defaultCase)
                .UpdateCodePosition(context)
                .UpdateDocument(context, CommentParser);
        }

        /// <summary>
        /// if_generate_construct:
        ///     KW_IF LPAREN constant_expression RPAREN generate_item
        ///     ( KW_ELSE generate_item | {_input->LA(1) != KW_ELSE}? );
        ///
        /// conditional_statement:
        ///    ( unique_priority )? KW_IF LPAREN cond_predicate RPAREN statement_or_null
        ///    ( KW_ELSE statement_or_null | {_input->LA(1) != KW_ELSE}? );
        /// </summary>
        private HDLObject VisitIfGenerateConstruct(If_generate_constructContext context)
        {
            var conditionContext = context.constant_expression();
            var genItemContext = context.generate_item();

            ExpressionParser expressionParser = new ExpressionParser(this);

            Expression condition = expressionParser.VisitConstantExpression(conditionContext);
            var ifTrueContext = genItemContext.First();
            BlockStatement ifTrueBlock = new BlockStatement().UpdateCodePosition(ifTrueContext);
            VisitGenerateItem(ifTrueContext, ifTrueBlock.Statements);
            HDLObject ifTrue = ifTrueBlock.PopBlockIfPossible();

            HDLObject? ifFalse = null;
            if (genItemContext.Length == 2)
            {
                var ifFalseContext = genItemContext.Last();
                BlockStatement ifFalseBlock = new BlockStatement().UpdateCodePosition(ifFalseContext);
                VisitGenerateItem(ifFalseContext, ifFalseBlock.Statements);
                ifFalse = ifFalseBlock.PopBlockIfPossible();    
            }

            IfStatement ifStatement = new IfStatement(condition, ifTrue, ifFalse)
                .UpdateCodePosition(context).UpdateDocument(context, CommentParser);

            ifStatement.CollapseElseIf();
            return ifStatement;
        }

        /// <summary>
        /// generate_item:
        ///     ( attribute_instance )*
        ///         ( module_or_generate_item
        ///           | extern_tf_declaration
        ///         )
        ///         | KW_RAND data_declaration
        ///         | generate_region
        ///         | generate_begin_end_block
        /// ;
        /// </summary>
        private void VisitGenerateItem(Generate_itemContext context, List<HDLObject> generateItems)
        {
            if (context.KW_RAND() != null)
            {
#warning Data declaration of generate item is not implemented now
                return;
            }

            var moduleGenerateItemContext = context.module_or_generate_item();
            if (moduleGenerateItemContext != null)
            {
                List<IdentifierDefinition> parameters = new List<IdentifierDefinition>();
                VisitModuleOrGenerateItem(moduleGenerateItemContext, generateItems, parameters);
                if (parameters.Any())
                {
#warning Parameters of module/generate item is not implemented now
                }

                return;
            }

            var extTaskFunctionDeclarationContext = context.extern_tf_declaration();
            if (extTaskFunctionDeclarationContext != null)
            {
#warning External task/function declaration is not implemented now
                return;
            }

            var generateRegionContext = context.generate_region();
            if (generateRegionContext != null)
            {
                VisitGenerateRegion(generateRegionContext, generateItems);
                return;
            }

            var generateBeginEndBlockContext = context.generate_begin_end_block();
            if (generateBeginEndBlockContext != null)
            {
                VisitGenerateBeginEndBlock(generateBeginEndBlockContext, generateItems);
                return;
            }

            throw new Exception("Excepted a generate item");
        }
    }
}
