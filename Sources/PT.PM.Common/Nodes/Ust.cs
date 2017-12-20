﻿using System;
using System.Diagnostics;
using System.Text;

namespace PT.PM.Common.Nodes
{
    [DebuggerDisplay("{ToStringWithoutLineBreaks()}")]
    public abstract class Ust : IComparable<Ust>, IEquatable<Ust>, IUst<Ust, RootUst>, IUst
    {
        private PrettyPrinter debuggerPrinter = new PrettyPrinter
        {
            MaxMessageLength = 0,
            ReduceWhitespaces = true
        };

        public int KindId => GetType().Name.GetHashCode();

        public RootUst Root { get; set; }

        public Ust Parent { get; set; }

        public virtual bool IsTerminal => false;

        public LineColumnTextSpan LineColumnTextSpan => Root?.SourceCodeFile?.GetLineColumnTextSpan(TextSpan);

        public TextSpan TextSpan { get; set; }

        public Ust[] Children => GetChildren();

        public string ToStringWithoutLineBreaks() => debuggerPrinter.Print(ToString());

        protected Ust()
        {
        }

        protected Ust(TextSpan textSpan)
            : this()
        {
            TextSpan = textSpan;
        }

        public abstract Ust[] GetChildren();

        public bool Equals(Ust other)
        {
            return CompareTo(other) == 0;
        }

        public virtual int CompareTo(Ust other)
        {
            if (other == null)
            {
                return KindId;
            }

            int nodeTypeCompareResult = KindId - other.KindId;
            if (nodeTypeCompareResult != 0)
            {
                return nodeTypeCompareResult;
            }

            return Children.CompareTo(other.Children);
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var child in Children)
            {
                result.Append(child);
                result.Append(" ");
            }
            return result.ToString();
        }
    }
}