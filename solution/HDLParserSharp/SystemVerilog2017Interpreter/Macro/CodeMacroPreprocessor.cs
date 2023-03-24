using Antlr4.Runtime;
using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Statement;
using HDLParserBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SystemVerilog2017;
using SystemVerilog2017Interpreter.Parsers;

namespace SystemVerilog2017Interpreter.Macro
{
    public class CodeMacroPreprocessor : HDLMacroPreprocessor
    {
        public CodeMacroPreprocessor(Func<Expression, bool> macroIfParser, string path = ".")
            : base(macroIfParser, path)
        {

        }

        private static string GenerateIfVerilogModule(string expression)
            => $"module IF(); assign MACRO = ({expression}); endmodule";

        private static List<HDLObject> GetAstFromExpression(string expression)
        {
            List<HDLObject> objects = new List<HDLObject>();
            var inputStream = new AntlrInputStream(expression);
            var lexer = new SystemVerilog2017Lexer(inputStream);
            var tokens = new CommonTokenStream(lexer);
            var parser = new SystemVerilog2017Parser(tokens);
            ITokenStream tokenStream = parser.TokenStream;
            var interpreter = new SourceTextParser(tokenStream, objects, true);
            interpreter.VisitSourceText(parser.source_text());
            return objects;
        }

        private static Expression GetMacroIfExpression(List<HDLObject> ast)
        {
            // Get first module
            ModuleDefinition define = (ModuleDefinition)ast[0];
            AssignStatement statement = (AssignStatement)define.Objects[0];
            return statement.Source;
        }

        private bool CheckMacroIfExpression(string expression)
        {
            string code = GenerateIfVerilogModule(expression);
            var ast = GetAstFromExpression(code);
            var expr = GetMacroIfExpression(ast);
            return MacroIfParser(expr);
        }

