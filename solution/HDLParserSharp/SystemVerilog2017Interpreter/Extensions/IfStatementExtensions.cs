using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Statement;
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemVerilog2017Interpreter.Extensions
{
    public static class IfStatementExtensions
    {
        public static void CollapseElseIf(this IfStatement ifStatement)
        {
            if (ifStatement.FalseBody is IfStatement elseStatement)
            {
                if (elseStatement.IsInPreprocess != ifStatement.IsInPreprocess)
                {
                    throw new Exception("Collapse if statement must be same preprocess status");
                }

                // Add if-then branch as if else
                Expression condition = elseStatement.Condition;
                HDLObject? elseBody = elseStatement.TrueBody;

                ifStatement.ElseIfBodies.Add(condition, elseBody);
                foreach (var elseIfs in elseStatement.ElseIfBodies)
                {
                    ifStatement.ElseIfBodies.Add(elseIfs.Key, elseIfs.Value);
                }

                ifStatement.FalseBody = elseStatement.FalseBody;

                // TODO: labels (if there was label for else, merge it with first label if there was any)
                // foreach (var lebel in elseStatment.Labels)
                // {
                //
                // }

                if (elseStatement.Document.Length > 0)
                {
                    ifStatement.Document += (ifStatement.Document.Length > 0) ? "\n" : string.Empty;
                    ifStatement.Document += elseStatement.Document;                                                
                }
            }
        }
    }
}
