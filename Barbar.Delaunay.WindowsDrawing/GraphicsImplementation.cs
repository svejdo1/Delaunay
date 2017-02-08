using Barbar.Delaunay.Drawing;
using System;
using System.Drawing;

namespace Barbar.Delaunay.WindowsDrawing
{
    internal sealed class GraphicsImplementation : IGraphics
    {
        private Graphics _graphics;

        public GraphicsImplementation(Graphics graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            _graphics = graphics;
        }

        public void Dispose() {
            Dispose(true);
        }

        ~GraphicsImplementation() {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            if (_graphics != null)
            {
                _graphics.Dispose();
                _graphics = null;
            }
        }

        public void DrawLine(PortableColor color, int x1, int y1, int x2, int y2)
        {
            _graphics.DrawLine(new Pen(color.ToColor()), x1, y1, x2, y2);
        }

        public void DrawPolygon(PortableColor color, PointInt32[] points)
        {
            var clone = new Point[points.Length];
            for(var i = 0; i < points.Length; i++)
            {
                clone[i] = new Point(points[i].X, points[i].Y);
            }

            _graphics.DrawPolygon(new Pen(color.ToColor()), clone);
        }

        public void DrawRectangle(PortableColor color, int x, int y, int width, int height)
        {
            _graphics.DrawRectangle(new Pen(color.ToColor()), x, y, width, height);
        }

        public void FillEllipse(PortableColor color, int x, int y, int width, int height)
        {
            _graphics.FillEllipse(new SolidBrush(color.ToColor()), x, y, width, height);
        }

        public void FillPolygon(PortableColor color, PointInt32[] points)
        {
            var clone = new Point[points.Length];
            for (var i = 0; i < points.Length; i++)
            {
                clone[i] = new Point(points[i].X, points[i].Y);
            }
            _graphics.FillPolygon(new SolidBrush(color.ToColor()), clone);
        }
    }
}
