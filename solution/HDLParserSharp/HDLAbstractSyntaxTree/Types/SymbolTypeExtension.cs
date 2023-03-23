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

        public static string GetSymbolString(this SymbolType type)
            => type switch
            {
                SymbolType.Null => "NULL",
                SymbolType.Open => "OPEN",
                SymbolType.All => ".*",
                SymbolType.Others => "OTHERS",
                SymbolType.Type => "<T>",
                SymbolType.Auto => "auto",
                SymbolType.SubType => "SUBTYPE",
                _ => throw new Exception("Invalid symbol")
            };
    }
}
