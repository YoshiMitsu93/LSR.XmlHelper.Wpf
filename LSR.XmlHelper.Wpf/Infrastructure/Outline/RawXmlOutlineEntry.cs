using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Infrastructure.Outline
{
    public sealed class RawXmlOutlineEntry
    {
        public RawXmlOutlineEntry(string title, int offset, IReadOnlyList<RawXmlOutlineEntry> children)
        {
            Title = title;
            Offset = offset;
            Children = children;
        }

        public string Title { get; }
        public int Offset { get; }
        public IReadOnlyList<RawXmlOutlineEntry> Children { get; }
    }
}
