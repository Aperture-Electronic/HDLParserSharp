using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Types
{
    public enum SymbolType
    {
        /// <summary>
        /// Unconnected
        /// </summary>
        Null,

        /// <summary>
        /// Open-loop
        /// </summary>
        Open,

        /// <summary>
        /// All items in destination
        /// </summary>
        All,

        /// <summary>
        /// Not explicitly specified items in destination
        /// </summary>
        Others,

        /// <summary>
        /// Type of type (generic)
        /// </summary>
        Type,

        /// <summary>
        /// Automatically derived type (implicit data type from Verilog)
        /// </summary>
        Auto,

        /// <summary>
        /// Used in subtype declaration as data type of symbol
        /// </summary>
        SubType
    }
}
