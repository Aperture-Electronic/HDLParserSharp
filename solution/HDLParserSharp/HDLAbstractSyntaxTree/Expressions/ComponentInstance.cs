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

        public void SetGenericMap(IEnumerable<Expression>? map)
        {
            if (map != null)
            {
                GenericMap.Clear();
                GenericMap.AddRange(map);
            }
        }

        public void SetPortMap(IEnumerable<Expression>? portMap)
        {
            if (portMap != null)
            {
                PortMap.Clear();
                PortMap.AddRange(portMap);
            }
        }
    }
}
