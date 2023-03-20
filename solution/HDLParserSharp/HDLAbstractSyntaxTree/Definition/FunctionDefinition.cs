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
        public List<IdentifierDefinition>? Arguments { get; }
        public List<HDLObject>? Body { get; }

        public bool IsOperator { get; }

        public bool IsVirtual { get; }

        public bool IsTask { get; }

        public bool IsDeclarationOnly { get; }

        public FunctionDefinition(string name, bool isOperator,
            Expression returnType, List<IdentifierDefinition>? arguments) : base(name)
        {
            ReturnType = returnType;
            Arguments = arguments;
            IsStatic = false;
            IsVirtual = false;
            IsTask = false;
            IsOperator = isOperator;
            IsDeclarationOnly = true;
            Arguments = arguments;
        }
    }
}
