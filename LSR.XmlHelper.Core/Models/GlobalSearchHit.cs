using System;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class GlobalSearchHit
    {
        public GlobalSearchHit(string filePath, int offset, int length, int lineNumber, int columnNumber, string previewLine)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Offset = offset;
            Length = length;
            LineNumber = lineNumber;
            ColumnNumber = columnNumber;
            PreviewLine = previewLine ?? "";
        }

        public string FilePath { get; }
        public int Offset { get; }
        public int Length { get; }
        public int LineNumber { get; }
        public int ColumnNumber { get; }
        public string PreviewLine { get; }
    }
}
