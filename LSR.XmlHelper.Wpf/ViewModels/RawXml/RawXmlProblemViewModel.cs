using LSR.XmlHelper.Core.Models;
using System;
using System.Text.RegularExpressions;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class RawXmlProblemViewModel
    {
        private static readonly Regex MismatchTagNamesRegex = new Regex(
            "The '([^']+)' start tag.*end tag of '([^']+)'",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public RawXmlProblemViewModel(XmlParseProblem p)
        {
            Message = p.Message ?? "";
            Offset = p.Offset;
            LineNumber = p.LineNumber;
            ColumnNumber = p.ColumnNumber;
            Severity = p.Severity;

            var detail = BuildFriendlyDetail(Message);
            DisplayText = $"Ln {LineNumber}, Ch {ColumnNumber}: {detail}";
        }

        public string Message { get; }
        public int Offset { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }
        public XmlProblemSeverity Severity { get; }
        public bool IsWarning => Severity == XmlProblemSeverity.Warning;
        public string DisplayText { get; }

        private static string BuildFriendlyDetail(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "XML error.";

            var mismatch = MismatchTagNamesRegex.Match(message);
            if (mismatch.Success)
            {
                var startTag = mismatch.Groups[1].Value;
                var endTag = mismatch.Groups[2].Value;

                if (!string.IsNullOrWhiteSpace(startTag) && !string.IsNullOrWhiteSpace(endTag))
                {
                    return $"Mismatched tags: opening tag is <{startTag}> but closing tag is </{endTag}>. Fix: You need to add the closing tag </{startTag}> for <{startTag}>";
                }
            }

            if (message.Contains("Unexpected text between elements", StringComparison.OrdinalIgnoreCase))
                return "Extra text found between tags. Fix: remove it, wrap it in an element, or make it a comment.";

            if (message.Contains("expected token is '>'", StringComparison.OrdinalIgnoreCase))
                return "A tag is missing '>'. Fix: add '>' at the end of the highlighted tag.";

            return message;
        }

    }
}
