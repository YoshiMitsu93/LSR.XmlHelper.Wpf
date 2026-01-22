namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public interface IRawXmlLineDepthProvider
    {
        bool TryGetLineDepth(int lineNumber, out int depth);
    }
}
