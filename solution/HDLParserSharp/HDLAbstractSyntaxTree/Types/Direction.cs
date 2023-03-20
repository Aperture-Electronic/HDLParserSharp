using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Types
{
    /// <summary>
    /// Directions in HDL
    /// </summary>
    public enum Direction
    {
        In,
        Out,
        Inout,
        Buffer,
        Linkage,
        Internal,
        Unknown
    }
}
