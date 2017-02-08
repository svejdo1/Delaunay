using System;
using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    /// <summary>
    /// The line segment connecting the two Sites is part of the Delaunay
    /// triangulation; the line segment connecting the two Vertices is part of the
    /// Voronoi diagram
    /// </summary>
    public sealed class Edge
    {
        // the equation of the edge: ax + by = c
        public double a, b, c;

        // Once clipVertices() is called, this Dictionary will hold two Points
        // representing the clipped coordinates of the left and right ends...
        private Dictionary<LR, PointDouble> _clippedVertices;

        // the two input Sites for which this Edge is a bisector:
        private Dictionary<LR, Site> _sites;

        private int _edgeIndex;

        private static readonly Stack<Edge> _pool = new Stack<Edge>();
        private static int _nedges = 0;
        public static readonly Edge DELETED = new Edge();

        // the two Voronoi vertices that the edge connects
        //		(if one of them is null, the edge extends to infinity)
        public Vertex LeftVertex { get; private set; }
        public Vertex RightVertex { get; private set; }
        
        public LineSegment DelaunayLine()
        {
            // draw a line connecting the input Sites for which the edge is a bisector:
            return new LineSegment(LeftSite.Coordinates, RightSite.Coordinates);
        }

        public LineSegment VoronoiEdge()
        {
            if (!IsVisible)
            {
                return new LineSegment(PointDouble.Empty, PointDouble.Empty);
            }
            return new LineSegment(_clippedVertices[LR.Left], _clippedVertices[LR.Right]);
        }

        public Vertex GetVertex(LR leftRight)
        {
            return leftRight == LR.Left ? LeftVertex : RightVertex;
        }

        public void SetVertex(LR leftRight, Vertex v)
        {
            if (leftRight == LR.Left)
            {
                LeftVertex = v;
            }
            else
            {
                RightVertex = v;
            }
        }

        public bool IsPartOfConvexHull
        {
            get { return LeftVertex == null || RightVertex == null; }
        }

        public double SitesDistance
        {
            get { return PointDouble.Distance(LeftSite.Coordinates, RightSite.Coordinates); }
        }

        public Dictionary<LR, PointDouble> ClippedEnds
        {
            get { return _clippedVertices; }
        }

        public bool IsVisible
        {
            get { return _clippedVertices != null; }
        }

        public Site LeftSite
        {
            get { return _sites[LR.Left]; }
            set { _sites[LR.Left] = value; }
        }

        public Site RightSite
        {
            get { return _sites[LR.Right]; }
            set { _sites[LR.Right] = value; }
        }

        public Site GetSite(LR leftRight)
        {
            return _sites[leftRight];
        }

        public void dispose()
        {
            LeftVertex = null;
            RightVertex = null;
            if (_clippedVertices != null)
            {
                _clippedVertices.Clear();
                _clippedVertices = null;
            }
            _sites.Clear();
            _sites = null;

            _pool.Push(this);
        }

        private Edge()
        {
            _edgeIndex = _nedges++;
            Init();
        }

        private void Init()
        {
            _sites = new Dictionary<LR, Site>();
        }

        public override string ToString()
        {
            return "Edge " + _edgeIndex + "; sites " + _sites[LR.Left] + ", " + _sites[LR.Right]
                    + "; endVertices " + (LeftVertex != null ? Convert.ToString(LeftVertex.VertexIndex) : "null") + ", "
                    + (RightVertex != null ? Convert.ToString(RightVertex.VertexIndex) : "null") + "::";
        }

        /// <summary>
        /// Set _clippedVertices to contain the two ends of the portion of the
        /// Voronoi edge that is visible within the bounds. If no part of the Edge
        /// falls within the bounds, leave _clippedVertices null.
        /// </summary>
        /// <param name="bounds"></param>
        public void ClipVertices(Rectangle bounds)
        {
            double xmin = bounds.x;
            double ymin = bounds.y;
            double xmax = bounds.right;
            double ymax = bounds.bottom;

            Vertex vertex0, vertex1;
            double x0, x1, y0, y1;

            if (a == 1.0 && b >= 0.0)
            {
                vertex0 = RightVertex;
                vertex1 = LeftVertex;
            }
            else
            {
                vertex0 = LeftVertex;
                vertex1 = RightVertex;
            }

            if (a == 1.0)
            {
                y0 = ymin;
                if (vertex0 != null && vertex0.Y > ymin)
                {
                    y0 = vertex0.Y;
                }
                if (y0 > ymax)
                {
                    return;
                }
                x0 = c - b * y0;

                y1 = ymax;
                if (vertex1 != null && vertex1.Y < ymax)
                {
                    y1 = vertex1.Y;
                }
                if (y1 < ymin)
                {
                    return;
                }
                x1 = c - b * y1;

                if ((x0 > xmax && x1 > xmax) || (x0 < xmin && x1 < xmin))
                {
                    return;
                }

                if (x0 > xmax)
                {
                    x0 = xmax;
                    y0 = (c - x0) / b;
                }
                else if (x0 < xmin)
                {
                    x0 = xmin;
                    y0 = (c - x0) / b;
                }

                if (x1 > xmax)
                {
                    x1 = xmax;
                    y1 = (c - x1) / b;
                }
                else if (x1 < xmin)
                {
                    x1 = xmin;
                    y1 = (c - x1) / b;
                }
            }
            else
            {
                x0 = xmin;
                if (vertex0 != null && vertex0.X > xmin)
                {
                    x0 = vertex0.X;
                }
                if (x0 > xmax)
                {
                    return;
                }
                y0 = c - a * x0;

                x1 = xmax;
                if (vertex1 != null && vertex1.X < xmax)
                {
                    x1 = vertex1.X;
                }
                if (x1 < xmin)
                {
                    return;
                }
                y1 = c - a * x1;

                if ((y0 > ymax && y1 > ymax) || (y0 < ymin && y1 < ymin))
                {
                    return;
                }

                if (y0 > ymax)
                {
                    y0 = ymax;
                    x0 = (c - y0) / a;
                }
                else if (y0 < ymin)
                {
                    y0 = ymin;
                    x0 = (c - y0) / a;
                }

                if (y1 > ymax)
                {
                    y1 = ymax;
                    x1 = (c - y1) / a;
                }
                else if (y1 < ymin)
                {
                    y1 = ymin;
                    x1 = (c - y1) / a;
                }
            }

            _clippedVertices = new Dictionary<LR, PointDouble>();
            if (vertex0 == LeftVertex)
            {
                _clippedVertices[LR.Left] = new PointDouble(x0, y0);
                _clippedVertices[LR.Right] = new PointDouble(x1, y1);
            }
            else
            {
                _clippedVertices[LR.Right] = new PointDouble(x0, y0);
                _clippedVertices[LR.Left] = new PointDouble(x1, y1);
            }
        }

        /// <summary>
        /// This is the only way to create a new Edge
        /// </summary>
        /// <param name="site0"></param>
        /// <param name="site1"></param>
        /// <returns></returns>
        public static Edge CreateBisectingEdge(Site site0, Site site1)
        {
            double dx, dy, absdx, absdy;
            double a, b, c;

            dx = site1.X - site0.X;
            dy = site1.Y - site0.Y;
            absdx = dx > 0 ? dx : -dx;
            absdy = dy > 0 ? dy : -dy;
            c = site0.X * dx + site0.Y * dy + (dx * dx + dy * dy) * 0.5;
            if (absdx > absdy)
            {
                a = 1.0;
                b = dy / dx;
                c /= dx;
            }
            else
            {
                b = 1.0;
                a = dx / dy;
                c /= dy;
            }

            var edge = Create();

            edge.LeftSite = site0;
            edge.RightSite = site1;
            site0.AddEdge(edge);
            site1.AddEdge(edge);

            edge.LeftVertex = null;
            edge.RightVertex = null;

            edge.a = a;
            edge.b = b;
            edge.c = c;

            return edge;
        }

        private static Edge Create()
        {
            Edge edge;
            if (_pool.Count > 0)
            {
                edge = _pool.Pop();
                edge.Init();
            }
            else
            {
                edge = new Edge();
            }
            return edge;
        }

        public static double CompareSitesDistancesMax(Edge edge0, Edge edge1)
        {
            double length0 = edge0.SitesDistance;
            double length1 = edge1.SitesDistance;
            if (length0 < length1)
            {
                return 1;
            }
            if (length0 > length1)
            {
                return -1;
            }
            return 0;
        }

        public static double CompareSitesDistances(Edge edge0, Edge edge1)
        {
            return -CompareSitesDistancesMax(edge0, edge1);
        }
    }
}