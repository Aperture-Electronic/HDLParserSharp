using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace HDLElaborateRoslyn
{
    public class HDLElaborator
    {

        public int EvalToInteger(string sharpSentence)
        {
            Stopwatch sw = Stopwatch.StartNew();
            
            object? result = null;
            try
            {
                result = CSharpScript.EvaluateAsync(sharpSentence).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when evaluate");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                sw.Stop();
                Console.WriteLine($"Complie and execute used {sw.ElapsedMilliseconds} ms");
            }

            if (result is int intResult)
            {
                return intResult;
            }

            return 0;
        }
    }
}
