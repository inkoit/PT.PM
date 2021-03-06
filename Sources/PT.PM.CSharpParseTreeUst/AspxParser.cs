﻿using AspxParser;
using PT.PM.Common;
using PT.PM.Common.Exceptions;
using System;
using System.Diagnostics;
using System.Threading;
using PT.PM.Common.Files;

namespace PT.PM.CSharpParseTreeUst
{
    public class AspxParser : ILanguageParser<TextFile>
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public Language Language => Language.Aspx;

        public static AspxParser Create() => new AspxParser();

        public ParseTree Parse(TextFile sourceFile, out TimeSpan parserTimeSpan)
        {
            if (sourceFile.Data == null)
            {
                return null;
            }

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var parser = new global::AspxParser.AspxParser(sourceFile.RelativePath);
                var source = new AspxSource(sourceFile.FullName, sourceFile.Data);
                AspxParseResult aspxTree = parser.Parse(source);
                foreach (var error in aspxTree.ParseErrors)
                {
                    Logger.LogError(new ParsingException(sourceFile, message: error.Message)
                    {
                        TextSpan = error.Location.GetTextSpan()
                    });
                }
                var result = new AspxParseTree(aspxTree.RootNode);
                result.SourceFile = sourceFile;
                stopwatch.Stop();

                parserTimeSpan = stopwatch.Elapsed;

                return result;
            }
            catch (Exception ex) when (!(ex is ThreadAbortException))
            {
                Logger.LogError(new ParsingException(sourceFile, ex));
                return null;
            }
        }
    }
}
