using HDLAbstractSyntaxTree.BasicUnit;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Definition
{
    public class ClassDefinition : Expression, IDocumented
    {
        public string Document { get; set; } = string.Empty;

        public List<Expression> BaseTypes { get; } = new List<Expression>();

        public List<HDLObject> Members { get; } = new List<HDLObject>();

        public ClassType Type { get; set; }    

        public bool IsPacked { get; set; }

        public bool IsVirtual { get; set; }

        public ClassDefinition()
        {

        }

        public ClassDefinition(IEnumerable<Expression> baseTypes, IEnumerable<HDLObject> members)
        {
            BaseTypes.AddRange(baseTypes);
            Members.AddRange(members);
        }

        public override Expression Clone() => new ClassDefinition(BaseTypes, Members)
        {
            Document = Document,
            Type = Type,
            IsPacked = IsPacked,
            IsVirtual = IsVirtual
        };
    }
}
