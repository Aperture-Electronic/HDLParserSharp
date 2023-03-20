using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Value
{
    public class String : Expression
    {
        public string Content { get; }

        public String(string content) => Content = content;

        public override Expression Clone() => new String(Content);  
    }
}
