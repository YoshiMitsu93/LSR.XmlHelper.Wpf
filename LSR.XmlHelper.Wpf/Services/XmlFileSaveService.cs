using LSR.XmlHelper.Core.Services;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class XmlFileSaveService
    {
        private readonly XmlDocumentService _xml;

        public XmlFileSaveService()
        {
            _xml = new XmlDocumentService();
        }

        public (bool Success, string? Error) Save(string filePath, string xml)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return (false, "Invalid file path.");

            try
            {
                _xml.SaveToFile(filePath, xml);
                return (true, null);
            }
            catch (System.Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
