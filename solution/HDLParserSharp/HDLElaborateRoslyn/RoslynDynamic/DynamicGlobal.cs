using System;
using System.Collections.Generic;
using System.Text;

namespace HDLElaborateRoslyn.RoslynDynamic
{
    public class DynamicGlobal
    {
        public dynamic Global { get; }

        public DynamicGlobal(DynamicIdentifierSet identifierSet) => Global = identifierSet;
    }
}
