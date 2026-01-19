using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Linq;

namespace LSR.XmlHelper.Wpf.Services.Appearance
{
    public static class XmlSyntaxHighlightingService
    {
        public static void Apply(TextEditor editor, string? xmlSyntaxForegroundHex)
        {
            if (editor is null)
                return;

            if (string.IsNullOrWhiteSpace(xmlSyntaxForegroundHex))
                return;

            if (!HexColorParser.TryParseColor(xmlSyntaxForegroundHex, out var color))
                return;

            var def = HighlightingManager.Instance.GetDefinition("XML");
            if (def is null)
                return;

            var brush = new SimpleHighlightingBrush(color);

            var colors = def.NamedHighlightingColors?.ToList();
            if (colors is null || colors.Count == 0)
            {
                editor.SyntaxHighlighting = def;
                return;
            }

            var picked = colors
                .Where(c => ShouldOverride(c?.Name))
                .ToList();

            if (picked.Count == 0)
            {
                picked = colors
                    .Where(c => !IsCommentLike(c?.Name))
                    .ToList();
            }

            foreach (var c in picked)
            {
                if (c is null)
                    continue;

                c.Foreground = brush;
            }

            editor.SyntaxHighlighting = def;
        }

        private static bool ShouldOverride(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var n = name.Trim();

            if (IsCommentLike(n))
                return false;

            return n.Contains("Tag", StringComparison.OrdinalIgnoreCase)
                || n.Contains("Attribute", StringComparison.OrdinalIgnoreCase)
                || n.Contains("Markup", StringComparison.OrdinalIgnoreCase)
                || n.Contains("Name", StringComparison.OrdinalIgnoreCase)
                || n.Contains("Element", StringComparison.OrdinalIgnoreCase)
                || n.Contains("Xml", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCommentLike(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return name.Contains("Comment", StringComparison.OrdinalIgnoreCase)
                || name.Contains("CData", StringComparison.OrdinalIgnoreCase);
        }
    }
}
