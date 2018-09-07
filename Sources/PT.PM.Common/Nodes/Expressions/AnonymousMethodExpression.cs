﻿using PT.PM.Common.Nodes.Statements;
using PT.PM.Common.Nodes.TypeMembers;
using System.Collections.Generic;
using System.Linq;

namespace PT.PM.Common.Nodes.Expressions
{
    public class AnonymousMethodExpression : Expression
    {
        public List<ParameterDeclaration> Parameters { get; set; } = new List<ParameterDeclaration>();

        public BlockStatement Body { get; set; }

        public string Id => Parent is AssignmentExpression assignment 
            ? assignment.Left.ToString()
            : LineColumnTextSpan.ToString();

        public string Signature => UstUtils.GenerateSignature(Id, Parameters);

        public override Expression[] GetArgs() => new Expression[0];

        public AnonymousMethodExpression()
        {
        }

        public AnonymousMethodExpression(IEnumerable<ParameterDeclaration> parameters, BlockStatement body,
            TextSpan textSpan)
            : base(textSpan)
        {
            Parameters = parameters as List<ParameterDeclaration> ?? parameters.ToList();
            Body = body;
        }

        public override Ust[] GetChildren()
        {
            var result = new List<Ust>();
            result.AddRange(Parameters);
            result.Add(Body);
            return result.ToArray();
        }
    }
}
