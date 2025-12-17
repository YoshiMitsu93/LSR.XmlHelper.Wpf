using LSR.XmlHelper.Core.Services;
using System.Threading;
using System.Threading.Tasks;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class XmlFileLoaderService
    {
        private readonly XmlDocumentService _xml;

        public XmlFileLoaderService()
        {
            _xml = new XmlDocumentService();
        }

        public Task<(bool Success, string? Text, string? Error)> LoadAsync(
            string filePath,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return Task.FromResult<(bool, string?, string?)>((false, null, "Invalid file path."));

            return Task.Run<(bool Success, string? Text, string? Error)>(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return (false, null, "Cancelled.");

                try
                {
                    var text = _xml.LoadFromFile(filePath);
                    return (true, text, null);
                }
                catch (System.Exception ex)
                {
                    return (false, null, ex.Message);
                }
            }, cancellationToken);
        }
    }
}
