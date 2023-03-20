using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Expressions
{
    public class Library : HDLCodedObject
    {
        public Library(string name)
        {
            Name = name;
        }
    }
}
