using HDLAbstractSyntaxTree.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.BasicUnit
{
    /// <summary>
    /// Indicate the position in the source code
    /// </summary>
    public interface ICodePosition
    {
        public ValueRange<int> Line { get; set; }
        public ValueRange<int> Column { get; set; }
    }
}
