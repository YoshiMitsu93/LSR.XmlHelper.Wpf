using LSR.XmlHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace LSR.XmlHelper.Core.Services
{
    public static class XmlParseDiagnosticsService
    {
        private static readonly Regex MismatchStartTagRegex = new Regex(
            "start tag on line\\s+(\\d+)\\s+position\\s+(\\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex MismatchTagNamesRegex = new Regex(
            "The '([^']+)' start tag.*end tag of '([^']+)'",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static XmlParseProblem? TryGetParseProblem(string xml)
        {
            var problems = TryGetParseProblems(xml);
            if (problems.Count == 0)
                return null;

            return problems[0];
        }

        public static IReadOnlyList<XmlParseProblem> TryGetParseProblems(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                return Array.Empty<XmlParseProblem>();

            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit
                };

                using var sr = new StringReader(xml);
                using var reader = XmlReader.Create(sr, settings);
                while (reader.Read())
                {
                }

                return FindLintProblems(xml);
            }
            catch (XmlException ex)
            {
                var msg = ex.Message ?? "XML parse error.";

                var offset = LineColumnToOffset(xml, ex.LineNumber, ex.LinePosition);

                var mismatchInfo = TryGetMismatchStartTagInfo(msg);
                if (mismatchInfo is not null)
                {
                    offset = LineColumnToOffset(xml, mismatchInfo.LineNumber, mismatchInfo.ColumnNumber);
                    offset = TryShiftToSecondStartTagOnSameLine(xml, offset, mismatchInfo.StartTagName);
                }

                offset = AdjustOffset(xml, offset, msg);

                var lc = OffsetToLineColumn(xml, offset);
                return new List<XmlParseProblem> { new XmlParseProblem(msg, offset, lc.LineNumber, lc.ColumnNumber, XmlProblemSeverity.Error) };
            }
            catch (Exception ex)
            {
                var msg = ex.Message ?? "XML error.";
                return new List<XmlParseProblem> { new XmlParseProblem(msg, 0, 1, 1, XmlProblemSeverity.Error) };
            }
        }

        private sealed class ElementContext
        {
            public bool HasElementChild { get; set; }
        }

        private static List<XmlParseProblem> FindLintProblems(string xml)
        {
            var problems = new List<XmlParseProblem>();

            problems.AddRange(FindUnexpectedTextBetweenElements(xml));

            return problems;
        }

        private static List<XmlParseProblem> FindUnexpectedTextBetweenElements(string xml)
        {
            var problems = new List<XmlParseProblem>();

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit
            };

            using var sr = new StringReader(xml);
            using var reader = XmlReader.Create(sr, settings);

            var stack = new Stack<ElementContext>();

            while (true)
            {
                bool ok;
                try
                {
                    ok = reader.Read();
                }
                catch
                {
                    return problems;
                }

                if (!ok)
                    break;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (stack.Count > 0)
                        stack.Peek().HasElementChild = true;

                    if (!reader.IsEmptyElement)
                        stack.Push(new ElementContext());

                    continue;
                }

                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (stack.Count > 0)
                        stack.Pop();

                    continue;
                }

                if (reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.CDATA)
                    continue;

                if (string.IsNullOrWhiteSpace(reader.Value))
                    continue;

                if (stack.Count == 0)
                    continue;

                if (stack.Peek().HasElementChild == false)
                    continue;

                if (reader is not IXmlLineInfo li || !li.HasLineInfo())
                    continue;

                var o = LineColumnToOffset(xml, li.LineNumber, li.LinePosition);
                var lc = OffsetToLineColumn(xml, o);
                var msg = "Unexpected text between elements. Only whitespace is expected here.";
                problems.Add(new XmlParseProblem(msg, o, lc.LineNumber, lc.ColumnNumber, XmlProblemSeverity.Warning));

                if (problems.Count >= 50)
                    return problems;
            }

            return problems;
        }

        private sealed class MismatchStartTagInfo
        {
            public MismatchStartTagInfo(int lineNumber, int columnNumber, string startTagName, string endTagName)
            {
                LineNumber = lineNumber;
                ColumnNumber = columnNumber;
                StartTagName = startTagName ?? "";
                EndTagName = endTagName ?? "";
            }

            public int LineNumber { get; }
            public int ColumnNumber { get; }
            public string StartTagName { get; }
            public string EndTagName { get; }
        }

        private static MismatchStartTagInfo? TryGetMismatchStartTagInfo(string message)
        {
            var loc = MismatchStartTagRegex.Match(message);
            if (!loc.Success)
                return null;

            if (!int.TryParse(loc.Groups[1].Value, out var line))
                return null;

            if (!int.TryParse(loc.Groups[2].Value, out var pos))
                return null;

            var names = MismatchTagNamesRegex.Match(message);
            var startTag = names.Success ? names.Groups[1].Value : "";
            var endTag = names.Success ? names.Groups[2].Value : "";
            if (string.IsNullOrWhiteSpace(startTag))
                return null;

            return new MismatchStartTagInfo(line, pos, startTag, endTag);
        }

        private static int TryShiftToSecondStartTagOnSameLine(string text, int offset, string tagName)
        {
            if (offset < 0)
                return 0;

            if (offset >= text.Length)
                return Math.Max(0, text.Length - 1);

            if (string.IsNullOrWhiteSpace(tagName))
                return offset;

            var lineStart = offset;
            while (lineStart > 0 && text[lineStart - 1] != '\n' && text[lineStart - 1] != '\r')
                lineStart--;

            var lineEnd = offset;
            while (lineEnd < text.Length && text[lineEnd] != '\n' && text[lineEnd] != '\r')
                lineEnd++;

            var lineText = text.Substring(lineStart, lineEnd - lineStart);
            var token = "<" + tagName;

            var first = lineText.IndexOf(token, StringComparison.Ordinal);
            if (first < 0)
                return offset;

            var second = lineText.IndexOf(token, first + token.Length, StringComparison.Ordinal);
            if (second < 0)
                return offset;

            return lineStart + second;
        }

        private static int LineColumnToOffset(string text, int lineNumber, int columnNumber)
        {
            if (lineNumber <= 1)
                return Math.Max(0, columnNumber - 1);

            var line = 1;
            var i = 0;

            while (i < text.Length && line < lineNumber)
            {
                if (text[i] == '\n')
                    line++;

                i++;
            }

            var col = Math.Max(1, columnNumber);
            var offset = i + (col - 1);

            if (offset < 0)
                return 0;

            if (offset >= text.Length)
                return Math.Max(0, text.Length - 1);

            return offset;
        }

        private static (int LineNumber, int ColumnNumber) OffsetToLineColumn(string text, int offset)
        {
            if (offset < 0)
                return (1, 1);

            if (offset >= text.Length)
                offset = Math.Max(0, text.Length - 1);

            var line = 1;
            var lastLineStart = 0;

            for (var i = 0; i < offset; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    lastLineStart = i + 1;
                }
            }

            var column = (offset - lastLineStart) + 1;
            if (column < 1)
                column = 1;

            return (line, column);
        }

        private static int AdjustOffset(string text, int offset, string message)
        {
            if (offset < 0)
                return 0;

            if (offset >= text.Length)
                return Math.Max(0, text.Length - 1);

            if (!message.Contains("expected token is '>'", StringComparison.OrdinalIgnoreCase))
                return offset;

            if (text[offset] != '<')
                return offset;

            var i = offset - 1;
            while (i > 0 && (text[i] == '\r' || text[i] == '\n' || text[i] == ' ' || text[i] == '\t'))
                i--;

            return Math.Max(0, i);
        }
    }
}
