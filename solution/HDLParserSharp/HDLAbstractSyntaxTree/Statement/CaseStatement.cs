using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;

namespace HDLAbstractSyntaxTree.Statement
{
    public class CaseStatement : HDLStatement
    {
        public CaseUniqueConstrain UniqueConstrain { get; set; }
        
        public CaseType Type { get; set; }

        public Expression SelectOn { get; set; }

        public Dictionary<Expression, HDLObject> Cases { get; set; }

        public HDLObject? Default { get; set; }

        public CaseStatement(CaseType type, Expression selectOn, Dictionary<Expression, HDLObject> cases, HDLObject? defaultCase)
        {
            UniqueConstrain = CaseUniqueConstrain.None;
            Type = type;
            SelectOn = selectOn;
            Cases = cases;
            Default = defaultCase;
        }

        public CaseStatement(CaseType type, Expression selectOn, Dictionary<Expression, HDLObject> cases)
            : this(type, selectOn, cases, null)
        {

        }

        public CaseStatement(Expression selectOn, Dictionary<Expression, HDLObject> cases)
            : this(CaseType.Case, selectOn, cases)
        {
            
        }

        public CaseStatement(Expression selectOn, Dictionary<Expression, HDLObject> cases, HDLObject? defaultCase)
            : this(CaseType.Case, selectOn, cases, defaultCase)
        {

        }


    }
}
