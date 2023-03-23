using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    /// <summary>
    /// HDL AST node which purpose is used if conversion of original expression is not implemented
    /// </summary>
    public class NotImplemented : Expression
    {
        public string Description { get; }

        public NotImplemented(string description = "") => Description = description;

        public override Expression Clone() => throw new NotImplementedException();

        public override string ToString() => $"Not Impl. ({Description})";
    }
}
