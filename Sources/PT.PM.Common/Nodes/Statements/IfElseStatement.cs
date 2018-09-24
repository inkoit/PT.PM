﻿using System.Collections.Generic;
using PT.PM.Common.Nodes.Expressions;
using System;

namespace PT.PM.Common.Nodes.Statements
{
    public class IfElseStatement : Statement
    {
        public Expression Condition { get; set; }

        public Statement TrueStatement { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public Statement FalseStatement { get; set; }

        public IfElseStatement(Expression condition, Statement trueStatement, TextSpan textSpan)
            : base(textSpan)
        {
            Condition = condition;
            TrueStatement = trueStatement;
        }

        public IfElseStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>()
            {
                Condition,
                TrueStatement
            };
            if (FalseStatement != null)
            {
                result.Add(FalseStatement);
            }

            return result.ToArray();
        }

        public override string ToString()
        {
            var result = $"if ({Condition})\n{{\n{TrueStatement}\n}}";
            if (FalseStatement != null)
            {
                result += $"else\n{{\n{FalseStatement}\n}}";
            }
            return result;
        }
    }
}
