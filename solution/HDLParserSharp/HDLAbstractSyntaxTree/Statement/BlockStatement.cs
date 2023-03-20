using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class BlockStatement : HDLStatement
    {
        BlockJoinType JoinType { get; set; }
        List<HDLObject> Statements { get; set; } = new List<HDLObject>();

        public BlockStatement(List<HDLObject> statements) => Statements.AddRange(statements);
    }
}
