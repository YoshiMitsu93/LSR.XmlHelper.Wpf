using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class EditModeSaveTransactionService
    {
        public bool TryCommit(IReadOnlyDictionary<string, string> writes, out string error)
        {
            error = "";

            if (writes is null || writes.Count == 0)
                return true;

            var orderedPaths = writes.Keys
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (orderedPaths.Count == 0)
                return true;

            var originals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var path in orderedPaths)
                {
                    if (!File.Exists(path))
                    {
                        error = $"Missing file: {path}";
                        return false;
                    }

                    originals[path] = File.ReadAllText(path);
                }
            }
            catch (Exception ex)
            {
                error = "Failed to read original file contents:\r\n" + ex.Message;
                return false;
            }

            var written = new List<string>();

            try
            {
                foreach (var path in orderedPaths)
                {
                    if (!writes.TryGetValue(path, out var content))
                        continue;

                    File.WriteAllText(path, content ?? "");
                    written.Add(path);
                }

                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    foreach (var path in written)
                    {
                        if (originals.TryGetValue(path, out var original))
                            File.WriteAllText(path, original);
                    }
                }
                catch (Exception rollbackEx)
                {
                    error =
                        "Save failed and rollback also failed.\r\n\r\n" +
                        "Save error:\r\n" + ex.Message + "\r\n\r\n" +
                        "Rollback error:\r\n" + rollbackEx.Message;

                    return false;
                }

                error = "Save failed. All written files were rolled back.\r\n\r\n" + ex.Message;
                return false;
            }
        }
    }
}
