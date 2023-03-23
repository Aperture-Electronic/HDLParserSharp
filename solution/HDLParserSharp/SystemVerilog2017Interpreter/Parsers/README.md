# System Verilog 2017 Parser
## Introduction
These files are the core parser to convert ANTLR4 context to Abstract Syntax Tree (AST).
These files are re-constructed by hdlConverter (i.e. the C# version of hdlConverter).

## TODO list
We have several files/functions in hdlConvertor not re-wrote now.
And we are working hard to complete them.

The re-wrote files/functions are list as follows

| File / Function | Completed |
| -- | -- |
| Attribute Parser | OK |
| Comment Parser | OK |
| Declaration Parser | OK |
| Delay Parser | OK |
| Event Expression Parser | OK |
| Expression Parser | OK |
| Expression Primary Parser | OK |
| Gate Parser | OK |
| Generate Parser | OK |
| Literal Parser | OK |
| Module Instance Parser | OK |
| Module Parser | OK |
| Parameter Definition Parser | OK |
| Port Parser | OK |
| Program Parser | OK |
| Source Text Parser | OK |
| Statement Parser | OK |
| Type Parser | OK |



## Not implemented features
Some of System Verilog features are not implemented now. 
We add the #warning statement that can let IntelliSense throw the warning.
We are working hard to try to implement these features in future.

The features not implemented are list as follows

| Feature | Associated Statement | File | Status |
| -- | -- | -- | -- |
|Array method name|Expression(primary)|ExpressionPrimaryParser.cs||
|Asseration Item|Generate|GenerateParser.cs||
|Assignment pattern expression type|Expression(primary)|ExpressionPrimaryParser.cs||
|Assignment pattern variable left value|Expression|ExpressionParser.cs||
|Attribute instance of named port connection|Module Instance|ModuleInstanceParser.cs||
|Attribute instance of ordered port connection|Module Instance|ModuleInstanceParser.cs||
|Bi-direction arrow|Literal|LiteralParser.cs||
|Bind directive|Source, Generate|SourceTextParser.cs, GenerateParser.cs||
|Case statement inside|Statement|StatementParser.cs||
|Charge strength|Module|ModuleParser.cs||
|Checker declaration|Generate|GenerateParser.cs||
|Class constructor declaration|Generate|GenreateParser.cs||
|Class declaration|Generate|GenreateParser.cs||
|Class scope name of task/function declaration|Program|ProgramParser.cs||
|Clocking declaration|Generate|GenerateParser.cs||
|Clocking drive statement|Statement|StatementParser.cs||
|Clocking event|Expression(primary)|ExpressionPrimaryParser.cs||
|Configuration declaration|Source|SourceTextParser.cs||
|Constant expression assignment patternExpression(primary)|ExpressionPrimaryParser.cs||
|Conversion of non-ANSI ports|Port|PortParser.cs||
|Cover group declaration|Generate|GenerateParser.cs||
|Cycle delay|Delay|DelayParser.cs||
|Delay 2 sentence|Gate|GateParser.cs||
|Delay 3 sentence|Module, Statement|ModuleParser.cs, StatementParser.cs||
|Delay or event control|Expression, Statement|ExpressionParser.cs, StatementParser.cs||
|Default clocking/disable construct|Generate|GenerateParser.cs||
|Disable statement|Statement|StatementParser.cs||
|DPI import/export|Generate|GenerateParser.cs||
|Drive strength|Module, Statement, Gate|ModuleParser.cs, StatementParser.cs, GateParser.cs||
|Edge type identifier|EventExpression|EventExpressionParser.cs|Not fully implemented||
|Event trigger statement|Statement|StatementParser.cs||
|Expect property statement|Statement|StatementParser.cs||
|Extern constraint declaration|Generate|GenerateParser.cs||
|Gate type (expect output gate)|Gate|GateParser.cs||
|Final construct|Generate|GenerateParser.cs||
|Hierarchical name of task/function declaration|Program|ProgramParser.cs||
|Inside expression|Expression|ExpressionParser.cs||
|Integer type of assignment pattern key|Expression(primary)|ExpressionPrimaryParser.cs||
|Interconnect type (declaration)|Module|ModuleParser.cs||
|Interface/class declaration|Generate|GenerateParser.cs||
|Interface declaration|Module, Source|Parser.cs, SourceTextParser.cs||
|Let declaration|Generate, Statement|GenerateParser.cs, StatementParser.cs||
|Mathces condition predicate|Expression|ExpressionParser.cs||
|Matches expression is not implemented now|Expression|ExpressionParser.cs||
|Mintypmax expression (type and max specified)|Expression|ExpressionParser.cs||
|Module lifetime|Module|ModuleParser.cs||
|Multiple event expression item|EventExpression|EventExpressionParser.cs||
|Name of N output gate instance|Gate|GateParser.cs||
|Net alias|Generate|GenerateParser.cs||
|New class blocking assignment|Statement|StatementParser.cs||
|New dynamic array of blocking assignment|Statement|StatementParser.cs||
|Non-integer type of assignment pattern key|Expression(primary)|ExpressionPrimaryParser.cs||
|Non-ANSI ports of task/function|Program|ProgramParser.cs||
|Operator assignment|Expression|ExpressionParser.cs||
|Package declaration|Source|SourceTextParser.cs||
|Pakcage import declaration|Module|ModuleParser.cs||
|Package item declaration|Source|SourceTextParser.cs||
|Primary cast|Expression(primary)|ExpressionPrimaryParser.cs||
|Primary streaming concatenation|Expression(primary)|ExpressionPrimaryParser.cs||
|Procedural assertion statement|Statement|StatementParser.cs||
|Procedural continuous assignment|Statement|StatementParser.cs||
|Program declaration|Module, Source|ModuleParser.cs, SourceTextParser.cs||
|Property declaration|Generate|GenerateParser.cs||
|Randomize|Expression(primary)|ExpressionPrimaryParser.cs||
|Randomize2|Expression(primary)|ExpressionPrimaryParser.cs||
|Random case statement||Statement|StatementParser.cs||
|Random sequence statement|Statement|StatementParser.cs||
|Range cycle delay|Delay|DelayParser.cs||
|Repeat delay or event control|Delay|DelayParser.cs||
|Sequence declaration|Generate|GenerateParser.cs||
|Specify block|Module|ModuleParser.cs||
|Specparam declaration|Module|ModuleParser.cs||
|Streaming concatenation|Expression|ExpressionParser.cs||
|Tagged expression|Expression|ExpressionParser.cs||
|Time literal|Literal|LiteralParser.cs||
|Time units declaration|Module, Source|ModuleParser.cs, SourceTextParser.cs||
|Triple AND condition predicate|Expression|ExpressionParser.cs||
|Triple AND expression is not implemented now|Expression|ExpressionParser.cs||
|User defined primtive declaration|Source|SourceTextParser.cs||
|Vectored net declaration|Module|ModuleParser.cs||
|Wait statement|Statement|StatementParser.cs||
|With primary call|Expression(primary)|ExpressionPrimaryParser.cs||

