using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Files;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Common.MessagePack
{
    public class RootUstFormatter : IMessagePackFormatter<RootUst>
    {
        public RootUst CurrentRoot { get; internal set; }

        public BinaryFile SerializedFile { get; private set; }

        public HashSet<IFile> SourceFiles { get; private set; }

        public static RootUstFormatter CreateWriter()
        {
            return new RootUstFormatter();
        }

        public static RootUstFormatter CreateReader(BinaryFile serializedFile, HashSet<IFile> sourceFiles)
        {
            return new RootUstFormatter
            {
                SerializedFile = serializedFile ?? throw new ArgumentNullException(nameof(serializedFile)),
                SourceFiles = sourceFiles ?? throw new ArgumentNullException(nameof(sourceFiles))
            };
        }

        private RootUstFormatter()
        {
        }

        public int Serialize(ref byte[] bytes, int offset, RootUst value, IFormatterResolver formatterResolver)
        {
            int newOffset = offset;
            CurrentRoot = value;

            var languageFormatter = formatterResolver.GetFormatter<Language>();
            newOffset += languageFormatter.Serialize(ref bytes, newOffset, value.Language, formatterResolver);

            var localSourceFiles = new List<TextFile> {CurrentRoot.SourceFile};
            value.ApplyActionToDescendantsAndSelf(ust =>
            {
                foreach (TextSpan textSpan in ust.TextSpans)
                {
                    if (textSpan.File != null && !localSourceFiles.Contains(textSpan.File))
                    {
                        localSourceFiles.Add(textSpan.File);
                    }
                }
            });

            var textSpanFormatter = formatterResolver.GetFormatter<TextSpan>();
            if (textSpanFormatter is TextSpanFormatter formatter)
            {
                formatter.LocalSourceFiles = localSourceFiles.ToArray();
            }

            var filesFormatter = formatterResolver.GetFormatter<TextFile[]>();
            newOffset += filesFormatter.Serialize(ref bytes, newOffset, localSourceFiles.ToArray(), formatterResolver);

            var ustsFormatter = formatterResolver.GetFormatter<Ust[]>();
            newOffset += ustsFormatter.Serialize(ref bytes, newOffset, value.Nodes, formatterResolver);

            var commentsFormatter = formatterResolver.GetFormatter<CommentLiteral[]>();
            newOffset += commentsFormatter.Serialize(ref bytes, newOffset, value.Comments, formatterResolver);

            newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.LineOffset);

            newOffset += textSpanFormatter.Serialize(ref bytes, newOffset, value.TextSpan, formatterResolver);

            newOffset += MessagePackBinary.WriteInt32(ref bytes, newOffset, value.Key);

            return newOffset - offset;
        }

        public RootUst Deserialize(byte[] bytes, int offset, IFormatterResolver formatterResolver, out int readSize)
        {
            int newOffset = offset;
            try
            {
                var languageFormatter = formatterResolver.GetFormatter<Language>();
                Language language = languageFormatter.Deserialize(bytes, newOffset, formatterResolver, out int size);
                newOffset += size;

                var filesFormatter = formatterResolver.GetFormatter<TextFile[]>();
                TextFile[] localSourceFiles = filesFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                lock (SourceFiles)
                {
                    foreach (TextFile localSourceFile in localSourceFiles)
                    {
                        SourceFiles.Add(localSourceFile);
                    }
                }

                var textSpanFormatter = formatterResolver.GetFormatter<TextSpan>();
                if (textSpanFormatter is TextSpanFormatter formatter)
                {
                    formatter.LocalSourceFiles = localSourceFiles;
                }

                var ustsFormatter = formatterResolver.GetFormatter<Ust[]>();
                Ust[] nodes = ustsFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                var commentsFormatter = formatterResolver.GetFormatter<CommentLiteral[]>();
                CommentLiteral[] comments =
                    commentsFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                int lineOffset = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                newOffset += size;

                TextSpan textSpan =
                    textSpanFormatter.Deserialize(bytes, newOffset, formatterResolver, out size);
                newOffset += size;

                int key = MessagePackBinary.ReadInt32(bytes, newOffset, out size);
                newOffset += size;

                CurrentRoot = new RootUst(localSourceFiles[0], language, textSpan)
                {
                    Key = key,
                    Nodes = nodes,
                    Comments = comments,
                    LineOffset = lineOffset
                };

                readSize = newOffset - offset;

                return CurrentRoot;
            }
            catch (InvalidOperationException ex) // Catch incorrect format exceptions
            {
                throw new ReadException(SerializedFile, ex, $"Error during reading {nameof(RootUst)} at {newOffset} offset; Message: {ex.Message}");
            }
        }
    }
}