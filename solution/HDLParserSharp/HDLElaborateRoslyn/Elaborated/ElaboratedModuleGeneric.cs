using HDLAbstractSyntaxTree.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDLElaborateRoslyn.Elaborated
{
    public class ElaboratedModuleGeneric : ICloneable
    {
        public string Name { get; }

        /// <summary>
        /// The name in dynamic global identifier set
        /// </summary>
        public string GlobalName => $"hdl_id_{Name}"; 

        public Expression? DefaultValueExpression { get; }

        public object? UserSetValue { get; set; }

        public bool UserSetted => UserSetValue != null;

        public object? Value => UserSetted ? UserSetValue : defaultValue;

        public object? DefaultValue => defaultValue;

        public Type DefaultValueType => defaultValue?.GetType() ?? typeof(object);

        private object? defaultValue;

        public void ElaborateValue(Func<Expression, object?> evaluator)
        {
            if (DefaultValueExpression != null)
            {
                defaultValue = evaluator(DefaultValueExpression);
            }
        }

        public ElaboratedModuleGeneric(string name, Expression? defaultExpr)
        {
            Name = name;
            DefaultValueExpression = defaultExpr;
        }

        object ICloneable.Clone()
        {
            return new ElaboratedModuleGeneric(Name, DefaultValueExpression?.Clone());
        }
    }
}
