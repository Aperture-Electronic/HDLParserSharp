using System;
using System.Collections.Generic;
using System.Text;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Value;

namespace HDLAbstractSyntaxTree.Definition
{
    /// <summary>
    /// Module definition
    /// <para>In Verilog, it is the body of <c>module</c></para>
    /// <para>In VHDL, it is the body of <c>ARCHITECTURE</c></para>
    /// </summary>
    public class ModuleDefinition : HDLCodedObject
    { 
        public new string Name
        {
            get
            {
                if (ModuleName is Identifier moduelIdentifier)
                {
                    return moduelIdentifier.Name;
                }

                return string.Empty;
            }
        }

        public Expression? ModuleName { get; set; }

        public ModuleDeclaration? Entity { get; set; }

        public List<HDLObject> Objects { get; }

        public ModuleDefinition()
        {
            Objects = new List<HDLObject>();
            Entity = null;
            ModuleName = null;
        }

        public ModuleDefinition(Expression nameExpression, ModuleDeclaration entity, List<HDLObject> objects)
        {
            ModuleName = nameExpression;
            Entity = entity;
            Objects = objects;
        }
    }
}
