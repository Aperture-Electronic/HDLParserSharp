using HDLAbstractSyntaxTree.Definition;
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemVerilog2017Interpreter.Context
{
    public class ModuleContext
    {
        public Dictionary<string, IdentifierDefinition> NonANSIPortGroups { get; }
        public ModuleDeclaration Entity { get; }
        public ModuleDefinition? Architecture { get; set; }

        public ModuleContext(ModuleDeclaration declaration)
        {
            NonANSIPortGroups = new Dictionary<string, IdentifierDefinition>();
            Entity = declaration;
            Architecture = null;
        }
    }
}
