using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    /// <summary>
    /// Statements of IF statement
    /// </summary>
    public class IfStatement : HDLStatement
    {
        public Expression Condition { get; set; }
        public HDLObject? TrueBody { get; set;  }
        public Dictionary<Expression, HDLObject?> ElseIfBodies { get; }
        public HDLObject? FalseBody { get; set; }

        public IfStatement(Expression condition, HDLObject? trueBody)
        {
            Condition = condition;
            TrueBody = trueBody;
            ElseIfBodies = new Dictionary<Expression, HDLObject?>();
            FalseBody = null;
        }

        public IfStatement(Expression condition, HDLObject? trueBody, HDLObject? falseBody)
            : this(condition, trueBody)
        {
            FalseBody = falseBody;
        }

        public IfStatement(Expression condition, HDLObject? trueBody, 
            Dictionary<Expression, HDLObject?> elseIfBodies, HDLObject? falseBody)
            : this(condition, trueBody, falseBody)
        {
            ElseIfBodies = elseIfBodies;
        }
    }
}
