using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Expressions
{
    public class ComponentInstance : HDLUnnamedObject
    {
        public Expression Name { get; }
        public Expression ModuleName { get; }
        public List<Expression> GenericMap { get; } = new List<Expression>();
        public List<Expression> PortMap { get; } = new List<Expression>();

        public ComponentInstance(Expression name, Expression moduleName)
        {
            Name = name;
            ModuleName = moduleName;
        }
    }
}
