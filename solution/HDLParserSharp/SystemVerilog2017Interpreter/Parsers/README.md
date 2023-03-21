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
| Delay Parser | **NO** |
| Expression Parser | **NO** |
| Expression Primary Parser | **NO** |
| Generate Parser | OK |
| Literal Parser | OK |
| Module Instance Parser | **NO** |
| Module Parser | OK |
| Parameter Definition Parser | OK |
| Port Parser | OK |
| Program Parser | **NO** |
| Source Text Parser | OK |
| Statement Parser | **NO** |
| Type Parser | OK |



## Not implemented features
Some of System Verilog features are not implemented now. 
We add the #warning statement that can let IntelliSense throw the warning.
We are working hard to try to implement these features in future.

The features not implemented are list as follows

| Feature | Associated Statement | File |
| -- | -- | -- |
|Asseration Item|Generate|GenerateParser.cs|
|Bi-direction arrow|Literal|LiteralParser.cs|
|Bind directive|Source|SourceTextParser.cs|
|Charge strength|Module|ModuleParser.cs|
|Checker declaration|Generate|GenerateParser.cs|
|Clocking declaration|Generate|GenerateParser.cs|
|Configuration declaration|Source|SourceTextParser.cs|
|Cover group declaration|Generate|GenerateParser.cs|
|Delay 3 sentence|Module|ModuleParser.cs|
|Drive strength|Module|ModuleParser.cs|
|Interconnect type (declaration)|Module|ModuleParser.cs|
|Interface declaration|Module, Source|Parser.cs, SourceTextParser.cs|
|Let declaration|Generate|GenerateParser.cs|
|Module lifetime|Module|ModuleParser.cs|
|Package declaration|Source|SourceTextParser.cs|
|Pakcage import declaration|Module|ModuleParser.cs|
|Package item declaration|Source|SourceTextParser.cs|
|Program declaration|Module, Source|ModuleParser.cs, SourceTextParser.cs|
|Property declaration|Generate|GenerateParser.cs|
|Sequence declaration|Generate|GenerateParser.cs|
|Specify block|Module|ModuleParser.cs|
|Specparam declaration|Module|ModuleParser.cs|
|Time literal|Literal|LiteralParser.cs|
|Time units declaration|Module, Source|ModuleParser.cs, SourceTextParser.cs|
|User defined primtive declaration|Source|SourceTextParser.cs|
|Vectored net declaration|Module|ModuleParser.cs|

