using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class XmlFileLoaderService
    {
        public async Task<(bool Success, string? Text, string? Error)> LoadAsync(
            string filePath,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return (false, null, "Invalid file path.");

            if (!File.Exists(filePath))
                return (false, null, "File does not exist.");

            try
            {
                using var stream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 4096,
                    useAsync: true);

                using var reader = new StreamReader(stream, Encoding.UTF8);
                var text = await reader.ReadToEndAsync();

                if (cancellationToken.IsCancellationRequested)
                    return (false, null, null);

                return (true, text, null);
            }
            catch (IOException ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
