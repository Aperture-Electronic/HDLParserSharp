using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    /// <summary>
    /// Statements of FOR loops
    /// </summary>
    public class ForStatement : HDLStatement
    {
        public HDLStatement Initial { get; set; }

        public Expression Condition { get; set; }

        public HDLStatement Step { get; set; }

        public HDLObject Body { get; set; }

        public ForStatement(HDLStatement initial, Expression condition, HDLStatement step, HDLObject body)
        {
            Initial = initial;
            Condition = condition;
            Step = step;
            Body = body;
        }
    }
}
