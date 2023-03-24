using HDLElaborateRoslyn.HDLLibrary;
using HDLElaborateRoslyn.RoslynDynamic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HDLElaborateRoslyn
{
    public class HDLElaborator
    {
        private ScriptOptions options;
        public DynamicIdentifierSet IdentifierSet { get; } = new DynamicIdentifierSet();

        public HDLElaborator()
        {
            options = ScriptOptions.Default;
            options = options.AddReferences(typeof(CSharpArgumentInfo).Assembly);
            options = options.AddReferences(typeof(BitLogic).Assembly);
            options = options.AddImports($"{nameof(HDLElaborateRoslyn)}.{nameof(HDLLibrary)}");
        }

        public bool EvalToBool(string sharpSentence)
        {
            object? result = null;

            try
            {
                result = CSharpScript.EvaluateAsync(sharpSentence, options: options).Result;
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
            }

            return result is bool boolResult ? boolResult : false;
        }

        public int EvalToInteger(string sharpSentence)
        {
            Stopwatch sw = Stopwatch.StartNew();

            IdentifierSet.AddOrModifyIdentifier("hdl_id_DATA_WIDTH", 5);
            DynamicGlobal dGlobal = new DynamicGlobal(IdentifierSet);

            object? result = null;

            try
            {
                result = CSharpScript.EvaluateAsync(sharpSentence, options: options, globals: dGlobal).Result;
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

            return result is HDLInteger intResult ? intResult : 0;
        }
    }
}
