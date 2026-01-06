using LSR.XmlHelper.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlGlobalFriendlySearchService
    {
        private readonly XmlFriendlyViewService _friendly = new XmlFriendlyViewService();

        public async Task<IReadOnlyList<GlobalFriendlySearchHit>> SearchAsync(
      IReadOnlyList<string> filePaths,
      string query,
      bool caseSensitive,
      int maxResults,
      CancellationToken token,
      IProgress<int>? fileProcessedProgress = null,
      IProgress<string>? currentFileProgress = null,
      bool useParallelProcessing = true)
        {
            var results = new System.Collections.Concurrent.ConcurrentBag<GlobalFriendlySearchHit>();
            if (filePaths is null || filePaths.Count == 0)
                return Array.Empty<GlobalFriendlySearchHit>();

            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<GlobalFriendlySearchHit>();

            if (maxResults <= 0)
                return Array.Empty<GlobalFriendlySearchHit>();

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var degree = useParallelProcessing ? Math.Max(1, Environment.ProcessorCount / 2) : 1;
            var options = new ParallelOptions
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = degree
            };

            await Parallel.ForEachAsync(filePaths, options, async (path, ct) =>
            {
                if (!string.IsNullOrWhiteSpace(path))
                    currentFileProgress?.Report(path);

                try
                {
                    if (results.Count >= maxResults)
                        return;

                    if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                        return;

                    string text;
                    try
                    {
                        text = await File.ReadAllTextAsync(path, ct).ConfigureAwait(false);
                    }
                    catch
                    {
                        return;
                    }

                    if (text.IndexOf(query, comparison) < 0)
                        return;

                    var doc = _friendly.TryBuild(text);
                    if (doc is null)
                        return;

                    foreach (var col in doc.Collections)
                    {
                        ct.ThrowIfCancellationRequested();

                        if (results.Count >= maxResults)
                            return;

                        foreach (var entry in col.Entries)
                        {
                            ct.ThrowIfCancellationRequested();

                            if (results.Count >= maxResults)
                                return;

                            if (entry.Fields is null)
                                continue;

                            foreach (var kv in entry.Fields)
                            {
                                ct.ThrowIfCancellationRequested();

                                if (results.Count >= maxResults)
                                    return;

                                var fieldKey = kv.Key ?? "";
                                var fieldValue = kv.Value?.Value ?? "";

                                if (fieldKey.IndexOf(query, comparison) < 0 && fieldValue.IndexOf(query, comparison) < 0)
                                    continue;

                                var preview = $"{fieldKey}: {fieldValue}";
                                if (preview.Length > 240)
                                    preview = preview.Substring(0, 240);

                                results.Add(new GlobalFriendlySearchHit(
                                    path,
                                    col.Title,
                                    entry.Key,
                                    entry.Occurrence,
                                    fieldKey,
                                    preview));
                            }
                        }
                    }
                }
                finally
                {
                    fileProcessedProgress?.Report(1);
                }
            });

            return results.Take(maxResults).ToList();
        }
    }
}
