using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class AvalonEditScopeShadingService : IBackgroundRenderer, IRawXmlLineDepthProvider
    {
        private readonly TextDocument _document;
        private int[] _lineDepth = Array.Empty<int>();
        private readonly DispatcherTimer _rebuildTimer;
        private TextView? _lastTextView;
        private bool _isEnabled = true;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value)
                    return;

                _isEnabled = value;

                if (!_isEnabled)
                    Clear();

                Invalidate();
            }
        }

        private bool _isDarkMode;
        private string _scopeShadingColor = "";
        private bool _isScopeShadingEnabled = true;
        private bool _isRegionHighlightEnabled = true;
        private readonly Dictionary<int, SolidColorBrush> _brushCache = new();
        private string _regionHighlightColor = "";
        private SolidColorBrush? _regionHighlightBrush;
        private int _caretLine;
        private IReadOnlyList<RawXmlScopeRange> _scopes = Array.Empty<RawXmlScopeRange>();
        private int _currentRegionStartLine;
        private int _currentRegionEndLine;
        private int _currentRegionDepth;

        public AvalonEditScopeShadingService(TextDocument document)
        {
            _document = document;

            _rebuildTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _rebuildTimer.Tick += (_, __) => RebuildScopesNow();

            _document.Changed += (_, __) => ScheduleRebuild();
            ScheduleRebuild();
        }

        private void ScheduleRebuild()
        {
            if (!IsEnabled)
                return;

            _rebuildTimer.Stop();
            _rebuildTimer.Start();
        }

        private void RebuildScopesNow()
        {
            _rebuildTimer.Stop();

            if (!IsEnabled)
                return;

            var scopes = RawXmlScopeShadingBuilder.TryGetScopes(_document.Text);
            SetScopes(scopes);
        }

        private void Invalidate()
        {
            if (_lastTextView is null)
                return;

            _lastTextView.InvalidateLayer(Layer);
            _lastTextView.InvalidateVisual();
        }


        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode == value)
                    return;

                _isDarkMode = value;
                _brushCache.Clear();
                _regionHighlightBrush = null;
                Invalidate();
            }
        }

        public KnownLayer Layer => KnownLayer.Selection;
        public string ScopeShadingColor
        {
            get => _scopeShadingColor;
            set
            {
                value ??= "";
                if (_scopeShadingColor == value)
                    return;

                _scopeShadingColor = value;
                _brushCache.Clear();
                Invalidate();
            }
        }
        public bool IsScopeShadingEnabled
        {
            get => _isScopeShadingEnabled;
            set
            {
                if (_isScopeShadingEnabled == value)
                    return;

                _isScopeShadingEnabled = value;
                Invalidate();
            }
        }

        public bool IsRegionHighlightEnabled
        {
            get => _isRegionHighlightEnabled;
            set
            {
                if (_isRegionHighlightEnabled == value)
                    return;

                _isRegionHighlightEnabled = value;
                _regionHighlightBrush = null;
                Invalidate();
            }
        }

        public string RegionHighlightColor
        {
            get => _regionHighlightColor;
            set
            {
                value ??= "";
                if (_regionHighlightColor == value)
                    return;

                _regionHighlightColor = value;
                _regionHighlightBrush = null;
                Invalidate();
            }
        }

        public int CaretLine
        {
            get => _caretLine;
            set
            {
                if (_caretLine == value)
                    return;

                _caretLine = value;
                UpdateCurrentRegion();
                Invalidate();
            }
        }

        public void Clear()
        {
            _lineDepth = Array.Empty<int>();
            _scopes = Array.Empty<RawXmlScopeRange>();
            _currentRegionStartLine = 0;
            _currentRegionEndLine = 0;
            _currentRegionDepth = 0;
            Invalidate();
        }

        public void SetScopes(IReadOnlyList<RawXmlScopeRange> scopes)
        {
            _scopes = scopes ?? Array.Empty<RawXmlScopeRange>();

            var lineCount = _document.LineCount;
            var depths = new int[lineCount + 1];

            foreach (var s in _scopes)
            {
                if (s.EndLine < 1)
                    continue;

                var start = Math.Max(1, s.StartLine);
                var end = Math.Min(lineCount, s.EndLine);

                for (var line = start; line <= end; line++)
                    depths[line] = Math.Max(depths[line], s.Depth + 1);
            }

            _lineDepth = depths;
            UpdateCurrentRegion();
            Invalidate();
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!_isEnabled)
                return;

            if (!ReferenceEquals(_lastTextView, textView))
                _lastTextView = textView;

            if (_document is null || textView?.Document is null)
                return;

            if (!textView.VisualLinesValid)
                return;

            var width = textView.ActualWidth;
            if (double.IsNaN(width) || width <= 0)
                return;

            var regionBrush = GetRegionHighlightBrush();
            var regionStart = _currentRegionStartLine;
            var regionEnd = _currentRegionEndLine;

            foreach (var visualLine in textView.VisualLines)
            {
                var docLine = visualLine.FirstDocumentLine;
                if (docLine is null)
                    continue;

                var lineNumber = docLine.LineNumber;

                if (_isScopeShadingEnabled && !string.IsNullOrWhiteSpace(_scopeShadingColor) && lineNumber > 0 && lineNumber < _lineDepth.Length)
                {
                    var depth = _lineDepth[lineNumber];
                    if (depth > 0)
                    {
                        var brush = GetBrush(depth);

                        var segment = new TextSegment
                        {
                            StartOffset = docLine.Offset,
                            Length = Math.Max(0, docLine.Length)
                        };

                        foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, segment))
                        {
                            var rect = new Rect(0, r.Top, width, r.Height);
                            drawingContext.DrawRectangle(brush, null, rect);
                        }
                    }
                }

                if (regionBrush is null)
                    continue;

                if (regionStart == 0 || regionEnd == 0)
                    continue;

                if (lineNumber < regionStart || lineNumber > regionEnd)
                    continue;

                var regionSegment = new TextSegment
                {
                    StartOffset = docLine.Offset,
                    Length = Math.Max(0, docLine.Length)
                };

                foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, regionSegment))
                {
                    var rect = new Rect(0, r.Top, width, r.Height);
                    drawingContext.DrawRectangle(regionBrush, null, rect);
                }
            }
        }

        private void UpdateCurrentRegion()
        {
            _currentRegionStartLine = 0;
            _currentRegionEndLine = 0;
            _currentRegionDepth = 0;

            if (_caretLine <= 0)
                return;

            if (_scopes is null || _scopes.Count == 0)
                return;

            var bestDepth = -1;
            var bestSpan = int.MaxValue;
            var bestStart = 0;
            var bestEnd = 0;

            foreach (var s in _scopes)
            {
                if (s.StartLine <= 0 || s.EndLine <= 0)
                    continue;

                if (_caretLine < s.StartLine || _caretLine > s.EndLine)
                    continue;

                var span = s.EndLine - s.StartLine;

                if (s.Depth > bestDepth || (s.Depth == bestDepth && span < bestSpan))
                {
                    bestDepth = s.Depth;
                    bestSpan = span;
                    bestStart = s.StartLine;
                    bestEnd = s.EndLine;
                }
            }

            if (bestDepth < 0)
                return;

            _currentRegionStartLine = bestStart;
            _currentRegionEndLine = bestEnd;
            _currentRegionDepth = bestDepth;
        }

        private SolidColorBrush? GetRegionHighlightBrush()
        {
            if (!IsRegionHighlightEnabled || string.IsNullOrWhiteSpace(_regionHighlightColor))
                return null;

            if (_regionHighlightBrush is not null)
                return _regionHighlightBrush;

            System.Windows.Media.Color baseColor;
            try
            {
                var converted = System.Windows.Media.ColorConverter.ConvertFromString(_regionHighlightColor);
                if (converted is not System.Windows.Media.Color c)
                    return null;

                baseColor = c;
            }
            catch
            {
                return null;
            }

            byte alpha;
            if (IsDarkMode)
                alpha = 45;
            else
                alpha = 25;

            var color = System.Windows.Media.Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            _regionHighlightBrush = brush;
            return brush;
        }

        private bool TryParseScopeBaseColor(out System.Windows.Media.Color color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(_scopeShadingColor))
                return false;

            try
            {
                var converted = System.Windows.Media.ColorConverter.ConvertFromString(_scopeShadingColor);
                if (converted is System.Windows.Media.Color c)
                {
                    color = c;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static System.Windows.Media.Color CreateScopeVariant(System.Windows.Media.Color baseColor, int group, bool isDarkMode)
        {
            var (h, s, l) = RgbToHsl(baseColor);

            var offset = group switch
            {
                1 => 18.0,
                2 => -18.0,
                3 => 36.0,
                _ => 0.0
            };

            h = (h + offset) % 360.0;
            if (h < 0)
                h += 360.0;

            if (isDarkMode)
                l = Clamp01(l * 0.92 + group * 0.02);
            else
                l = Clamp01(l * 0.98 - group * 0.015);

            s = Clamp01(s * 0.90);

            return HslToRgb(h, s, l);
        }

        private static (double H, double S, double L) RgbToHsl(System.Windows.Media.Color c)
        {
            var r = c.R / 255.0;
            var g = c.G / 255.0;
            var b = c.B / 255.0;

            var max = Math.Max(r, Math.Max(g, b));
            var min = Math.Min(r, Math.Min(g, b));
            var delta = max - min;

            var l = (max + min) / 2.0;

            if (delta == 0)
                return (0.0, 0.0, l);

            var s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            double h;
            if (max == r)
                h = (g - b) / delta + (g < b ? 6.0 : 0.0);
            else if (max == g)
                h = (b - r) / delta + 2.0;
            else
                h = (r - g) / delta + 4.0;

            h *= 60.0;
            return (h, s, l);
        }

        private static System.Windows.Media.Color HslToRgb(double h, double s, double l)
        {
            double r;
            double g;
            double b;

            if (s == 0)
            {
                r = l;
                g = l;
                b = l;
            }
            else
            {
                var q = l < 0.5 ? l * (1.0 + s) : l + s - l * s;
                var p = 2.0 * l - q;
                var hk = h / 360.0;
                r = HueToRgb(p, q, hk + 1.0 / 3.0);
                g = HueToRgb(p, q, hk);
                b = HueToRgb(p, q, hk - 1.0 / 3.0);
            }

            return System.Windows.Media.Color.FromRgb((byte)Math.Round(r * 255.0), (byte)Math.Round(g * 255.0), (byte)Math.Round(b * 255.0));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0.0)
                t += 1.0;
            if (t > 1.0)
                t -= 1.0;
            if (t < 1.0 / 6.0)
                return p + (q - p) * 6.0 * t;
            if (t < 1.0 / 2.0)
                return q;
            if (t < 2.0 / 3.0)
                return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
            return p;
        }

        private static double Clamp01(double v)
        {
            if (v < 0.0)
                return 0.0;
            if (v > 1.0)
                return 1.0;
            return v;
        }

        private SolidColorBrush GetBrush(int depth)
        {
            if (_brushCache.TryGetValue(depth, out var existing))
                return existing;

            var group = Math.Abs(depth) % 4;

            byte alpha;
            if (IsDarkMode)
                alpha = (byte)Math.Min(130, 55 + group * 20);
            else
                alpha = (byte)Math.Min(100, 35 + group * 15);

            System.Windows.Media.Color baseColor;
            if (TryParseScopeBaseColor(out var custom))
            {
                baseColor = CreateScopeVariant(custom, group, IsDarkMode);
            }
            else if (IsDarkMode)
            {
                baseColor = group switch
                {
                    0 => System.Windows.Media.Color.FromRgb(0, 110, 180),
                    1 => System.Windows.Media.Color.FromRgb(0, 140, 90),
                    2 => System.Windows.Media.Color.FromRgb(150, 90, 0),
                    _ => System.Windows.Media.Color.FromRgb(120, 0, 140)
                };
            }
            else
            {
                baseColor = group switch
                {
                    0 => System.Windows.Media.Color.FromRgb(225, 240, 255),
                    1 => System.Windows.Media.Color.FromRgb(225, 255, 235),
                    2 => System.Windows.Media.Color.FromRgb(255, 245, 225),
                    _ => System.Windows.Media.Color.FromRgb(245, 225, 255)
                };
            }

            var color = System.Windows.Media.Color.FromArgb(alpha, baseColor.R, baseColor.G, baseColor.B);

            var brush = new SolidColorBrush(color);
            brush.Freeze();

            _brushCache[depth] = brush;
            return brush;
        }

        public sealed class RawXmlScopeRange
        {
            public RawXmlScopeRange(int startLine, int endLine, int depth)
            {
                StartLine = startLine;
                EndLine = endLine;
                Depth = depth;
            }

            public int StartLine { get; }
            public int EndLine { get; }
            public int Depth { get; }
        }

        public static class RawXmlScopeShadingBuilder
        {
            public static List<RawXmlScopeRange> TryGetScopes(string xml)
            {
                if (string.IsNullOrWhiteSpace(xml))
                    return new List<RawXmlScopeRange>();

                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true
                };

                var result = new List<RawXmlScopeRange>();
                var stack = new Stack<(int StartLine, int Depth)>();

                try
                {
                    using var sr = new StringReader(xml);
                    using var reader = XmlReader.Create(sr, settings);

                    while (reader.Read())
                    {
                        if (reader is not IXmlLineInfo li || li.HasLineInfo() == false)
                            continue;

                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            var startLine = li.LineNumber;
                            var depth = stack.Count;

                            if (reader.IsEmptyElement)
                            {
                                result.Add(new RawXmlScopeRange(startLine, startLine, depth));
                            }
                            else
                            {
                                stack.Push((startLine, depth));
                            }

                            continue;
                        }

                        if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            if (stack.Count == 0)
                                continue;

                            var (startLine, depth) = stack.Pop();
                            var endLine = li.LineNumber;

                            result.Add(new RawXmlScopeRange(startLine, endLine, depth));
                        }
                    }
                }
                catch
                {
                    var tolerantScopes = RawXmlTolerantScopeScanner.GetScopes(xml);
                    var converted = new List<RawXmlScopeRange>(tolerantScopes.Count);

                    foreach (var s in tolerantScopes)
                        converted.Add(new RawXmlScopeRange(s.StartLine, s.EndLine, s.Depth));

                    return converted;
                }

                return result;
            }
        }

        public bool TryGetLineDepth(int lineNumber, out int depth)
        {
            if (lineNumber <= 0 || lineNumber >= _lineDepth.Length)
            {
                depth = 0;
                return false;
            }

            depth = _lineDepth[lineNumber];
            return true;
        }
    }
}
