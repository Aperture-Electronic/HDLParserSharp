using HDLAbstractSyntaxTree.BasicUnit;
using HDLAbstractSyntaxTree.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.HDLElement
{
    public abstract class HDLCodedObject : HDLObject, ICoded
    {
        /// <summary>
        /// Object name
        /// </summary>
        public string Name { get; set; } = string.Empty;

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
