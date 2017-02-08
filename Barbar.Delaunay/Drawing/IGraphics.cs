using System;

namespace Barbar.Delaunay.Drawing
{
    public interface IGraphics : IDisposable
    {
        void FillPolygon(PortableColor color, PointInt32[] points);
        void DrawLine(PortableColor color, int x1, int y1, int x2, int y2);
        void FillEllipse(PortableColor color, int x, int y, int width, int height);
        void DrawRectangle(PortableColor color, int x, int y, int width, int height);
        void DrawPolygon(PortableColor color, PointInt32[] points);
    }
}
