using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace LSR.XmlHelper.Wpf.Services.Guides
{
    public sealed class EmbeddedGuidesSourceService
    {
        public string EnsureExtracted(string helperRootFolder)
        {
            var appDataRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LSR.XmlHelper.Wpf");

            Directory.CreateDirectory(appDataRoot);

            var extractedRoot = Path.Combine(appDataRoot, "_AppGuidesSource");
            var hashFilePath = Path.Combine(extractedRoot, ".embedded-guides.sha256");

            var resourceName = FindGuidesZipResourceName();
            if (string.IsNullOrWhiteSpace(resourceName))
                return "";

            var embeddedHash = ComputeResourceSha256(resourceName);
            if (string.IsNullOrWhiteSpace(embeddedHash))
                return "";

            if (Directory.Exists(extractedRoot) && File.Exists(hashFilePath))
            {
                var existingHash = File.ReadAllText(hashFilePath).Trim();
                if (string.Equals(existingHash, embeddedHash, StringComparison.OrdinalIgnoreCase))
                    return extractedRoot;
            }

            if (Directory.Exists(extractedRoot))
                Directory.Delete(extractedRoot, true);

            Directory.CreateDirectory(extractedRoot);

            using (var stream = OpenResourceStream(resourceName))
            {
                if (stream is null)
                    return "";

                ZipFile.ExtractToDirectory(stream, extractedRoot, true);
            }

            File.WriteAllText(hashFilePath, embeddedHash);

            return extractedRoot;
        }

        private static string? FindGuidesZipResourceName()
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceNames()
                .FirstOrDefault(x => x.EndsWith("Guides.zip", StringComparison.OrdinalIgnoreCase));
        }

        private static Stream? OpenResourceStream(string resourceName)
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetManifestResourceStream(resourceName);
        }

        private static string ComputeResourceSha256(string resourceName)
        {
            using (var stream = OpenResourceStream(resourceName))
            {
                if (stream is null)
                    return "";

                using (var sha = SHA256.Create())
                {
                    var hash = sha.ComputeHash(stream);
                    return BytesToHex(hash);
                }
            }
        }

        private static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            for (var i = 0; i < bytes.Length; i++)
                sb.Append(bytes[i].ToString("x2"));
            return sb.ToString();
        }
    }
}
