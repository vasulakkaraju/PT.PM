﻿using PT.PM.AntlrUtils;
using PT.PM.Common;
using Antlr4.Runtime;
using System;
using PT.PM.Common.Exceptions;

namespace PT.PM.Dsl
{
    public class DslAntlrParser : ILoggable
    {
        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public DslAntlrParser()
        {
        }

        public DslParser.PatternContext Parse(string data)
        {
            DslParser.PatternContext pattern = null;
            var codeFile = new CodeFile(data) { Name = "Pattern", IsPattern = true };
            var errorListener = new AntlrMemoryErrorListener()
            {
                IsPattern = true,
                CodeFile = codeFile,
                Logger = Logger
            };
            try
            {
                var inputStream = new AntlrInputStream(data);
                DslLexer lexer = new DslLexer(inputStream);

                lexer.RemoveErrorListeners();
                lexer.AddErrorListener(errorListener);

                var tokenStream = new CommonTokenStream(lexer);
                DslParser parser = new DslParser(tokenStream);
                parser.RemoveErrorListeners();
                parser.AddErrorListener(errorListener);
                pattern = parser.pattern();
            }
            catch (Exception ex)
            {
                Logger.LogError(new ParsingException(codeFile, ex));
                throw;
            }

            return pattern;
        }
    }
}
