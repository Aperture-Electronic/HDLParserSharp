using HDLAbstractSyntaxTree.Elements;
using HDLElaborateRoslyn.Expressions;
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
namespace HDLElaborateRoslyn.Elaborator
{
    public class HDLEvaluator
    {
        private ScriptOptions options;
        public DynamicIdentifierSet IdentifierSet { get; } = new DynamicIdentifierSet();

        private DynamicGlobal dynamicGlobal;

        public HDLEvaluator()
        {
            options = ScriptOptions.Default;
            options = options.AddReferences(typeof(CSharpArgumentInfo).Assembly);
            options = options.AddReferences(typeof(BitLogic).Assembly);
            options = options.AddImports($"{nameof(HDLElaborateRoslyn)}.{nameof(HDLLibrary)}");

            dynamicGlobal = new DynamicGlobal(IdentifierSet);
        }

        public bool EvalToBool(Expression expression)
            => EvalToBool(ExpressionToSharp.ToSharp(expression));

        public int EvalToInteger(Expression expression)
            => EvalToInteger(ExpressionToSharp.ToSharp(expression));

        public object? EvalToAny(Expression expression)
            => EvalToAny(ExpressionToSharp.ToSharp(expression));

        private bool EvalToBool(string sharpSentence)
        {
            object? result = null;

            try
            {
                result = CSharpScript.EvaluateAsync(sharpSentence, options: options, globals: dynamicGlobal).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception when evaluate{ex.Message}");
            }

            ClearEvaluationSpace();

            return result is bool boolResult && boolResult;
        }

        private int EvalToInteger(string sharpSentence)
        {
            object? result = null;

            try
            {
                result = CSharpScript.EvaluateAsync(sharpSentence, options: options, globals: dynamicGlobal).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when evaluate");
                Console.WriteLine(ex.Message);
            }

            ClearEvaluationSpace();

            return result is HDLInteger intResult ? intResult : 0;
        }

        private object? EvalToAny(string sharpSentence)
        {
            object? result = null;

            try
            {
                result = CSharpScript.EvaluateAsync(sharpSentence, options: options, globals: dynamicGlobal).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception when evaluate");
                Console.WriteLine(ex.Message);
            }

            ClearEvaluationSpace();

            return result;
        }

        public static void ClearEvaluationSpace()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
