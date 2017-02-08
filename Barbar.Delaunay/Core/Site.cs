using System.Collections.Generic;
using System;
using Barbar.Delaunay.Drawing;

namespace Barbar.Delaunay.Core
{
    public class Site : ICoordinates
    {
        protected PointDouble _coordinates;
        protected int _siteIndex;
        // ordered list of points that define the region clipped to bounds:
        protected List<PointDouble> _region;

        public double weight;
        // the edges that define this Site's Voronoi region:
        public List<Edge> _edges;
        // which end of each edge hooks up with the previous edge in _edges:
        private List<LR> _edgeOrientations;

        private const double EPSILON = .005;

        public PointDouble Coordinates
        {
            get { return _coordinates; }
        }

        public override string ToString()
        {
            return string.Format("Site {0}: {1}", _siteIndex, Coordinates);
        }

        private void Move(PointDouble p)
        {
            Clear();
            _coordinates = p;
        }

        private void Clear()
        {
            if (_edges != null)
            {
                _edges.Clear();
                _edges = null;
            }
            if (_edgeOrientations != null)
            {
                _edgeOrientations.Clear();
                _edgeOrientations = null;
            }
            if (_region != null)
            {
                _region.Clear();
                _region = null;
            }
        }

        public void AddEdge(Edge edge)
        {
            _edges.Add(edge);
        }

        public Edge NearestEdge()
        {
            _edges.Sort(new Comparison<Edge>((x, y) =>
            {
                return (int)Edge.CompareSitesDistances(x, y);
            }));
            return _edges[0];
        }

        public virtual void dispose()
        {
            _coordinates = PointDouble.Empty;
            Clear();
        }

        public List<Site> GetNeighborSites()
        {
            if (_edges == null || _edges.Count == 0)
            {
                return new List<Site>();
            }
            if (_edgeOrientations == null)
            {
                ReorderEdges();
            }
            var list = new List<Site>();
            foreach (var edge in _edges)
            {
                list.Add(GetNeighborSite(edge));
            }
            return list;
        }

        private Site GetNeighborSite(Edge edge)
        {
            if (this == edge.LeftSite)
            {
                return edge.RightSite;
            }
            if (this == edge.RightSite)
            {
                return edge.LeftSite;
            }
            return null;
        }

        public List<PointDouble> GetRegion(Rectangle clippingBounds)
        {
            if (_edges == null || _edges.Count == 0)
            {
                return new List<PointDouble>();
            }
            if (_edgeOrientations == null)
            {
                ReorderEdges();
                _region = ClipToBounds(clippingBounds);
                if ((new Polygon(_region)).Winding() == Winding.Clockwise)
                {
                    _region.Reverse();
                }
            }
            return _region;
        }

        private void ReorderEdges()
        {
            EdgeReorderer reorderer = new EdgeReorderer(_edges, typeof(Vertex));
            _edges = reorderer.get_edges();
            _edgeOrientations = reorderer.get_edgeOrientations();
            reorderer.dispose();
        }

        private List<PointDouble> ClipToBounds(Rectangle bounds)
        {
            var points = new List<PointDouble>();
            int n = _edges.Count;
            int i = 0;
            Edge edge;
            while (i < n && (!_edges[i].IsVisible))
            {
                ++i;
            }

            if (i == n)
            {
                // no edges visible
                return new List<PointDouble>();
            }
            edge = _edges[i];
            LR orientation = _edgeOrientations[i];
            points.Add(edge.ClippedEnds[orientation]);
            points.Add(edge.ClippedEnds[orientation.other()]);

            for (int j = i + 1; j < n; ++j)
            {
                edge = _edges[j];
                if (!edge.IsVisible)
                {
                    continue;
                }
                Connect(points, j, bounds, false);
            }
            // close up the polygon by adding another corner point of the bounds if needed:
            Connect(points, i, bounds, true);

            return points;
        }

