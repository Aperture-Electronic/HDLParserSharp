using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class ReturnStatement : HDLStatement
    {
        Expression? Value { get; }

        public ReturnStatement(Expression value) => Value = value;

        public ReturnStatement() => Value = null;   
    }
}
