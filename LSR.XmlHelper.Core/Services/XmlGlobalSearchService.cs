using LSR.XmlHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlGlobalSearchService
    {
        public async Task<IReadOnlyList<GlobalSearchHit>> SearchAsync(
            IReadOnlyList<string> filePaths,
            string query,
            bool caseSensitive,
            int maxResults,
            CancellationToken cancellationToken,
            IProgress<int>? fileProcessedProgress = null,
            IProgress<string>? currentFileProgress = null)
        {
            if (filePaths is null)
                throw new ArgumentNullException(nameof(filePaths));

            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<GlobalSearchHit>();

            if (maxResults <= 0)
                return Array.Empty<GlobalSearchHit>();

            var hits = new List<GlobalSearchHit>(Math.Min(maxResults, 256));
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            foreach (var path in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrWhiteSpace(path))
                    currentFileProgress?.Report(path);

                try
                {
                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                        continue;

                    string text;
                    try
                    {
                        text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
                    }
                    catch
                    {
                        continue;
                    }

                    var startIndex = 0;
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var idx = text.IndexOf(query, startIndex, comparison);
                        if (idx < 0)
                            break;

                        var (line, col) = GetLineAndColumn(text, idx);
                        var preview = GetPreviewLine(text, idx);

                        hits.Add(new GlobalSearchHit(path, idx, query.Length, line, col, preview));

                        if (hits.Count >= maxResults)
                            return hits;

                        startIndex = idx + (query.Length > 0 ? query.Length : 1);
                    }
                }
                finally
                {
                    fileProcessedProgress?.Report(1);
                }
            }

            return hits;
        }

        private static (int lineNumber, int columnNumber) GetLineAndColumn(string text, int offset)
        {
            var line = 1;
            var lastLineStart = 0;

            for (var i = 0; i < offset && i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    line++;
                    lastLineStart = i + 1;
                }
            }

            return (line, (offset - lastLineStart) + 1);
        }

        private static string GetPreviewLine(string text, int offset)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var start = offset;
            while (start > 0 && text[start - 1] != '\n' && text[start - 1] != '\r')
                start--;

            var end = offset;
            while (end < text.Length && text[end] != '\n' && text[end] != '\r')
                end++;

            var line = text.Substring(start, end - start).Trim();
            return line.Length > 240 ? line.Substring(0, 240) + "…" : line;
        }
    }
}
