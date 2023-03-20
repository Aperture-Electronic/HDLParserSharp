using HDLAbstractSyntaxTree.BasicUnit;
using HDLAbstractSyntaxTree.Common;
using HDLAbstractSyntaxTree.Elements;
using HDLAbstractSyntaxTree.HDLElement;
using HDLAbstractSyntaxTree.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Definition
{
    /// <summary>
    /// A HDL object defined an identifier
    /// </summary>
    public class IdentifierDefinition : HDLCodedObject
    {
        public Expression? Type { get; set; }

        public Expression? Value { get; set; }

        public bool IsLatched { get; set; }
        public bool IsConstant { get; set; }
        public bool IsStatic { get; set; }
        public bool IsShared { get; set; }

        public Direction Direction { get; set; }

        public IdentifierDefinition(string identifier, Expression? type, Expression? value, Direction direction, bool isLatched)
        {
            Name = identifier;
            Type = type;
            Value = value;

            IsLatched = isLatched;
            IsConstant = false;
            IsShared = false;
            IsShared = false;

            Direction = direction;
        }

        public void UpdateWith(IdentifierDefinition declaration, bool updateStatic = false)
        {
            Type = declaration.Type;
            Value = declaration.Value;
            IsLatched |= declaration.IsLatched;
            IsConstant |= declaration.IsConstant;
            if (Direction == Direction.Unknown)
            {
                Direction = declaration.Direction;
            }

            if (updateStatic)
            {
                IsStatic |= declaration.IsStatic;
            }
        }

        public IdentifierDefinition(string identifier, Expression? type, Expression? value) :
            this(identifier, type, value, Direction.Internal, false)
        {

        }

        public IdentifierDefinition(string identifier) :
            this(identifier, null, null)
        {

        }
    }
}
