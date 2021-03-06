﻿using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using PT.PM.Common;
using System.Collections.Generic;
using System.Text;

namespace PT.PM.AntlrUtils
{
    public class AntlrDumper : ParseTreeDumper
    {
        public override void DumpTokens(ParseTree parseTree)
        {
            var antlrParseTree = parseTree as AntlrParseTree;

            IVocabulary vocabulary = ((AntlrParser)parseTree.SourceLanguage.CreateParser()).Lexer.Vocabulary;
            var resultString = new StringBuilder();
            foreach (IToken token in antlrParseTree.Tokens)
            {
                if (!OnlyCommonTokens || token.Channel == 0)
                {
                    resultString.Append(RenderToken(token, false, vocabulary));
                    if (EachTokenOnNewLine)
                        resultString.AppendLine();
                    else
                        resultString.Append(" ");
                }
            }
            resultString.Append("EOF");

            Dump(resultString.ToString(), parseTree.SourceCodeFile, true);
        }

        public override void DumpTree(ParseTree parseTree)
        {
            var result = new StringBuilder();
            Parser parser = ((AntlrParser)parseTree.SourceLanguage.CreateParser()).Parser;
            DumpTree(((AntlrParseTree)parseTree).SyntaxTree, parser, result, 0);

            Dump(result.ToString(), parseTree.SourceCodeFile, false);
        }

        private void DumpTree(IParseTree parseTree, Parser parser, StringBuilder builder, int level)
        {
            int currentLevelStringLength = level * IndentSize;
            builder.PadLeft(currentLevelStringLength);
            if (parseTree is RuleContext ruleContext)
            {
                builder.Append(parser.RuleNames[ruleContext.RuleIndex]);
                builder.AppendLine(" (");

                for (int i = 0; i < ruleContext.ChildCount; i++)
                {
                    DumpTree(ruleContext.GetChild(i), parser, builder, level + 1);
                    builder.AppendLine();
                }

                builder.PadLeft(currentLevelStringLength);
                builder.Append(")");
            }
            else
            {
                builder.Append('\'' + parseTree.GetText().Replace(@"\", @"\\").Replace(@"'", @"\'") + '\''); // TODO: replace with RenderToken.
            }
        }

        private string RenderToken(IToken token, bool showChannel, IVocabulary vocabulary)
        {
            string symbolicName = vocabulary?.GetSymbolicName(token.Type) ?? token.Type.ToString();
            string value = TokenValueDisplayMode == TokenValueDisplayMode.Trim ? token.Text?.Trim() : token.Text;

            string tokenValue = string.Empty;
            if (TokenValueDisplayMode != TokenValueDisplayMode.Ignore && value != null && symbolicName != value)
            {
                tokenValue = value.Length <= MaxTokenValueLength
                    ? value
                    : value.Substring(0, MaxTokenValueLength) + "...";
            }

            string channelValue = string.Empty;
            if (showChannel)
            {
                channelValue = "c: " + token.Channel.ToString(); // TODO: channel name instead of identifier.
            }

            string result = symbolicName;
            if (!string.IsNullOrEmpty(tokenValue) || !string.IsNullOrEmpty(channelValue))
            {
                var strings = new List<string>();
                if (!string.IsNullOrEmpty(tokenValue))
                    strings.Add(tokenValue);
                if (!string.IsNullOrEmpty(channelValue))
                    strings.Add(channelValue);
                result = $"{result} ({(string.Join(", ", strings))})";
            }

            return result;
        }
    }
}
