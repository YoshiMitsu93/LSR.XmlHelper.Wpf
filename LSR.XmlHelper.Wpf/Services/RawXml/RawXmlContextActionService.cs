using LSR.XmlHelper.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace LSR.XmlHelper.Wpf.Services.RawXml
{
    public sealed class RawXmlContextActionService
    {
        private readonly XmlDocumentService _xml;

        public RawXmlContextActionService()
        {
            _xml = new XmlDocumentService();
        }
        public bool TryDuplicateSpan(string xmlText, int startOffset, int endOffset, out string updatedText, out int newCaretOffset)
        {
            updatedText = xmlText;
            newCaretOffset = startOffset;

            if (string.IsNullOrEmpty(xmlText))
                return false;

            if (startOffset < 0 || endOffset > xmlText.Length || endOffset <= startOffset)
                return false;

            var spanText = xmlText.Substring(startOffset, endOffset - startOffset);

            var newline = DetectNewLine(xmlText);
            var indent = ReadLineIndent(xmlText, startOffset);

            var insertText = newline + indent + spanText;

            updatedText = xmlText.Substring(0, endOffset) + insertText + xmlText.Substring(endOffset);
            newCaretOffset = Math.Max(0, Math.Min(updatedText.Length, endOffset + insertText.Length));
            return true;
        }
        public bool TryDeleteSpan(string xmlText, int startOffset, int endOffset, out string updatedText, out int newCaretOffset)
        {
            updatedText = xmlText;
            newCaretOffset = startOffset;

            if (string.IsNullOrEmpty(xmlText))
                return false;

            if (startOffset < 0 || endOffset > xmlText.Length || endOffset <= startOffset)
                return false;

            var deleteStart = startOffset;
            var lineStart = LastNewlineIndex(xmlText, startOffset) + 1;

            if (lineStart >= 0 && lineStart <= startOffset)
            {
                var allWhitespace = true;

                for (var i = lineStart; i < startOffset; i++)
                {
                    if (!char.IsWhiteSpace(xmlText[i]) || xmlText[i] == '\r' || xmlText[i] == '\n')
                    {
                        allWhitespace = false;
                        break;
                    }
                }

                if (allWhitespace)
                    deleteStart = lineStart;
            }

            var deleteEnd = endOffset;

            if (deleteEnd < xmlText.Length)
            {
                if (xmlText[deleteEnd] == '\r')
                {
                    if (deleteEnd + 1 < xmlText.Length && xmlText[deleteEnd + 1] == '\n')
                        deleteEnd += 2;
                    else
                        deleteEnd += 1;
                }
                else if (xmlText[deleteEnd] == '\n')
                {
                    deleteEnd += 1;
                }
            }

            updatedText = xmlText.Substring(0, deleteStart) + xmlText.Substring(deleteEnd);
            newCaretOffset = Math.Max(0, Math.Min(updatedText.Length, deleteStart));
            return true;
        }

        private static int LastNewlineIndex(string text, int offset)
        {
            var idx = Math.Min(Math.Max(0, offset), text.Length);

            for (var i = idx - 1; i >= 0; i--)
            {
                var c = text[i];
                if (c == '\n')
                    return i;

                if (c == '\r')
                    return i;
            }

            return -1;
        }

        public bool TryFormatSpan(string xmlText, int startOffset, int endOffset, out string updatedText, out int newCaretOffset)
        {
            updatedText = xmlText;
            newCaretOffset = startOffset;

            if (string.IsNullOrEmpty(xmlText))
                return false;

            if (startOffset < 0 || endOffset > xmlText.Length || endOffset <= startOffset)
                return false;

            var spanText = xmlText.Substring(startOffset, endOffset - startOffset);

            string formatted;
            try
            {
                formatted = _xml.Format(spanText);
            }
            catch
            {
                return false;
            }

            var newline = DetectNewLine(xmlText);
            var indent = ReadLineIndent(xmlText, startOffset);

            formatted = formatted.TrimEnd('\r', '\n');

            var formattedIndented = ApplyIndent(formatted, indent, newline);

            updatedText = xmlText.Substring(0, startOffset) + formattedIndented + xmlText.Substring(endOffset);

            var delta = formattedIndented.Length - (endOffset - startOffset);
            newCaretOffset = Math.Max(0, Math.Min(updatedText.Length, startOffset + delta));
            return true;
        }

        public bool TryDuplicateContainingElement(string xmlText, int caretOffset, out string updatedText, out int newCaretOffset)
        {
            updatedText = xmlText;
            newCaretOffset = caretOffset;

            if (!TryGetContainingElementSpan(xmlText, caretOffset, out var start, out var end))
                return false;

            var elementText = xmlText.Substring(start, end - start);

            var newline = DetectNewLine(xmlText);
            var indent = ReadLineIndent(xmlText, start);

            var insertText = newline + indent + elementText;

            updatedText = xmlText.Substring(0, end) + insertText + xmlText.Substring(end);
            newCaretOffset = caretOffset;
            return true;
        }

        public bool TryFormatContainingElement(string xmlText, int caretOffset, out string updatedText, out int newCaretOffset)
        {
            updatedText = xmlText;
            newCaretOffset = caretOffset;

            if (!TryGetContainingElementSpan(xmlText, caretOffset, out var start, out var end))
                return false;

            var elementText = xmlText.Substring(start, end - start);

            string formatted;
            try
            {
                formatted = _xml.Format(elementText);
            }
            catch
            {
                return false;
            }

            var newline = DetectNewLine(xmlText);
            var indent = ReadLineIndent(xmlText, start);

            formatted = formatted.TrimEnd('\r', '\n');

            var formattedIndented = ApplyIndent(formatted, indent, newline);

            updatedText = xmlText.Substring(0, start) + formattedIndented + xmlText.Substring(end);

            var delta = formattedIndented.Length - (end - start);
            newCaretOffset = Math.Max(0, Math.Min(updatedText.Length, caretOffset + delta));
            return true;
        }

        public bool TryGetContainingElementSpan(string xmlText, int caretOffset, out int startOffset, out int endOffset)
        {
            startOffset = 0;
            endOffset = 0;

            if (string.IsNullOrEmpty(xmlText))
                return false;

            var offset = Math.Max(0, Math.Min(xmlText.Length - 1, caretOffset));

            for (var i = offset; i >= 0; i--)
            {
                if (xmlText[i] != '<')
                    continue;

                if (!TryReadStartTagName(xmlText, i, out var name, out var tagEnd, out var selfClosing))
                    continue;

                if (selfClosing)
                {
                    startOffset = i;
                    endOffset = tagEnd;
                    return caretOffset >= startOffset && caretOffset <= endOffset;
                }

                var stack = new Stack<string>();
                stack.Push(name);

                var j = tagEnd;

                while (j < xmlText.Length)
                {
                    if (xmlText[j] != '<')
                    {
                        j++;
                        continue;
                    }

                    if (StartsWith(xmlText, j, "<!--"))
                    {
                        j = IndexAfter(xmlText, j + 4, "-->");
                        if (j < 0)
                            return false;
                        continue;
                    }

                    if (StartsWith(xmlText, j, "<![CDATA["))
                    {
                        j = IndexAfter(xmlText, j + 9, "]]>");
                        if (j < 0)
                            return false;
                        continue;
                    }

                    if (StartsWith(xmlText, j, "<?"))
                    {
                        j = IndexAfter(xmlText, j + 2, "?>");
                        if (j < 0)
                            return false;
                        continue;
                    }

                    if (StartsWith(xmlText, j, "<!"))
                    {
                        var gt = FindTagClose(xmlText, j + 2);
                        if (gt < 0)
                            return false;
                        j = gt;
                        continue;
                    }

                    if (StartsWith(xmlText, j, "</"))
                    {
                        if (!TryReadEndTagName(xmlText, j, out var endName, out var endTagEnd))
                        {
                            j++;
                            continue;
                        }

                        if (stack.Count > 0 && string.Equals(stack.Peek(), endName, StringComparison.Ordinal))
                            stack.Pop();
                        else
                            TryPopUntil(stack, endName);

                        j = endTagEnd;

                        if (stack.Count == 0)
                        {
                            startOffset = i;
                            endOffset = endTagEnd;
                            return caretOffset >= startOffset && caretOffset <= endOffset;
                        }

                        continue;
                    }

                    if (TryReadStartTagName(xmlText, j, out var childName, out var childTagEnd, out var childSelfClosing))
                    {
                        if (!childSelfClosing)
                            stack.Push(childName);

                        j = childTagEnd;
                        continue;
                    }

                    j++;
                }
            }

            return false;
        }

        private static bool TryReadStartTagName(string text, int ltIndex, out string name, out int tagEndIndex, out bool selfClosing)
        {
            name = string.Empty;
            tagEndIndex = ltIndex;
            selfClosing = false;

            if (ltIndex < 0 || ltIndex >= text.Length)
                return false;

            var i = ltIndex + 1;
            if (i >= text.Length)
                return false;

            var first = text[i];
            if (first == '/' || first == '!' || first == '?')
                return false;

            while (i < text.Length && char.IsWhiteSpace(text[i]))
                i++;

            var startName = i;

            while (i < text.Length)
            {
                var c = text[i];
                if (char.IsWhiteSpace(c) || c == '>' || c == '/')
                    break;
                i++;
            }

            if (i <= startName)
                return false;

            name = text.Substring(startName, i - startName);

            var gt = FindTagClose(text, i);
            if (gt < 0)
                return false;

            tagEndIndex = gt;

            var k = gt - 2;
            while (k >= ltIndex && char.IsWhiteSpace(text[k]))
                k--;

            selfClosing = k >= ltIndex && text[k] == '/';

            return true;
        }

        private static bool TryReadEndTagName(string text, int ltIndex, out string name, out int tagEndIndex)
        {
            name = string.Empty;
            tagEndIndex = ltIndex;

            var i = ltIndex + 2;
            while (i < text.Length && char.IsWhiteSpace(text[i]))
                i++;

            var startName = i;

            while (i < text.Length)
            {
                var c = text[i];
                if (char.IsWhiteSpace(c) || c == '>')
                    break;
                i++;
            }

            if (i <= startName)
                return false;

            name = text.Substring(startName, i - startName);

            var gt = FindTagClose(text, i);
            if (gt < 0)
                return false;

            tagEndIndex = gt;
            return true;
        }

        private static int FindTagClose(string text, int startIndex)
        {
            var inQuote = false;
            var quoteChar = '\0';

            for (var i = startIndex; i < text.Length; i++)
            {
                var c = text[i];

                if (!inQuote)
                {
                    if (c == '"' || c == '\'')
                    {
                        inQuote = true;
                        quoteChar = c;
                        continue;
                    }

                    if (c == '>')
                        return i + 1;

                    continue;
                }

                if (c == quoteChar)
                {
                    inQuote = false;
                    quoteChar = '\0';
                }
            }

            return -1;
        }

        private static string DetectNewLine(string text)
        {
            return text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n" : "\n";
        }

        private static string ReadLineIndent(string text, int offset)
        {
            var lineStart = text.LastIndexOf('\n', Math.Min(offset, text.Length - 1));
            if (lineStart < 0)
                lineStart = 0;
            else
                lineStart++;

            var sb = new StringBuilder();
            for (var i = lineStart; i < text.Length; i++)
            {
                var c = text[i];
                if (c == ' ' || c == '\t')
                {
                    sb.Append(c);
                    continue;
                }

                break;
            }

            return sb.ToString();
        }

        private static string ApplyIndent(string text, string indent, string newline)
        {
            var lines = SplitLines(text);
            var sb = new StringBuilder();

            for (var i = 0; i < lines.Count; i++)
            {
                sb.Append(indent);
                sb.Append(lines[i]);

                if (i < lines.Count - 1)
                    sb.Append(newline);
            }

            return sb.ToString();
        }

        private static List<string> SplitLines(string text)
        {
            var list = new List<string>();
            var start = 0;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c != '\n')
                    continue;

                var len = i - start;
                if (len > 0 && text[i - 1] == '\r')
                    len--;

                list.Add(text.Substring(start, len));
                start = i + 1;
            }

            if (start <= text.Length)
                list.Add(text.Substring(start));

            return list;
        }

        private static bool StartsWith(string text, int index, string value)
        {
            if (index < 0 || index + value.Length > text.Length)
                return false;

            return string.Compare(text, index, value, 0, value.Length, StringComparison.Ordinal) == 0;
        }

        private static int IndexAfter(string text, int startIndex, string value)
        {
            var idx = text.IndexOf(value, startIndex, StringComparison.Ordinal);
            if (idx < 0)
                return -1;

            return idx + value.Length;
        }

        private static void TryPopUntil(Stack<string> stack, string name)
        {
            if (stack.Count == 0)
                return;

            if (!stack.Contains(name))
                return;

            while (stack.Count > 0)
            {
                var top = stack.Pop();
                if (string.Equals(top, name, StringComparison.Ordinal))
                    return;
            }
        }
    }
}
