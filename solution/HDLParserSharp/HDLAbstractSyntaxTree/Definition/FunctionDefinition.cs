using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Definition
{
    public class FunctionDefinition : IdentifierDefinition
    {
        public Expression ReturnType { get; }
        public List<IdentifierDefinition> Arguments { get; } = new List<IdentifierDefinition>();
        public List<HDLObject> Body { get; } = new List<HDLObject>();

        public bool IsOperator { get; set; }

        public bool IsVirtual { get; set; }

        public bool IsTask { get; set; }

        public bool IsDeclarationOnly { get; set; }

        public FunctionDefinition(string name, bool isOperator,
            Expression returnType, IEnumerable<IdentifierDefinition> arguments) : base(name)
        {
            ReturnType = returnType;
            IsStatic = false;
            IsVirtual = false;
            IsTask = false;
            IsOperator = isOperator;
            IsDeclarationOnly = true;
            Arguments.AddRange(arguments);
        }

        public FunctionDefinition(string name, bool isOperator,
            Expression returnType) : base(name)
        {
            ReturnType = returnType;
            IsStatic = false;
            IsVirtual = false;
            IsTask = false;
            IsOperator = isOperator;
            IsDeclarationOnly = true;
        }
    }
}