        private void Connect(List<PointDouble> points, int j, Rectangle bounds, bool closingUp)
        {
            PointDouble rightPoint = points[points.Count - 1];
            Edge newEdge = _edges[j];
            LR newOrientation = _edgeOrientations[j];
            // the point that  must be connected to rightPoint:
            PointDouble newPoint = newEdge.ClippedEnds[newOrientation];
            if (!CloseEnough(rightPoint, newPoint))
            {
                // The points do not coincide, so they must have been clipped at the bounds;
                // see if they are on the same border of the bounds:
                if (rightPoint.X != newPoint.X
                        && rightPoint.Y != newPoint.Y)
                {
                    // They are on different borders of the bounds;
                    // insert one or two corners of bounds as needed to hook them up:
                    // (NOTE this will not be correct if the region should take up more than
                    // half of the bounds rect, for then we will have gone the wrong way
                    // around the bounds and included the smaller part rather than the larger)
                    int rightCheck = BoundsCheck.Check(rightPoint, bounds);
                    int newCheck = BoundsCheck.Check(newPoint, bounds);
                    double px, py;
                    if ((rightCheck & BoundsCheck.RIGHT) != 0)
                    {
                        px = bounds.right;
                        if ((newCheck & BoundsCheck.BOTTOM) != 0)
                        {
                            py = bounds.bottom;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.TOP) != 0)
                        {
                            py = bounds.top;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.LEFT) != 0)
                        {
                            if (rightPoint.Y - bounds.y + newPoint.Y - bounds.y < bounds.height)
                            {
                                py = bounds.top;
                            }
                            else
                            {
                                py = bounds.bottom;
                            }
                            points.Add(new PointDouble(px, py));
                            points.Add(new PointDouble(bounds.left, py));
                        }
                    }
                    else if ((rightCheck & BoundsCheck.LEFT) != 0)
                    {
                        px = bounds.left;
                        if ((newCheck & BoundsCheck.BOTTOM) != 0)
                        {
                            py = bounds.bottom;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.TOP) != 0)
                        {
                            py = bounds.top;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.RIGHT) != 0)
                        {
                            if (rightPoint.Y - bounds.y + newPoint.Y - bounds.y < bounds.height)
                            {
                                py = bounds.top;
                            }
                            else
                            {
                                py = bounds.bottom;
                            }
                            points.Add(new PointDouble(px, py));
                            points.Add(new PointDouble(bounds.right, py));
                        }
                    }
                    else if ((rightCheck & BoundsCheck.TOP) != 0)
                    {
                        py = bounds.top;
                        if ((newCheck & BoundsCheck.RIGHT) != 0)
                        {
                            px = bounds.right;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.LEFT) != 0)
                        {
                            px = bounds.left;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.BOTTOM) != 0)
                        {
                            if (rightPoint.X - bounds.x + newPoint.X - bounds.x < bounds.width)
                            {
                                px = bounds.left;
                            }
                            else
                            {
                                px = bounds.right;
                            }
                            points.Add(new PointDouble(px, py));
                            points.Add(new PointDouble(px, bounds.bottom));
                        }
                    }
                    else if ((rightCheck & BoundsCheck.BOTTOM) != 0)
                    {
                        py = bounds.bottom;
                        if ((newCheck & BoundsCheck.RIGHT) != 0)
                        {
                            px = bounds.right;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.LEFT) != 0)
                        {
                            px = bounds.left;
                            points.Add(new PointDouble(px, py));
                        }
                        else if ((newCheck & BoundsCheck.TOP) != 0)
                        {
                            if (rightPoint.X - bounds.x + newPoint.X - bounds.x < bounds.width)
                            {
                                px = bounds.left;
                            }
                            else
                            {
                                px = bounds.right;
                            }
                            points.Add(new PointDouble(px, py));
                            points.Add(new PointDouble(px, bounds.top));
                        }
                    }
                }
                if (closingUp)
                {
                    // newEdge's ends have already been added
                    return;
                }
                points.Add(newPoint);
            }
            var newRightPoint = newEdge.ClippedEnds[newOrientation.other()];
            if (!CloseEnough(points[0], newRightPoint))
            {
                points.Add(newRightPoint);
            }
        }

        public double X
        {
            get { return _coordinates.X; }
        }

        public double Y
        {
            get { return _coordinates.Y; }
        }

        public double Distance(ICoordinates p)
        {
            return PointDouble.Distance(p.Coordinates, _coordinates);
        }

        public static void SortSites(List<Site> sites)
        {
            sites.Sort(new Comparison<Site>((x, y) =>
            {
                return (int)Site.Compare(x, y);
            }));
        }

        /// <summary>
        /// sort sites on y, then x, coord also change each site's _siteIndex to
        /// match its new position in the list so the _siteIndex can be used to
        /// identify the site for nearest-neighbor queries
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private static double Compare(Site s1, Site s2)
        {
            int returnValue = FortunesAlgorithm.CompareByYThenX(s1, s2);

            // swap _siteIndex values if necessary to match new ordering:
            int tempIndex;
            if (returnValue == -1)
            {
                if (s1._siteIndex > s2._siteIndex)
                {
                    tempIndex = s1._siteIndex;
                    s1._siteIndex = s2._siteIndex;
                    s2._siteIndex = tempIndex;
                }
            }
            else if (returnValue == 1)
            {
                if (s2._siteIndex > s1._siteIndex)
                {
                    tempIndex = s2._siteIndex;
                    s2._siteIndex = s1._siteIndex;
                    s1._siteIndex = tempIndex;
                }

            }

            return returnValue;
        }


        private static bool CloseEnough(PointDouble p0, PointDouble p1)
        {
            return PointDouble.Distance(p0, p1) < EPSILON;
        }
    }

    public sealed class Site<TColor> : Site
    {
        public TColor color;

        private static Stack<Site<TColor>> _pool = new Stack<Site<TColor>>();

        public Site(PointDouble p, int index, double weight, TColor color)
        {
            Init(p, index, weight, color);
        }

        private Site<TColor> Init(PointDouble p, int index, double weight, TColor color)
        {
            _coordinates = p;
            _siteIndex = index;
            this.weight = weight;
            this.color = color;
            _edges = new List<Edge>();
            _region = null;
            return this;
        }

        public override void dispose()
        {
            _pool.Push(this);
        }

        public static Site<TColor> Create(PointDouble p, int index, double weight, TColor color)
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop().Init(p, index, weight, color);
            }
            else
            {
                return new Site<TColor>(p, index, weight, color);
            }
        }
    }
}