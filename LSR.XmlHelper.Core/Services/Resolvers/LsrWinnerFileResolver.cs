using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Resolvers
{
    public sealed class LsrWinnerFileResolver
    {
        public string? ResolveWinnerFile(string? basePath, string[] additivePaths, Func<XDocument, bool> match)
        {
            if (match is null)
                return null;

            string? winner = null;

            if (!string.IsNullOrWhiteSpace(basePath) && File.Exists(basePath))
            {
                if (TryMatch(basePath, match))
                    winner = basePath;
            }

            if (additivePaths != null)
            {
                foreach (var addPath in additivePaths)
                {
                    if (string.IsNullOrWhiteSpace(addPath) || !File.Exists(addPath))
                        continue;

                    if (TryMatch(addPath, match))
                        winner = addPath;
                }
            }

            return winner;
        }

        private bool TryMatch(string filePath, Func<XDocument, bool> match)
        {
            XDocument doc;

            try
            {
                doc = XDocument.Load(filePath, LoadOptions.None);
            }
            catch
            {
                return false;
            }

            try
            {
                return match(doc);
            }
            catch
            {
                return false;
            }
        }
    }
}
