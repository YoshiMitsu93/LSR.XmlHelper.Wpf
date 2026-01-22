using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public static class RawXmlTolerantScopeScanner
    {
        public static List<RawXmlScopeRange> GetScopes(string xml)
        {
            var result = new List<RawXmlScopeRange>();

            if (string.IsNullOrWhiteSpace(xml))
                return result;

            var stack = new Stack<(int StartLine, int Depth)>();

            var i = 0;
            var line = 1;

            while (i < xml.Length)
            {
                var ch = xml[i];

                if (ch == '\r')
                {
                    if (i + 1 < xml.Length && xml[i + 1] == '\n')
                        i++;

                    line++;
                    i++;
                    continue;
                }

                if (ch == '\n')
                {
                    line++;
                    i++;
                    continue;
                }

                if (ch != '<')
                {
                    i++;
                    continue;
                }

                var tagLine = line;

                if (i + 1 >= xml.Length)
                    break;

                var next = xml[i + 1];

                if (next == '!' && StartsWith(xml, i + 2, "--"))
                {
                    i = SkipUntil(xml, i + 4, "-->", ref line);
                    continue;
                }

                if (next == '!' && StartsWith(xml, i + 2, "[CDATA["))
                {
                    i = SkipUntil(xml, i + 9, "]]>", ref line);
                    continue;
                }

                if (next == '?')
                {
                    i = SkipUntil(xml, i + 2, "?>", ref line);
                    continue;
                }

                var isEndTag = next == '/';
                var j = i + (isEndTag ? 2 : 1);

                while (j < xml.Length && char.IsWhiteSpace(xml[j]))
                {
                    if (xml[j] == '\r')
                    {
                        if (j + 1 < xml.Length && xml[j + 1] == '\n')
                            j++;

                        line++;
                    }
                    else if (xml[j] == '\n')
                    {
                        line++;
                    }

                    j++;
                }

                var nameStart = j;

                while (j < xml.Length && IsNameChar(xml[j]))
                    j++;

                var name = xml.Substring(nameStart, Math.Max(0, j - nameStart));

                if (string.IsNullOrWhiteSpace(name))
                {
                    i++;
                    continue;
                }

                var tagEnd = FindTagEnd(xml, j, ref line);
                if (tagEnd < 0)
                    break;

                var isSelfClosing = !isEndTag && IsSelfClosing(xml, i, tagEnd);

                if (isEndTag)
                {
                    if (stack.Count > 0)
                    {
                        var (startLine, depth) = stack.Pop();
                        result.Add(new RawXmlScopeRange(startLine, tagLine, depth));
                    }
                }
                else
                {
                    var depth = stack.Count;

                    if (isSelfClosing)
                    {
                        result.Add(new RawXmlScopeRange(tagLine, tagLine, depth));
                    }
                    else
                    {
                        stack.Push((tagLine, depth));
                    }
                }

                i = tagEnd + 1;
            }

            var lastLine = Math.Max(1, line);

            while (stack.Count > 0)
            {
                var (startLine, depth) = stack.Pop();
                result.Add(new RawXmlScopeRange(startLine, lastLine, depth));
            }

            return result;
        }

        private static bool IsNameChar(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_' || c == ':' || c == '-' || c == '.';
        }

        private static bool StartsWith(string s, int index, string value)
        {
            if (index < 0)
                return false;

            if (index + value.Length > s.Length)
                return false;

            for (var k = 0; k < value.Length; k++)
            {
                if (s[index + k] != value[k])
                    return false;
            }

            return true;
        }

        private static int SkipUntil(string s, int start, string terminator, ref int line)
        {
            var i = start;

            while (i < s.Length)
            {
                if (s[i] == '\r')
                {
                    if (i + 1 < s.Length && s[i + 1] == '\n')
                        i++;

                    line++;
                    i++;
                    continue;
                }

                if (s[i] == '\n')
                {
                    line++;
                    i++;
                    continue;
                }

                if (StartsWith(s, i, terminator))
                    return i + terminator.Length;

                i++;
            }

            return s.Length;
        }

        private static int FindTagEnd(string s, int start, ref int line)
        {
            var i = start;
            char quote = '\0';

            while (i < s.Length)
            {
                var ch = s[i];

                if (ch == '\r')
                {
                    if (i + 1 < s.Length && s[i + 1] == '\n')
                        i++;

                    line++;
                    i++;
                    continue;
                }

                if (ch == '\n')
                {
                    line++;
                    i++;
                    continue;
                }

                if (quote != '\0')
                {
                    if (ch == quote)
                        quote = '\0';

                    i++;
                    continue;
                }

                if (ch == '"' || ch == '\'')
                {
                    quote = ch;
                    i++;
                    continue;
                }

                if (ch == '>')
                    return i;

                i++;
            }

            return -1;
        }

        private static bool IsSelfClosing(string s, int tagStart, int tagEnd)
        {
            var k = tagEnd - 1;

            while (k > tagStart && char.IsWhiteSpace(s[k]))
                k--;

            return k > tagStart && s[k] == '/';
        }
    }
}
