﻿using System.Text;
using MessagePack;

namespace PT.PM.Common.Nodes.Expressions
{
    [MessagePackObject]
    public class TupleCreateExpression : ObjectCreateExpression
    {
        public override Ust[] GetChildren()
        {
            if (Initializers != null)
            {
                return Initializers.ToArray();
            }
            return new Ust[0];
        }

        public override Expression[] GetArgs()
        {
            return new Expression[0];
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            foreach(var initializer in Initializers)
            {
                sb.Append(initializer.ToString());
                sb.Append(", ");
            }
            sb.Append(")");
            return sb.ToString();
        }
    }
}
