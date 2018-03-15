﻿using System;
using System.Collections.Generic;
using System.IO;

namespace PT.PM.Common
{
    public class CodeFile : IEquatable<CodeFile>, IComparable<CodeFile>, IComparable
    {
        private readonly object lockObj = new object();
        private int[] lineIndexes;

        public const int StartLine = 1;

        public const int StartColumn = 1;

        public static CodeFile Empty = new CodeFile("");

        public string RootPath { get; set; } = "";

        public string RelativePath { get; set; } = "";

        public string Name { get; set; } = "";

        public bool IsPattern { get; set; } = false;

        public string Code { get; } = "";

        public string RelativeName => Path.Combine(RelativePath, Name);

        public string FullName => Path.Combine(RootPath, RelativePath, Name);

        public CodeFile(string code)
        {
            Code = code ?? "";
        }

        public override string ToString() => RelativeName;

        public LineColumnTextSpan GetLineColumnTextSpan(TextSpan textSpan)
        {
            GetLineColumnFromLinear(textSpan.Start, out int beginLine, out int beginColumn);
            GetLineColumnFromLinear(textSpan.End, out int endLine, out int endColumn);

            return new LineColumnTextSpan(beginLine, beginColumn, endLine, endColumn, textSpan.FileName);
        }

        public void GetLineColumnFromLinear(int position, out int line, out int column)
        {
            InitLineIndexesIfRequired();

            line = Array.BinarySearch(lineIndexes, position);
            if (line < 0)
            {
                line = (line == -1) ? 0 : (~line - 1);
            }

            column = position - lineIndexes[line] + StartColumn;
            line += StartLine;
        }

        public TextSpan GetTextSpan(LineColumnTextSpan textSpan)
        {
            int start = GetLinearFromLineColumn(textSpan.BeginLine, textSpan.BeginColumn);
            int end = GetLinearFromLineColumn(textSpan.EndLine, textSpan.EndColumn);

            var result = TextSpan.FromBounds(start, end);
            result.FileName = textSpan.FileName;
            return result;
        }

        public int GetLinearFromLineColumn(int line, int column)
        {
            InitLineIndexesIfRequired();

            return lineIndexes[line - StartLine] + column - StartColumn;
        }

        public int GetLineLinearIndex(int lineIndex)
        {
            InitLineIndexesIfRequired();

            return lineIndexes[lineIndex];
        }

        public int GetLinesCount()
        {
            InitLineIndexesIfRequired();

            return lineIndexes.Length;
        }

        private void InitLineIndexesIfRequired()
        {
            if (lineIndexes == null)
            {
                lock (lockObj)
                {
                    if (lineIndexes == null)
                    {
                        InitLineIndexes();
                    }
                }
            }
        }

        private void InitLineIndexes()
        {
            int currentLine = StartLine;
            int currentColumn = StartColumn;
            string text = Code;

            var lineIndexesBuffer = new List<int>(text.Length / 25) { 0 };
            int textIndex = 0;
            while (textIndex < text.Length)
            {
                char c = text[textIndex];
                if (c == '\r' || c == '\n')
                {
                    currentLine++;
                    currentColumn = StartColumn;
                    if (c == '\r' && textIndex + 1 < text.Length && text[textIndex + 1] == '\n')
                    {
                        textIndex++;
                    }
                    lineIndexesBuffer.Add(textIndex + 1);
                }
                else
                {
                    currentColumn++;
                }
                textIndex++;
            }

            lineIndexes = lineIndexesBuffer.ToArray();
        }

        public static bool operator ==(CodeFile codeFile1, CodeFile codeFile2)
        {
            if (codeFile1 is null)
            {
                return codeFile2 is null;
            }

            return codeFile1.Equals(codeFile2);
        }

        public static bool operator !=(CodeFile codeFile1, CodeFile codeFile2)
        {
            if (codeFile1 is null)
            {
                return !(codeFile2 is null);
            }

            return !codeFile1.Equals(codeFile2);
        }

        public override bool Equals(object obj) => Equals(obj as CodeFile);

        public bool Equals(CodeFile other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(FullName) && string.IsNullOrEmpty(other.FullName))
            {
                return Code == other.Code;
            }

            return FullName == other.FullName;
        }

        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(FullName)
                ? Code.GetHashCode()
                : FullName.GetHashCode();
        }

        public int CompareTo(object obj) => CompareTo(obj as CodeFile);

        public int CompareTo(CodeFile other)
        {
            if (other is null)
            {
                return 1;
            }

            if (string.IsNullOrEmpty(FullName) && string.IsNullOrEmpty(other.FullName))
            {
                return Code.CompareTo(other.Code);
            }

            return FullName.CompareTo(other.FullName);
        }
    }
}
