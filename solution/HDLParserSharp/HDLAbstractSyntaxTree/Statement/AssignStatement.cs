using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Statement
{
    public class AssignStatement : HDLStatement
    {
        /// <summary>
        /// Left-side of the assign statement
        /// </summary>
        public Expression Destination { get; }

        /// <summary>
        /// Right-side of the assign statement
        /// </summary>
        public Expression Source { get; }

        /// <summary>
        /// Assign time delay (not synthesizable)
        /// </summary>
        public Expression? TimeDelay { get; }

        /// <summary>
        /// Event delay (not synthesizable)
        /// </summary>
        public List<Expression>? EventDelay { get; }

        public bool IsBlocking { get; }

        public AssignStatement(Expression source, Expression destination,
            Expression? timeDelay, List<Expression>? eventDelay, bool isBlocking)
        {
            Source = source;
            Destination = destination;
            TimeDelay = timeDelay;
            EventDelay = eventDelay;
            IsBlocking = isBlocking;
        }

        public AssignStatement(Expression source, Expression destination, bool isBlocking)
            : this(source, destination, null, null, isBlocking)
        {

        }
    }
}
