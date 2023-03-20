using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Expressions
{
    public class Namespace : HDLCodedObject
    {
        /// <summary>
        /// Indicate the namespace (package) only has definition
        /// </summary>
        public bool DefinitionsOnly { get; } 

        public List<HDLObject>? Object { get; }
    }
}
