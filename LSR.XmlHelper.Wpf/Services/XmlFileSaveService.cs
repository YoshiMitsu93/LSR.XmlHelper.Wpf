using System.IO;
using System.Text;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class XmlFileSaveService
    {
        public (bool Success, string? Error) Save(string filePath, string xml)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return (false, "Invalid file path.");

            try
            {
                File.WriteAllText(filePath, xml, Encoding.UTF8);
                return (true, null);
            }
            catch (IOException ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
