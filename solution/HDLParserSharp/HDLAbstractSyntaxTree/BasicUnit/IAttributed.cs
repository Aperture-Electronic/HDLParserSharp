using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Value;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.BasicUnit
{
    internal interface IAttributed
    {
        public Dictionary<Identifier, Expression> Attributes { get; }
    }
}
