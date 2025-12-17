using System.IO;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlHelperRootService
    {
        public string GetHelperRootForXmlPath(string xmlPath)
        {
            var dir = Path.GetDirectoryName(xmlPath);
            if (string.IsNullOrWhiteSpace(dir))
                return Path.Combine(Directory.GetCurrentDirectory(), "LSR-XML-Helper");

            var helperRoot = Path.Combine(dir, "LSR-XML-Helper");
            Directory.CreateDirectory(helperRoot);
            return helperRoot;
        }

        public string GetOrCreateSubfolder(string xmlPath, string folderName)
        {
            var root = GetHelperRootForXmlPath(xmlPath);
            var sub = Path.Combine(root, folderName);
            Directory.CreateDirectory(sub);
            return sub;
        }
    }
}
