using HDLAbstractSyntaxTree.BasicUnit;
using HDLAbstractSyntaxTree.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.HDLElement
{
    public class HDLUnnamedObject : HDLObject, IDocumented, ICodePosition
    {
        /// <summary>
        /// Object document (comments, etc.)
        /// </summary>
        public string Document { get; set; } = string.Empty;

        /// <summary>
        /// Corresponding source line (range)
        /// </summary>
        public ValueRange<int> Line { get; set; }

        /// <summary>
        /// Corresponding source column (range)
        /// </summary>
        public ValueRange<int> Column { get; set; }
    }
}
