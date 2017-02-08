using System;
using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    public sealed class Polygon
    {

        private List<PointDouble> _vertices;

        public Polygon(List<PointDouble> vertices)
        {
            _vertices = vertices;
        }

        public double Area()
        {
            return Math.Abs(SignedDoubleArea() * 0.5);
        }

        public Winding Winding()
        {
            double signedDoubleArea = SignedDoubleArea();
            if (signedDoubleArea < 0)
            {
                return Core.Winding.Clockwise;
            }
            if (signedDoubleArea > 0)
            {
                return Core.Winding.CounterClockwise;
            }
            return Core.Winding.None;
        }

        private double SignedDoubleArea()
        {
            int index, nextIndex;
            int n = _vertices.Count;
            PointDouble point, next;
            double signedDoubleArea = 0;
            for (index = 0; index < n; ++index)
            {
                nextIndex = (index + 1) % n;
                point = _vertices[index];
                next = _vertices[nextIndex];
                signedDoubleArea += point.X * next.Y - next.X * point.Y;
            }
            return signedDoubleArea;
        }
    }
}