using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class ProcessStatement : HDLStatement
    {
        /// <summary>
        /// The trigger constrain of the process
        /// </summary>
        public ProcessTriggerConstrain TriggerConstrain { get; }

        /// <summary>
        /// Sensitivity list of the process, which trig the process when the conditions meet
        /// </summary>
        public List<Expression> SensitivityList { get; }

        /// <summary>
        /// Process body
        /// </summary>
        public HDLStatement? Body { get; }

        public ProcessStatement(List<Expression> sensitivityList, HDLStatement? body)
        {
            TriggerConstrain = ProcessTriggerConstrain.Always;
            SensitivityList = sensitivityList;
            Body = body;
        }
    }
}
