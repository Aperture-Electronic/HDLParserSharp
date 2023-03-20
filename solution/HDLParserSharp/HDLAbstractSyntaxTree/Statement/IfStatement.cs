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
    public class IfStatement
    {
        public Expression Condition { get; }
        public HDLObject TrueBody { get; }
        public Dictionary<Expression, HDLObject>? ElseIfBodies { get; }
        public HDLObject? FalseBody { get; }

        public IfStatement(Expression condition, HDLObject trueBody)
        {
            Condition = condition;
            TrueBody = trueBody;
        }

        public IfStatement(Expression condition, HDLObject trueBody, HDLObject falseBody)
            : this(condition, trueBody)
        {
            FalseBody = falseBody;
        }

        public IfStatement(Expression condition, HDLObject trueBody, 
            Dictionary<Expression, HDLObject> elseIfBodies, HDLObject falseBody)
            : this(condition, trueBody, falseBody)
        {
            ElseIfBodies = elseIfBodies;
        }
    }
}
