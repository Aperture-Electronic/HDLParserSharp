using HDLAbstractSyntaxTree.BasicUnit;
using HDLAbstractSyntaxTree.Common;
using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Definition
{
    public class EnumerateDefinition : Expression, IDocumented
    {
        public string Document { get; set; } = string.Empty;

        public Expression? BaseType { get; set; }

        public Dictionary<string, Expression> Values { get; } = new Dictionary<string, Expression>();

        public EnumerateDefinition()
        {

        }

        public EnumerateDefinition(IDictionary<string, Expression> values)
        {
            Values.AddRange(values);
        }

        public override Expression Clone() => new EnumerateDefinition(Values)
        {
            Document = Document,
            BaseType = BaseType
        };
    }
}
