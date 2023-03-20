using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class ExpressionStatement : HDLStatement
    {
        public Expression Expression { get; }

        public ExpressionStatement(Expression expression) => Expression = expression;
    }
}
