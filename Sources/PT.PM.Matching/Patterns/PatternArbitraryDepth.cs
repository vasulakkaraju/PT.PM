﻿using PT.PM.Common;
using PT.PM.Common.Nodes;

namespace PT.PM.Matching.Patterns
{
    public class PatternArbitraryDepth : PatternUst
    {
        public PatternUst Pattern { get; set; }

        public PatternArbitraryDepth()
        {
        }

        public PatternArbitraryDepth(PatternUst pattern, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Pattern = pattern;
        }

        public override string ToString()
        {
            if (Pattern == null)
            {
                return "#*";
            }

            return "<{ " + Pattern.ToString() + " }>";
        }

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (ust == null)
            {
                return context.Fail();
            }

            var result = ust.AnyDescendant(ustNode => MatchExpression(ustNode, context).Success);
            return context.Set(result).AddUstIfSuccess(ust);
        }

        protected MatchingContext MatchExpression(Ust other, MatchingContext context)
        {
            if (Pattern == null)
            {
                if (other == null)
                {
                    return context;
                }
                return context.Fail();
            }
            return Pattern.Match(other, context);
        }
    }
}