﻿using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternStringRegexLiteral : PatternUst<StringLiteral>
    {
        public Regex StringRegex { get; set; }

        public PatternStringRegexLiteral()
            : this("")
        {
        }

        public PatternStringRegexLiteral(string regexString, TextSpan textSpan = default(TextSpan))
            : this(new Regex(string.IsNullOrEmpty(regexString) ? ".*" : regexString, RegexOptions.Compiled), textSpan)
        {
        }

        public PatternStringRegexLiteral(Regex regex, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            StringRegex = regex;
        }

        public override string ToString() => $@"<""{StringRegex}"">";

        public override MatchContext Match(StringLiteral stringLiteral, MatchContext context)
        {
            IEnumerable<TextSpan> matches = StringRegex
                .MatchRegex(stringLiteral.Text, stringLiteral.EscapeCharsLength)
                .Select(location => location.AddOffset(stringLiteral.TextSpan.Start));

            return matches.Count() > 0
                ? context.AddMatches(matches)
                : context.Fail();
        }
    }
}