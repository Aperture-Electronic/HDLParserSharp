using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class ImportStatement : HDLStatement
    {
        List<Expression> Paths { get; }

        public ImportStatement(List<Expression> paths) => Paths = paths;
    }
}
