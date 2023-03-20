using HDLAbstractSyntaxTree.BasicUnit;
using HDLAbstractSyntaxTree.Common;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Elements
{
    /// <summary>
    /// An expression in the HDL
    /// </summary>
    public abstract class Expression : HDLObject, ICodePosition
    {
        public abstract Expression Clone();

        public ValueRange<int> Line { get; set; }
        public ValueRange<int> Column { get; set; }
    }
}
