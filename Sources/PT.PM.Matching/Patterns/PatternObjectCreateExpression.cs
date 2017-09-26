﻿using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using System;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternObjectCreateExpression : PatternBase
    {
        public PatternBase Type { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternObjectCreateExpression()
        {
        }

        public PatternObjectCreateExpression(PatternBase type, PatternArgs args, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Type = type;
            Arguments = args;
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.Add(Type);
            result.Add(Arguments);
            return result.ToArray();
        }

        public override string ToString() => $"new {Type}({Arguments})";

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust?.Kind != UstKind.ObjectCreateExpression)
            {
                return false;
            }

            var objectCreateExpression = (ObjectCreateExpression)ust;
            return Type.Match(objectCreateExpression.Type, context) &&
                   Arguments.Match(objectCreateExpression.Arguments, context);
        }
    }
}
