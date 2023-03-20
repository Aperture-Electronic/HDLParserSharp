using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    /// <summary>
    /// Statements of REPEAT loops, it repeat a specific times
    /// </summary>
    public class RepeatStatement : HDLStatement
    {
        Expression RepeatCount { get; }
        HDLObject Body { get; }

        public RepeatStatement(Expression repeatCount, HDLObject body)
        {
            RepeatCount = repeatCount;
            Body = body;
        }
    }
}
