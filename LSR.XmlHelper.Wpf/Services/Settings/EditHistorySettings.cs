using LSR.XmlHelper.Wpf.Services.EditHistory;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class EditHistorySettings
    {
        public List<EditHistoryItem> Pending { get; set; } = new List<EditHistoryItem>();
        public List<EditHistoryItem> Committed { get; set; } = new List<EditHistoryItem>();
    }
}
