using System;
using System.Collections.Generic;
using System.Text;

namespace HDLParserSharp
{
    public enum HDLLanguage
    {
        VHDL = 0,
        Verilog1995 = 1,
        Verilog2001 = 2,
        Verilog2001NoConfig = 3,
        Verilog2005 = 4,
        Verilog = Verilog2001,
        SystemVerilog2005 = 5,
        SystemVerilog2009 = 6,
        SystemVerilog2012 = 7,
        SystemVerilog2017 = 8,
        SystemVerilog = SystemVerilog2017,
        HWT = 9,
        Invalid = int.MaxValue,
    }
}
