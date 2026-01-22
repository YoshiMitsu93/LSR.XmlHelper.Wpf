namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class RawXmlScopeRange
    {
        public RawXmlScopeRange(int startLine, int endLine, int depth)
        {
            StartLine = startLine;
            EndLine = endLine;
            Depth = depth;
        }

        public int StartLine { get; }
        public int EndLine { get; }
        public int Depth { get; }
    }
}
