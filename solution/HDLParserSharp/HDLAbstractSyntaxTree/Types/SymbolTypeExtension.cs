using HDLAbstractSyntaxTree.Value;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Types
{
    public static class SymbolTypeExtension
    {
        public static Symbol AsNewSymbol(this SymbolType type)
            => new Symbol(type);
    }
}
