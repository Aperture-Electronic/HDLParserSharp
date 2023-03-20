using System;
using System.Collections.Generic;
using System.Text;

namespace HDLAbstractSyntaxTree.Types
{
    public enum OperatorType
    {
        // Arithmetic
        MinusUnary,
        PlusUnary,
        Sub, // -
        Add, // +
        Div, // /
        Mul, // *
        Mod, // % Modulo
        Rem, // Remainder
        Pow, // Power Of
        Abs, // Absolute Value
        IncrPre,  // ++X
        DecrPre,  // --X
        IncrPost,  // X--
        DecrPost,  // X++
                    // Bitwise Log. Operators
        NegLog,  // ~, Logical Negation "Not" In VHDL
        Neg,  // Bitwise Negation
        AndLog,  // "And" In VHDL, &&
        OrLog,  // "Or" In VHDL, ||
        And,  // & In VHDL
        Or,  // | In VHDL
        Nand,
        Nor,
        Xor,
        Xnor,

        // System Verilog Reduction Operators
        OrUnary,  // Or Reduction (|A)
        AndUnary,  // And Reduction (&A)
        NandUnary,  // Nand Reduction (~&A)
        NorUnary,  // Nor Reduction (~|A)
        XorUnary,  // Xor Reduction (^A)
        XnorUnary,  // And Reduction (~^A) Or (^~A)
                     // Shifts
        Sll,  // Shift Left Logical
        Srl,  // Shift Right Logical
        Sla,  // Shift Left Arithmetical
        Sra,  // Shift Right Arithmetical
        Rol,  // Rotate Left
        Ror,  // Rotate Right
              // Comparison Operators
        Eq,  // ==
        Ne,  // !=
        Is,  // ===
        IsNot, // !==
        Lt,  // <
        Le,   // <=
        Gt,  // >
        Ge,  // >=
             // VHDL-2008 Matching Ops (The X Values Are Ignored While Match)
        EqMatch, // System Verilog ==?
        NeMatch, // System Verilog !=?
        LtMatch,
        LeMatch,
        GtMatch,
        GeMatch,

        // Member Accessing
        Index,  // Array Index
        Concat,  // Concatenation Of Signals
        ReplConcat,  // Replicative Concatenation {<N>, <Item>}
                      // Duplicates And Concatenates The Item N Times
        PartSelectPost, // Logic [31: 0] A; Logic [0 :31] B;  A[ 0 +: 8] == A[ 7 : 0]; B[ 0 +: 8] == B[0 : 7]
        PartSelectPre, // A[15 -: 8] == A[15 : 8]; B[15 -: 8] == B[8 :15]
        Dot,  // Accessing Of Property
        DoubleColon,  // ::, System Verilog Accessing Class/Package Static Property/Type
        Apostrophe,  // VHDL Attribute Access
        Arrow,  // Pointer Member Access, VHDL Arrow Operator Used In Type Descriptions
        Reference,
        Dereference,
        // Assignment Operators
        Assign,  // =
        PlusAssign,  // +=
        MinusAssign,  // -=
        MulAssign,  // *=
        DivAssign,  // /=
        ModAssign,  // %=
        AndAssign,  // &=
        OrAssign,  // |=
        XorAssign,  // ^=
        ShiftLeftAssign,  // <<=
        ShiftRightAssign,  // >>=
        ArithShiftLeftAssign,  // <<<=
        ArithShiftRightAssign,  // >>>=

        Ternary, // Cond ? A:B, A If Cond Else B
        Call,  // Call Of HDL Function
        Rising,  // Rising Edge/Posedge Event Operator
        Falling,  // Falling Edge/Negedge Event Operator
        Downto,  // Downto For The Slice Specification
        To,  // To For The Slice Specification
        Parametrization,  // Specification Of Template Arguments
        MapAssociation, // Arg=Val
        Range,  // Range Used In VHDL Type Specifications
        Throughout,  // System Verilog Throughout Operator
        DefineResolver,  // Used In Resolver Specification In VHDL Subtype Definition
        TypeOf,  // System Verilog Type Operator
        UnitSpec, // VHDL Unit Specification Eg. 10 Ns
    }
}
