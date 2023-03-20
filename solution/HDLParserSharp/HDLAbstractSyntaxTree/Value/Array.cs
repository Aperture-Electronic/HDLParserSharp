using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    /// <summary>
    /// Arrays in HDL AST
    /// </summary>
    public class Array : Expression
    {
        public List<Expression> Values { get; }

        public Array(List<Expression> values) => Values = values;

        public override Expression Clone() => new Array(Values);
    }
}
