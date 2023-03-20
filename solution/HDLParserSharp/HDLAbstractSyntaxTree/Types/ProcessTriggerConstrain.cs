using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Types
{
    public enum ProcessTriggerConstrain
    {
        /// <summary>
        /// Trigger like <c>always</c>
        /// </summary>
        Always,
        /// <summary>
        /// Trigger like <c>always_comb</c>
        /// </summary>
        AlwaysCombinational,
        /// <summary>
        /// Trigger like <c>always_ff</c>
        /// </summary>
        AlwaysFlipFlop,
        /// <summary>
        /// Trigger like <c>always_latch</c>
        /// </summary>
        AlwaysLatch
    }
}
