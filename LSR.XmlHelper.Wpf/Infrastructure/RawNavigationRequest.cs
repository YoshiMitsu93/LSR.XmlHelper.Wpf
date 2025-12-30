using System;

namespace LSR.XmlHelper.Wpf.Infrastructure
{
    public sealed class RawNavigationRequest
    {
        public RawNavigationRequest(string filePath, int offset, int length)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            Offset = offset;
            Length = length;
        }

        public string FilePath { get; }
        public int Offset { get; }
        public int Length { get; }
    }
}
