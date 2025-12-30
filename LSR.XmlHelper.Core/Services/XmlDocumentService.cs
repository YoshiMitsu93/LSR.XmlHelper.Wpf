using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlDocumentService
    {
        private readonly XmlBackupService _backup;

        public XmlDocumentService()
        {
            var root = new XmlHelperRootService();
            _backup = new XmlBackupService(root);
        }

        public string LoadFromFile(string filePath)
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public void SaveToFile(string filePath, string xml)
        {
            if (File.Exists(filePath))
                _backup.Backup(filePath);

            File.WriteAllText(filePath, xml, Encoding.UTF8);
        }

        public string Format(string xml)
        {
            var doc = XDocument.Parse(xml, LoadOptions.None);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "    ",
                OmitXmlDeclaration = doc.Declaration is null,
                NewLineHandling = NewLineHandling.Replace,
                NewLineChars = "\r\n",
                NewLineOnAttributes = false
            };

            using var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, settings))
                doc.Save(xw);

            return sw.ToString();
        }

        public (bool ok, string message) ValidateWellFormed(string xml)
        {
            try
            {
                _ = XDocument.Parse(xml, LoadOptions.None);
                return (true, "XML is well-formed.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
