using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HDLAbstractSyntaxTree.HDLElement;

namespace HDLAbstractSyntaxTree.Definition
{
    public class ModuleDeclaration : HDLCodedObject
    {
        /// <summary>
        /// Generics of the module
        /// </summary>
        public List<IdentifierDefinition> Generics { get; set; } = new List<IdentifierDefinition>();

        /// <summary>
        /// Ports of the module
        /// </summary>
        public List<IdentifierDefinition> Ports { get; set; } = new List<IdentifierDefinition>();

        /// <summary>
        /// Objects in the module
        /// </summary>
        public List<HDLObject> Objects { get; set; } = new List<HDLObject>();

        /// <summary>
        /// Get a module port by its name
        /// If the name of port existed, then return true
        /// </summary>
        /// <param name="name">Port's name</param>
        /// <param name="port">Corresponding port</param>
        public IdentifierDefinition? GetPortByName(string name)
        {
            var query = from p in Ports where p.Name == name select p;

            if (query.Any())
            {
                return query.First();
            }

            return null;
        }
    }
}
