using ICSharpCode.AvalonEdit.Document;
using LSR.XmlHelper.Wpf.ViewModels;
using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Infrastructure.Breadcrumbs
{
    public static class XmlBreadcrumbBuilder
    {
        public static List<BreadcrumbSegmentViewModel> Build(TextDocument document, int caretOffset)
        {
            var result = new List<BreadcrumbSegmentViewModel>();

            if (document is null)
                return result;

            var max = Math.Max(0, Math.Min(caretOffset, document.TextLength));

            var stack = new List<(string Name, int Offset)>();

            var i = 0;
            while (i < max)
            {
                var lt = document.Text.IndexOf('<', i);
                if (lt < 0 || lt >= max)
                    break;

                if (lt + 3 < max && document.GetText(lt, Math.Min(4, document.TextLength - lt)) == "<!--")
                {
                    var end = document.Text.IndexOf("-->", lt + 4, StringComparison.Ordinal);
                    if (end < 0)
                        break;

                    i = end + 3;
                    continue;
                }

                if (lt + 1 < max && document.GetCharAt(lt + 1) == '?')
                {
                    var end = document.Text.IndexOf("?>", lt + 2, StringComparison.Ordinal);
                    if (end < 0)
                        break;

                    i = end + 2;
                    continue;
                }

                if (lt + 8 < max && document.Text.IndexOf("<![CDATA[", lt, StringComparison.Ordinal) == lt)
                {
                    var end = document.Text.IndexOf("]]>", lt + 9, StringComparison.Ordinal);
                    if (end < 0)
                        break;

                    i = end + 3;
                    continue;
                }

                if (lt + 1 < max && document.GetCharAt(lt + 1) == '!')
                {
                    var end = document.Text.IndexOf('>', lt + 2);
                    if (end < 0)
                        break;

                    i = end + 1;
                    continue;
                }

                var gt = document.Text.IndexOf('>', lt + 1);
                if (gt < 0 || gt >= max)
                    break;

                var isEndTag = lt + 1 < document.TextLength && document.GetCharAt(lt + 1) == '/';
                var tagText = document.GetText(lt + 1, Math.Max(0, gt - (lt + 1)));

                if (isEndTag)
                {
                    var endTagName = ReadTagName(tagText, 1);
                    if (!string.IsNullOrWhiteSpace(endTagName))
                        PopTag(stack, endTagName);

                    i = gt + 1;
                    continue;
                }

                var nameStart = 0;
                while (nameStart < tagText.Length && char.IsWhiteSpace(tagText[nameStart]))
                    nameStart++;

                var name = ReadTagName(tagText, nameStart);
                if (string.IsNullOrWhiteSpace(name))
                {
                    i = gt + 1;
                    continue;
                }

                var isSelfClosing = IsSelfClosing(tagText);
                if (!isSelfClosing)
                    stack.Add((name, lt));

                i = gt + 1;
            }

            foreach (var item in stack)
            {
                result.Add(new BreadcrumbSegmentViewModel
                {
                    Title = item.Name,
                    Offset = item.Offset
                });
            }

            return result;
        }

        private static string ReadTagName(string tagText, int startIndex)
        {
            if (startIndex < 0 || startIndex >= tagText.Length)
                return "";

            var i = startIndex;
            while (i < tagText.Length && char.IsWhiteSpace(tagText[i]))
                i++;

            if (i >= tagText.Length)
                return "";

            var j = i;
            while (j < tagText.Length)
            {
                var c = tagText[j];
                if (char.IsWhiteSpace(c) || c == '/' || c == '>')
                    break;

                j++;
            }

            return j > i ? tagText.Substring(i, j - i) : "";
        }

        private static bool IsSelfClosing(string tagText)
        {
            var i = tagText.Length - 1;
            while (i >= 0 && char.IsWhiteSpace(tagText[i]))
                i--;

            return i >= 0 && tagText[i] == '/';
        }

        private static void PopTag(List<(string Name, int Offset)> stack, string name)
        {
            for (var i = stack.Count - 1; i >= 0; i--)
            {
                if (string.Equals(stack[i].Name, name, StringComparison.Ordinal))
                {
                    stack.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
