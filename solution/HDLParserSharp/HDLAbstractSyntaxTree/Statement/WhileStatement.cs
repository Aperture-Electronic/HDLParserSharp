using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    /// <summary>
    /// Statement of WHILE loops
    /// </summary>
    public class WhileStatement : HDLStatement
    {
        public Expression Condition { get; }
        public HDLStatement? Body { get; }

        public WhileStatement(Expression condition, HDLStatement? body)
        {
            Condition = condition;
            Body = body;
        }
    }
}
