using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class BlockStatement : HDLStatement
    {
        public BlockJoinType JoinType { get; set; }
        public List<HDLObject> Statements { get; } = new List<HDLObject>();

        public BlockStatement() 
        {
            JoinType = BlockJoinType.Sequential;
        }

        public BlockStatement(List<HDLObject> statements) : this() => Statements.AddRange(statements);

        public BlockStatement(HDLObject? statement) : this()
        {
            if (statement != null)
            {
                Statements.Add(statement);
            }
        }
    }
}
