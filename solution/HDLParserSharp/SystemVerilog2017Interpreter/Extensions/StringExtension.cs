using System;
using System.Collections.Generic;
using System.Text;

namespace SystemVerilog2017Interpreter.Extensions
{
    internal static class StringExtension
    {
        public static string WithOutVerilogNumberSeparator(this string str)
            => str.Replace("_", "");

        public static int ToNumberInVerilog(this string str)
            => int.Parse(str.WithOutVerilogNumberSeparator());
    }
}
