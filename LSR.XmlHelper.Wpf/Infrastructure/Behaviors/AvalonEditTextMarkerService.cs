using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Windows;
using System.Windows.Media;

namespace LSR.XmlHelper.Wpf.Infrastructure.Behaviors
{
    public sealed class AvalonEditTextMarkerService : IBackgroundRenderer
    {
        private sealed class Marker : TextSegment
        {
            public string ToolTip { get; set; } = "";
            public bool IsWarning { get; set; }
        }

        private readonly TextSegmentCollection<Marker> _markers;

        public AvalonEditTextMarkerService(TextDocument document)
        {
            _markers = new TextSegmentCollection<Marker>(document ?? throw new ArgumentNullException(nameof(document)));
        }

        public KnownLayer Layer => KnownLayer.Caret;

        public void Clear()
        {
            _markers.Clear();
        }

        public void AddSquiggle(int offset, int length, string toolTip, bool isWarning)
        {
            if (length < 2)
                length = 2;

            _markers.Add(new Marker
            {
                StartOffset = offset,
                Length = length,
                ToolTip = toolTip ?? "",
                IsWarning = isWarning
            });
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (textView is null || drawingContext is null)
                return;

            if (!textView.VisualLinesValid)
                return;

            var errorPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Red, 1.6);
            var warningPen = new System.Windows.Media.Pen(System.Windows.Media.Brushes.Orange, 1.2);

            foreach (var m in _markers)
            {
                var pen = m.IsWarning ? warningPen : errorPen;
                foreach (var r in BackgroundGeometryBuilder.GetRectsForSegment(textView, m))
                {
                    var startPoint = new System.Windows.Point(r.Left, r.Bottom - 1);
                    var endPoint = new System.Windows.Point(r.Right, r.Bottom - 1);

                    if (m.IsWarning)
                        DrawDottedLine(drawingContext, pen, startPoint, endPoint);
                    else
                        DrawWavyLine(drawingContext, pen, startPoint, endPoint);
                }
            }
        }

        private static void DrawWavyLine(DrawingContext dc, System.Windows.Media.Pen pen, System.Windows.Point start, System.Windows.Point end)
        {
            var geometry = new StreamGeometry();

            using (var ctx = geometry.Open())
            {
                ctx.BeginFigure(start, false, false);

                var x = start.X;
                var y = start.Y;
                var up = true;

                while (x < end.X)
                {
                    x += 3;
                    y += up ? -2 : 2;
                    ctx.LineTo(new System.Windows.Point(Math.Min(x, end.X), y), true, false);
                    up = !up;
                }
            }

            geometry.Freeze();
            dc.DrawGeometry(null, pen, geometry);
        }

        private static void DrawDottedLine(DrawingContext dc, System.Windows.Media.Pen pen, System.Windows.Point start, System.Windows.Point end)
        {
            var x = start.X;
            var y = start.Y;
            while (x < end.X)
            {
                var x2 = Math.Min(end.X, x + 2.5);
                dc.DrawLine(pen, new System.Windows.Point(x, y), new System.Windows.Point(x2, y));
                x += 5;
            }
        }
    }
}
