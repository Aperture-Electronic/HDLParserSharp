using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.BasicUnit
{
    /// <summary>
    /// HDL object with name, document and code position
    /// </summary>
    public interface ICoded : INamed, IDocumented, ICodePosition
    {

    }
}