        public override string VisitMacro(string code)
        {
            // Macros
            // `define:  Define a new macro, with or without value
            // `include: Include (insert) all codes in the file into the postion
            // `ifdef:   Check if the macro is defined
            // `if:      Check the expression is true or false
            // `elsif:   Else check the expression is true or false
            // `else:    Else
            // `endif:   End if block
            // `IDENT:   Use macro

            List<string> codeLines = code.Split('\n').ToList();
            bool disableMode = false;
            bool multiLineCommentMode = false;
            bool continueScan = false;
            string line= string.Empty;
            Stack<bool> ifLevelSolved = new Stack<bool>();
            void MakeSolved()
            {
                ifLevelSolved.Pop();
                ifLevelSolved.Push(true);
            }

            for (int i = 0; i < codeLines.Count;)
            {
                if (!continueScan)
                {
                    line = codeLines[i].Trim();
                }
                {
                    continueScan = false;
                }

                // Replace in-line "multi-line" comment to empty
                Match inlineMLCommentMatch = Regex.Match(line, @"/\*.*\*/");
                if (inlineMLCommentMatch.Success)
                {
                    line = line.Replace(inlineMLCommentMatch.Value, string.Empty);
                }

                // Find and delete in-line comment
                int commentPosition = line.IndexOf("//");
                line = (commentPosition >= 0) ? line[0..commentPosition].Trim() : line.Trim();

                
                if (multiLineCommentMode)
                {
                    commentPosition = line.IndexOf("*/");
                    if (commentPosition >= 0)
                    {
                        if (commentPosition + 2 < line.Length)
                        {
                            line = line[(commentPosition + 2)..].Trim();
                            multiLineCommentMode = false;
                            continueScan = true; // Continue scan
                        }
                        else
                        {
                            multiLineCommentMode = false;
                            i++; // The line is over, go to next line
                            
                        }

                        continue;
                    }
                    else
                    {
                        i++;
                        continue;
                    }
                }
                else if (disableMode)
                {
                    // Delete all lines in disable mode
                    codeLines.RemoveAt(i);

                    // Replace all macro identifiers
                    foreach (var macro in MacroContext)
                    {
                        line = line.Replace($"`{macro.Name}", macro.ReplacementCode);
                    }

                    // In disable mode, we only return when meet `elsif, `else and `endif
                    Match elsifMatch = Regex.Match(line, @"^[\s]*`elsif(.*)$");
                    if (elsifMatch.Success)
                    {
                        if (!ifLevelSolved.Peek())
                        {
                            string ifExpression = elsifMatch.Groups[1].Value.Trim();

                            codeLines.RemoveAt(i);

                            if (CheckMacroIfExpression(ifExpression))
                            {
                                // If true
                                MakeSolved();
                                disableMode = false;
                                continue;
                            }
                            else
                            {
                                // If false
                                disableMode = true;
                                continue;
                            }
                        }
                        else
                        {
                            // The current level of if block is solved
                            disableMode = true;
                            continue;
                        }
                    }

                    Match endifMatch = Regex.Match(line, @"^[\s]*`endif$");
                    if (endifMatch.Success)
                    {
                        ifLevelSolved.Pop();
                        disableMode = false; // Exit the disable mode
                        continue; 
                    }

                    Match elseMatch = Regex.Match(line, @"^[\s]*`else$");
                    if (elseMatch.Success)
                    {
                        if (!ifLevelSolved.Peek())
                        {
                            disableMode = false; // Exit the disable mode
                            continue;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    commentPosition = line.IndexOf("/*");
                    if (commentPosition >= 0)
                    {
                        line = line[0..commentPosition].Trim();
                        multiLineCommentMode = true;
                    }

                    // Replace all macro identifiers
                    foreach (var macro in MacroContext)
                    {
                        line = line.Replace($"`{macro.Name}", macro.ReplacementCode);
                    }

                    Match ifdefMatch = Regex.Match(line, @"^[\s]*`ifdef[\s]+(.*)$");
                    if (ifdefMatch.Success)
                    {
                        string requireMacro = ifdefMatch.Groups[1].Value;
                        if (MacroContext.Where(e => e.Name == requireMacro).Any())
                        {   
                            ifLevelSolved.Push(true);
                        }
                        else
                        {
                            disableMode = true;
                            ifLevelSolved.Push(false);
                        }

                        codeLines.RemoveAt(i); // Delete the macro line

                        
                        continue;
                    }

                    Match ifndefMatch = Regex.Match(line, @"^[\s]*`ifndef[\s]+(.*)$");
                    if (ifndefMatch.Success)
                    {
                        string requireMacro = ifndefMatch.Groups[1].Value;
                        if (!MacroContext.Where(e => e.Name == requireMacro).Any())
                        {
                            ifLevelSolved.Push(true);
                        }
                        else
                        {
                            disableMode = true;
                            ifLevelSolved.Push(false);
                        }

                        codeLines.RemoveAt(i); // Delete the macro line
                        continue;
                    }

                    Match ifMatch = Regex.Match(line, @"^[\s]*`if(.*)$");
                    if (ifMatch.Success)
                    {
                        string ifExpression = ifMatch.Groups[1].Value.Trim();

                        codeLines.RemoveAt(i);  

                        if (CheckMacroIfExpression(ifExpression))
                        {
                            // If true
                            ifLevelSolved.Push(true);
                            continue;
                        }
                        else
                        {
                            // If false
                            ifLevelSolved.Push(false);
                            disableMode = true;
                            continue;
                        }
                    }

                    Match elsifMatch = Regex.Match(line, @"^[\s]*`elsif(.*)$");
                    if (elsifMatch.Success)
                    {
                        if (!ifLevelSolved.Peek())
                        {
                            string ifExpression = elsifMatch.Groups[1].Value.Trim();

                            codeLines.RemoveAt(i);

                            if (CheckMacroIfExpression(ifExpression))
                            {
                                // If true
                                MakeSolved();
                                disableMode = false;
                                continue;
                            }
                            else
                            {
                                // If false
                                disableMode = true;
                                continue;
                            }
                        }
                        else
                        {
                            // The current level of if block is solved
                            disableMode = true;
                            continue;
                        }
                    }

                    Match elseMatch = Regex.Match(line, @"^[\s]*`else$");
                    if (elseMatch.Success && ifLevelSolved.Peek())
                    {
                        codeLines.RemoveAt(i);
                        disableMode = true;
                        continue;
                    }

                    Match endifMatch = Regex.Match(line, @"^[\s]*`endif$");
                    if (endifMatch.Success)
                    {
                        ifLevelSolved.Pop();
                        codeLines.RemoveAt(i); // Delete the macro line
                        continue;
                    }

                    Match includeMatch = Regex.Match(line, @"^[\s]*`include[\s]+[""<](.*)["">]$");
                    if (includeMatch.Success)
                    {
                        // Include file, read it and get content insert to the position
                        string path = includeMatch.Groups[1].Value;
                        path = Path.Combine(ContextFilePath, path);
                        string[] fileLinesToInsert = File.ReadAllLines(path);

                        codeLines.RemoveAt(i);
                        codeLines.InsertRange(i, fileLinesToInsert);

                        continue;
                    }

                    Match defineMatch = Regex.Match(line, @"^[\s]*`define[\s]+(.*)$");
                    if (defineMatch.Success)
                    {
                        // Define a new macro
                        // Check if there is a value
                        string macroDefine = defineMatch.Groups[1].Value;
                        string[] macroNameAndValue = macroDefine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (macroNameAndValue.Length == 1)
                        {
                            MacroContext.Add(new HDLMacro(macroNameAndValue[0], ""));
                        }
                        else
                        {
                            MacroContext.Add(new HDLMacro(macroNameAndValue[0].Trim(), macroNameAndValue[1].Trim()));
                        }

                        codeLines.RemoveAt(i); // Delete the macro line
                        continue;
                    }

                    i++; // Go to next line
                }
            }


            return string.Join('\n', codeLines);
        }
    }
}
