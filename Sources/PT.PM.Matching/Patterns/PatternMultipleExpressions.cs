﻿using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternMultipleExpressions : PatternBase
    {
        public PatternMultipleExpressions()
        {
        }

        public override Ust[] GetChildren() => ArrayUtils<Ust>.EmptyArray;

        public override string ToString() => "#*";

        public override bool Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return false;
            }

            return ust is Expression;
        }
    }
}
