using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.HDLElement;
using HDLElaborateRoslyn.Elaborated;
using HDLParserBase;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.Elaborator
{
    public class HDLElaborator
    {
        private List<HDLObject> ast;
        private List<ElaboratedModule> modules = new List<ElaboratedModule>();

        public IEnumerable<ElaboratedModule> Modules => modules.AsEnumerable();

        public HDLElaborator(List<HDLObject> ast)
        {
            this.ast = ast;
        }

        public void ElaborateModules()
        {
            modules = (from node in ast
                      where node is ModuleDefinition
                      select new ElaboratedModule((ModuleDefinition)node)).ToList();
        }

        public void GenerateModuleGenericsList()
        {
            Parallel.ForEach(modules, delegate (ElaboratedModule module)
            {
                module.ElaboratedModuleGenerics.Clear();
                if (module.Entity is not null)
                {
                    foreach (var generic in module.Entity.Generics)
                    {
                        ElaboratedModuleGeneric eGeneric = new ElaboratedModuleGeneric(generic.Name, generic.Value);
                        module.ElaboratedModuleGenerics.Add(eGeneric);
                    }
                }
            });
        }

        public void ElaborateModuleGenerics()
        {
            Parallel.ForEach(modules, delegate (ElaboratedModule module)
            {
                HDLEvaluator evaluator = new HDLEvaluator();

                foreach (var generic in module.ElaboratedModuleGenerics)
                {
                    // Elaborate
                    generic.ElaborateValue(evaluator.EvalToAny);
                    evaluator.IdentifierSet.AddOrModifyIdentifier(generic.GlobalName, generic.Value);
                }
            });
        }

        /// <summary>
        /// Use default generic value to elaborate module port
        /// </summary>
        public void ElaborateModulePort()
        {
            Parallel.ForEach(modules, delegate (ElaboratedModule module)
            {
                if (module.Entity is not null) 
                {
                    module.ElaboratedModulePorts.Clear();
                    foreach (var port in module.Entity.Ports)
                    {
                        if (port != null)
                        {
                            ElaboratedModulePort ePort = new ElaboratedModulePort(port);
                            module.ElaboratedModulePorts.Add(ePort);
                        }
                    }
                }

                Parallel.ForEach(module.ElaboratedModulePorts, delegate (ElaboratedModulePort port)
                {
                    // Make a evaluator with all generics
                    HDLEvaluator evaluator = new HDLEvaluator();
                    foreach (var generic in module.ElaboratedModuleGenerics)
                    {
                        evaluator.IdentifierSet.AddOrModifyIdentifier(generic.GlobalName, generic.Value);
                    }

                    // Elaborate size
                    port.ElaborateSize(evaluator.EvalToInteger);
                });
            });
        }
    }
}
