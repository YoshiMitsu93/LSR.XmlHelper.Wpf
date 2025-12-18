using System.Xml.Linq;

public sealed class XmlFriendlyEntry
{
    public XmlFriendlyEntry(string key, string display, XElement element, Dictionary<string, XElement> fields)
        : this(key, display, element, fields, new List<LSR.XmlHelper.Core.Models.XmlFriendlyChildCollection>())
    {
    }

    public XmlFriendlyEntry(
        string key,
        string display,
        XElement element,
        Dictionary<string, XElement> fields,
        List<LSR.XmlHelper.Core.Models.XmlFriendlyChildCollection> childCollections)
    {
        Key = key;
        Display = display;
        Element = element;
        Fields = fields;
        ChildCollections = childCollections ?? new List<LSR.XmlHelper.Core.Models.XmlFriendlyChildCollection>();
    }

    public string Key { get; }
    public string Display { get; }
    public XElement Element { get; }
    public Dictionary<string, XElement> Fields { get; }

    public List<LSR.XmlHelper.Core.Models.XmlFriendlyChildCollection> ChildCollections { get; }

    public bool TrySetField(string fieldKey, string newValue, out string? error)
    {
        error = null;

        if (!Fields.TryGetValue(fieldKey, out var el))
        {
            error = $"Field '{fieldKey}' was not found.";
            return false;
        }

        var isSynthetic = string.Equals(el.Attribute("__synthetic")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        if (isSynthetic)
        {
            error = $"Field '{fieldKey}' is read-only.";
            return false;
        }

        try
        {
            el.Value = newValue ?? string.Empty;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
