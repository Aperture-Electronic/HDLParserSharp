using HDLAbstractSyntaxTree.Definition;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    /// <summary>
    /// Statements of FOR EACH iterations
    /// </summary>
    public class ForEachStatement : HDLStatement
    {
        public List<HDLObject> Variables = new List<HDLObject>();
        public Expression Collection;
        public HDLObject Body;

        public ForEachStatement(IdentifierDefinition variable, Expression collection, HDLObject body)
        {
            Variables.Add(variable);
            Collection = collection;
            Body = body;
        }

        public ForEachStatement(Expression variable, Expression collection, HDLObject body)
        {
            Variables.Add(variable);
            Collection = collection;
            Body = body;
        }

        public ForEachStatement(List<HDLObject> variables, Expression collection, HDLObject body)
        {
            Variables.AddRange(variables);
            Collection = collection;
            Body = body;
        }

        public ForEachStatement(List<Expression> variables, Expression collection, HDLObject body)
        {
            Variables.AddRange(variables);
            Collection = collection;
            Body = body;
        }
    }
}
