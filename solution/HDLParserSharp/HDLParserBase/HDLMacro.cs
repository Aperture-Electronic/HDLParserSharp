using System;
using System.Collections.Generic;
using System.Text;

namespace HDLParserBase
{
    public class HDLMacro
    {
        public string Name { get; }

        public string ReplacementCode { get; }

        public HDLMacro(string name, string replacementCode)
        {
            Name = name;
            ReplacementCode = replacementCode;
        }
    }
}
