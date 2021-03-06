﻿using PT.PM.Common;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;

namespace PT.PM.Matching.Patterns
{
    public class PatternVar : PatternUst<IdToken>
    {
        public string Id { get; set; } = "";

        public PatternVar()
            : this("")
        {
        }

        public PatternVar(string id, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Id = id;
        }

        public PatternUst Value { get; set; } = new PatternIdRegexToken();

        public override string ToString()
        {
            string valueString = "";
            if (Parent is PatternAssignmentExpression parentAssignment &&
                ReferenceEquals(this, parentAssignment.Left))
            {
                if (!(Value is PatternIdRegexToken patternIdRegexToken && patternIdRegexToken.Any))
                {
                    valueString = ": " + Value.ToString();
                }
            }

            return Id + valueString;
        }

        public override MatchContext Match(IdToken idToken, MatchContext context)
        {
            MatchContext newContext;

            newContext = context;
            if (idToken.Parent is AssignmentExpression parentAssignment &&
                ReferenceEquals(idToken, parentAssignment.Left))
            {
                if (Value != null)
                {
                    newContext = Value.MatchUst(idToken, newContext);
                    if (newContext.Success)
                    {
                        newContext.Vars[Id] = idToken;
                    }
                }
                else
                {
                    newContext.Vars[Id] = idToken;
                    newContext = newContext.AddMatch(idToken);
                }
            }
            else
            {
                newContext = newContext.Vars.ContainsKey(Id)
                    ? newContext.AddMatch(idToken)
                    : newContext.Fail();
            }

            return newContext;
        }
    }
}
