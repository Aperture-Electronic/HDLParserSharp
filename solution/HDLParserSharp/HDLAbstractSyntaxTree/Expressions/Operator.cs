using HDLAbstractSyntaxTree.Elements    ;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Expressions
{
    public class Operator : Expression
    {
        public OperatorType Type { get; set; }
        public List<Expression> Operands { get; } = new List<Expression>();

        public Operator(OperatorType type) => Type = type;
        
        private Operator(OperatorType type, IEnumerable<Expression> operands) : this(type)
        {
            Operands.AddRange(operands);
        }

        public Operator(OperatorType type, Expression singleOperand) : this(type)
        {
            Operands.Add(singleOperand);
        }

        public Operator(OperatorType type, Expression leftOperand, Expression rightOperand) :
            this(type, leftOperand)
        {
            Operands.Add(rightOperand);
        }

        private Operator(OperatorType type, Expression expression, IEnumerable<Expression> operands)
        {
            Type = type;
            Operands.Add(expression);
            Operands.AddRange(operands);
        }

        public static Operator Index(Expression expression, IEnumerable<Expression> operands)
            => new Operator(OperatorType.Index, expression, operands);  

        public static Operator Call(Expression expression, IEnumerable<Expression> operands)
            => new Operator(OperatorType.Call, expression, operands);

        public static Operator Call(Expression expression, Expression operand)
            => new Operator(OperatorType.Call, expression, operand);

        public static Operator Parametrization(Expression expression, IEnumerable<Expression> operands)
            => new Operator(OperatorType.Parametrization, expression, operands);

        public static Operator Ternary(Expression condition, Expression ifTrue, Expression? ifFalse)
        {
            Operator op = new Operator(OperatorType.Ternary);
            op.Operands.Add(condition);
            op.Operands.Add(ifTrue);

            if (ifFalse != null)
            {
                op.Operands.Add(ifFalse);
            }

            return op;
        }
    
        public static Expression Reduce(IEnumerable<Expression> operators, OperatorType concatType = OperatorType.Concat)
        {
            Expression? res = null;
            foreach (Operator op in operators)
            {
                if (res == null)
                {
                    res = op;
                }
                else
                {
                    res = new Operator(concatType, op, res);
                }
            }

            return res ?? throw new Exception("Operators list empty");
        }

        public override Expression Clone() => new Operator(Type, Operands);
    }
}
