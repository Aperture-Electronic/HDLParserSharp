using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    public class Identifier : Expression
    {
        public string Name { get; set; }

        public Identifier(string name) => Name = name;

        public override Expression Clone() => new Identifier(Name);

        public override string ToString() => Name;
    }
}
