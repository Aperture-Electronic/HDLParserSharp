using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    public class Real : Expression
    {
        public double Value { get; }

        public Real(double value) => Value = value;

        public override Expression Clone() => new Real(Value);
    }
}
