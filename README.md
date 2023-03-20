# HDLParserSharp
## Introduction
HDLParser# is a hardware description language (HDL) parser wrote in C#.

Basiclly, the core of language processing unit of the HDLParser# is C# implemetation of hdlConvertor.
With the ANTLR4 C# library, we use the Lexer and Parser files in hdlConvertor to generate the C# library of HDLLexer and HDLParser.
And, we construct a C# class for ASTs, using many modern and efficiency C# language features.
Then, we re-write the parser library in C# to generate the ANTLR4 objects to the AST.

## The hdlConvertor library
The [Nic30/hdlConvertor](https://github.com/Nic30/hdlConvertor) is a System Verilog and VHDL parser library (MIT License), 
which contains 
* ANTLR4 generated VHDL/(System) Verilog parser with full language support;
* Convertors from raw VHDL/SV AST to universal HDL AST;
* Convertors from this HDL AST to SV/VHDL/JSON and other formats;
* Compiler focused utils for manipulation with HDL AST.

## Why we do this work
The C# is a very modern and efficiency OOP language, contains many useful attributes and features to make programming easily.
The libraries of C# is also very rich, such as System.Linq can help you use simple query sentence to query the enumerable data.
We'd like to apply these features for HDL parser then we can construct many HDL applications by C#, such as Linter and BlockDesigner.

## What we plan to do?
We plan to reproduce the work of hdlConvertor completely and try to add new features to it (Such as the interface in SystemVerilog is unsupported now in hdlConverter).
And, we plan to use Lua (or some script languages, like Python) to evaluate some expressions in the AST, then we can get some result in "synthesis-time".
Finally, we plan to write a BlockDesigner in C# like the Xilinx Block Design, it can make we connect wires and buses easily when we design the top or high-level entries.


