using HDLAbstractSyntaxTree.Definition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.Elaborated
{
    public class ElaboratedModule
    {
        public string Name => Architecture.Name;

        public ModuleDefinition Architecture { get; }

        public ModuleDeclaration? Entity => Architecture.Entity;

        public List<ElaboratedModuleGeneric> ElaboratedModuleGenerics { get; }

        public List<ElaboratedModulePort> ElaboratedModulePorts { get; }

        public IEnumerable<string> GenericsName =>
            from g in ElaboratedModuleGenerics
            select g.Name;

        public void SetUserGeneric(string name, object? value)
        {
            var query = from g in ElaboratedModuleGenerics
                        where g.Name == name
                        select g;

            if (query.Any())
            {
                var existedGeneric = query.First();
                existedGeneric.UserSetValue = value;
            }
        }

        public ElaboratedModule(ModuleDefinition architecture)
        {
            Architecture = architecture;
            ElaboratedModuleGenerics = new List<ElaboratedModuleGeneric>();
            ElaboratedModulePorts = new List<ElaboratedModulePort>();
        }
    }
}
