using System;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class XmlParseProblem
    {
        public XmlParseProblem(string message, int offset, int lineNumber, int columnNumber, XmlProblemSeverity severity)
        {
            Message = message ?? "";
            Offset = offset;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
            Severity = severity;
        }

        public string Message { get; }
        public int Offset { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }
        public XmlProblemSeverity Severity { get; }
    }
}
