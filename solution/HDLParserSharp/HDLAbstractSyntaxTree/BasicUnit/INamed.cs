using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.BasicUnit
{
    /// <summary>
    /// Named HDL object
    /// </summary>
    public interface INamed
    {
        public string Name { get; }
    }
}
