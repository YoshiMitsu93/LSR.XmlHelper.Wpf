namespace LSR.XmlHelper.Wpf.Models
{
    public sealed class HelpTopic
    {
        public HelpTopic(string category, string title, string summary, string body, params string[] keywords)
            : this(int.MaxValue, category, int.MaxValue, title, summary, body, keywords)
        {
        }

        public HelpTopic(int categoryOrder, string category, int topicOrder, string title, string summary, string body, params string[] keywords)
        {
            CategoryOrder = categoryOrder;
            Category = category;
            TopicOrder = topicOrder;
            Title = title;
            Summary = summary;
            Body = body;
            Keywords = keywords ?? System.Array.Empty<string>();
        }

        public int CategoryOrder { get; }
        public string Category { get; }
        public int TopicOrder { get; }
        public string Title { get; }
        public string Summary { get; }
        public string Body { get; }
        public string[] Keywords { get; }

        public string SearchBlob => $"{Category}\n{Title}\n{Summary}\n{Body}\n{string.Join(" ", Keywords)}";
    }
}
