using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class WaitStatement : HDLStatement
    {
        List<Expression> Values { get; } = new List<Expression>();

        public WaitStatement(Expression value) => Values.Add(value);

        public WaitStatement(List<Expression> values) => Values.AddRange(values);

        public WaitStatement() { }
    }
}
