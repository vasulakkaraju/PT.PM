﻿using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternInvocationExpression : PatternUst<InvocationExpression>, IPatternExpression
    {
        public PatternUst Target { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternInvocationExpression()
        {
        }

        public PatternInvocationExpression(PatternUst target, PatternArgs arguments,
            TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Target = target;
            Arguments = arguments;
        }

        public PatternUst[] GetArgs()
        {
            var result = new List<PatternUst>();
            result.Add(Target);
            result.AddRange(Arguments.Args);
            return result.ToArray();
        }

        public override string ToString() => $"{Target}({Arguments})";

        public override MatchContext Match(InvocationExpression invocation, MatchContext context)
        {
            MatchContext newContext;

            newContext = Target.MatchUst(invocation.Target, context);
            if (!newContext.Success)
            {
                return newContext;
            }
            newContext = Arguments.Match(invocation.Arguments, newContext);

            return newContext.AddUstIfSuccess(invocation);
        }
    }
}
