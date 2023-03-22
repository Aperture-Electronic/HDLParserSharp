using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    /// <summary>
    /// Statement of DO...WHILE loops
    /// </summary>
    public class DoWhileStatement : HDLStatement
    {
        public Expression Condition { get; }
        public HDLStatement? Body { get; }

        public DoWhileStatement(Expression condition, HDLStatement? body)
        {
            Condition = condition;
            Body = body;
        }
    }
}
