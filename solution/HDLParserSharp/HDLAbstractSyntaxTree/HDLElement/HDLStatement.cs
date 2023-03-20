using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.HDLElement
{
    /// <summary>
    /// Statement of HDL, includes multiple types of blocks.
    /// </summary>
    public abstract class HDLStatement : HDLCodedObject
    {
        /// <summary>
        /// An optional extra label specified in HDL
        /// </summary>
        public List<string> Labels { get; } = new List<string>();

        /// <summary>
        /// Indicate the statement is the part of compile time evaluated statement
        /// </summary>
        public bool IsInPreprocess { get; set; }
    }
}
