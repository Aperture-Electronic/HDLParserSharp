using Antlr4.Runtime;
using HDLAbstractSyntaxTree.HDLElement;
using System;
using System.Collections.Generic;

namespace HDLParserBase
{
    public abstract class HDLParser
    {
        private HDLCommentParser? commentParser;

        public HDLCommentParser CommentParser
        {
            get
            {
                if (commentParser == null)
                {
                    throw new NullReferenceException("Comment parser is null");
                }

                return commentParser;
            }

            private set => commentParser = value;
        }

        public bool HierarchyOnly { get; } = false;

        public ITokenStream? Tokens { get; }

        public List<HDLObject>? HDLContext { get; }

        public HDLParser(ITokenStream stream, List<HDLObject> context, bool hierarchyOnly)
        {
            Tokens = stream;
            HDLContext = context;
            HierarchyOnly = hierarchyOnly;
            commentParser = null;
        }

        public HDLParser(HDLCommentParser commentParser, bool hierarchyOnly)
        {
            Tokens = null;
            HDLContext = null;
            HierarchyOnly = hierarchyOnly;
            CommentParser = commentParser;
        }

        public HDLParser(HDLParser other)
        {
            Tokens = other.Tokens;
            HDLContext = other.HDLContext;
            CommentParser = other.CommentParser;
            HierarchyOnly = other.HierarchyOnly;
        }
    }
}
