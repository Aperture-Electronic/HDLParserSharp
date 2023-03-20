using HDLAbstractSyntaxTree.BasicUnit;
using HDLAbstractSyntaxTree.Common;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Definition
{
    public class PhysicalDefinition : Expression, IDocumented
    {
        public string Document { get; set; } = string.Empty;

        public Expression? Range { get; set; }

        public Dictionary<string, Expression> Members { get; } = new Dictionary<string, Expression>();

        public PhysicalDefinition()
        {

        }

        public PhysicalDefinition(IDictionary<string, Expression> members)
        {
            Members.AddRange(members);
        }

        public override Expression Clone() => new PhysicalDefinition(Members)
        {
            Document = Document,
            Range = Range
        };
    }
}
