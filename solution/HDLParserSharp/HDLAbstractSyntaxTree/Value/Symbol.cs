using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    public class Symbol : Expression
    {
        public SymbolType Type { get; }

        public Symbol(SymbolType type) => Type = type;

        public override Expression Clone() => new Symbol(Type);
    }
}
