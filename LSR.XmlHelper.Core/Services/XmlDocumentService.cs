using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System;


namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlDocumentService
    {
        public string LoadFromFile(string filePath)
        {
            return File.ReadAllText(filePath, Encoding.UTF8);
        }

        public void SaveToFile(string filePath, string xml)
        {
            File.WriteAllText(filePath, xml, Encoding.UTF8);
        }

        public string Format(string xml)
        {
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = doc.Declaration is null,
                NewLineHandling = NewLineHandling.Replace,
                NewLineOnAttributes = false
            };

            using var sw = new StringWriter();
            using var xw = XmlWriter.Create(sw, settings);
            doc.Save(xw);

            return sw.ToString();
        }

        public (bool IsValid, string Message) ValidateWellFormed(string xml)
        {
            try
            {
                _ = XDocument.Parse(xml);
                return (true, "XML is well-formed.");
            }
            catch (XmlException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
