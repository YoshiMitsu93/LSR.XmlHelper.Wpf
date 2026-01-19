using System;
using System.Text.Json.Serialization;

namespace LSR.XmlHelper.Wpf.Models
{
    public sealed class XmlGuide
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "";
        public string Category { get; set; } = "Uncategorized";
        public string Summary { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public bool IsBuiltIn { get; set; }
    }
}
