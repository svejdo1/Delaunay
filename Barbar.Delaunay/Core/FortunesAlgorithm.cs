/*
 * Java implementaition by Connor Clark (www.hotengames.com). Pretty much a 1:1 
 * translation of a wonderful map generating algorthim by Amit Patel of Red Blob Games,
 * which can be found here (http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/)
 * Hopefully it's of use to someone out there who needed it in Java like I did!
 * Note, the only island mode implemented is Radial. Implementing more is something for another day.
 * 
 * FORTUNE'S ALGORTIHIM
 * 
 * This is a java implementation of an AS3 (Flash) implementation of an algorthim
 * originally created in C++. Pretty much a 1:1 translation from as3 to java, save
 * for some necessary workarounds. Original as3 implementation by Alan Shaw (of nodename)
 * can be found here (https://github.com/nodename/as3delaunay). Original algorthim
 * by Steven Fortune (see lisence for c++ implementation below)
 * 
 * The author of this software is Steven Fortune.  Copyright (c) 1994 by AT&T
 * Bell Laboratories.
 * Permission to use, copy, modify, and distribute this software for any
 * purpose without fee is hereby granted, provided that this entire notice
 * is included in all copies of any software which is or includes a copy
 * or modification of this software and in all copies of the supporting
 * documentation for such software.
 * THIS SOFTWARE IS BEING PROVIDED "AS IS", WITHOUT ANY EXPRESS OR IMPLIED
 * WARRANTY.  IN PARTICULAR, NEITHER THE AUTHORS NOR AT&T MAKE ANY
 * REPRESENTATION OR WARRANTY OF ANY KIND CONCERNING THE MERCHANTABILITY
 * OF THIS SOFTWARE OR ITS FITNESS FOR ANY PARTICULAR PURPOSE.
 */

using System;
using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    public abstract class FortunesAlgorithm
    {
        protected Random _random = new Random();
        protected SiteList _sites;
        protected Dictionary<PointDouble, Site> _sitesIndexedByLocation;
        protected List<Triangle> _triangles;

        public List<Edge> Edges { get; protected set; }

        // TODO generalize this so it doesn't have to be a rectangle;
        // then we can make the fractal voronois-within-voronois
        public Rectangle PlotBounds { get; protected set; }


        public void dispose()
        {
            int i, n;
            if (_sites != null)
            {
                _sites.dispose();
                _sites = null;
            }
            if (_triangles != null)
            {
                n = _triangles.Count;
                for (i = 0; i < n; ++i)
                {
                    _triangles[i].dispose();
                }
                _triangles.Clear();
                _triangles = null;
            }
            if (Edges != null)
            {
                n = Edges.Count;
                for (i = 0; i < n; ++i)
                {
                    Edges[i].dispose();
                }
                Edges.Clear();
                Edges = null;
            }
            //_plotBounds = null;
            _sitesIndexedByLocation = null;
        }

        public List<PointDouble> GetRegion(PointDouble p)
        {
            Site site = _sitesIndexedByLocation[p];
            if (site == null)
            {
                return new List<PointDouble>();
            }
            return site.GetRegion(PlotBounds);
        }

        // TODO: bug: if you call this before you call region(), something goes wrong :(
        public List<PointDouble> NeighborSitesForSite(PointDouble coord)
        {
            var points = new List<PointDouble>();
            Site site = _sitesIndexedByLocation[coord];
            if (site == null)
            {
                return points;
            }
            List<Site> sites = site.GetNeighborSites();
            foreach (var neighbor in sites)
            {
                points.Add(neighbor.Coordinates);
            }
            return points;
        }

        public List<Circle> GetCircles()
        {
            return _sites.GetCircles();
        }

        private List<Edge> SelectEdgesForSitePoint(PointDouble coord, List<Edge> edgesToTest)
        {
            var filtered = new List<Edge>();

            foreach (var e in edgesToTest)
            {
                if (((e.LeftSite != null && e.LeftSite.Coordinates == coord)
                        || (e.RightSite != null && e.RightSite.Coordinates == coord)))
                {
                    filtered.Add(e);
                }
            }
            return filtered;
        }

        private List<LineSegment> VisibleLineSegments(List<Edge> edges)
        {
            var segments = new List<LineSegment>();

            foreach (var edge in edges)
            {
                if (edge.IsVisible)
                {
                    var p1 = edge.ClippedEnds[LR.Left];
                    var p2 = edge.ClippedEnds[LR.Right];
                    segments.Add(new LineSegment(p1, p2));
                }
            }

            return segments;
        }

        private List<LineSegment> DelaunayLinesForEdges(List<Edge> edges)
        {
            var segments = new List<LineSegment>();

            foreach (var edge in edges)
            {
                segments.Add(edge.DelaunayLine());
            }

            return segments;
        }

        public List<LineSegment> VoronoiBoundaryForSite(PointDouble coord)
        {
            return VisibleLineSegments(SelectEdgesForSitePoint(coord, Edges));
        }

        public List<LineSegment> DelaunayLinesForSite(PointDouble coord)
        {
            return DelaunayLinesForEdges(SelectEdgesForSitePoint(coord, Edges));
        }

        public List<LineSegment> VoronoiDiagram()
        {
            return VisibleLineSegments(Edges);
        }

        public List<LineSegment> GetHull()
        {
            return DelaunayLinesForEdges(GetHullEdges());
        }

        private List<Edge> GetHullEdges()
        {
            var filtered = new List<Edge>();

            foreach (var e in Edges)
            {
                if (e.IsPartOfConvexHull)
                {
                    filtered.Add(e);
                }
            }

            return filtered;
        }

        public List<PointDouble> GetHullPointsInOrder()
        {
            List<Edge> hullEdges = GetHullEdges();

            var points = new List<PointDouble>();
            if (hullEdges.Count == 0)
            {
                return points;
            }

            var reorderer = new EdgeReorderer(hullEdges, typeof(Site));
            hullEdges = reorderer.get_edges();
            var orientations = reorderer.get_edgeOrientations();
            reorderer.dispose();

            LR orientation;

            int n = hullEdges.Count;
            for (int i = 0; i < n; ++i)
            {
                Edge edge = hullEdges[i];
                orientation = orientations[i];
                points.Add(edge.GetSite(orientation).Coordinates);
            }
            return points;
        }

        public List<List<PointDouble>> GetRegions()
        {
            return _sites.GetRegions(PlotBounds);
        }

        public List<PointDouble> GetSiteCoordinates()
        {
            return _sites.GetSiteCoordinates();
        }

        protected void Execute()
        {
            Site newSite, bottomSite, topSite, tempSite;
            Vertex v, vertex;
            PointDouble newintstar = PointDouble.Empty;
            LR leftRight;
            Halfedge lbnd, rbnd, llbnd, rrbnd, bisector;
            Edge edge;

            Rectangle dataBounds = _sites.GetSitesBounds();

            int sqrt_nsites = (int)Math.Sqrt(_sites.Count + 4);
            HalfedgePriorityQueue heap = new HalfedgePriorityQueue(dataBounds.y, dataBounds.height, sqrt_nsites);
            EdgeList edgeList = new EdgeList(dataBounds.x, dataBounds.width, sqrt_nsites);
            var halfEdges = new List<Halfedge>();
            var vertices = new List<Vertex>();

            Site bottomMostSite = _sites.Next();
            newSite = _sites.Next();

            for (;;)
            {
                if (heap.IsEmpty() == false)
                {
                    newintstar = heap.Min();
                }

                if (newSite != null
                        && (heap.IsEmpty() || CompareByYThenX(newSite, newintstar) < 0))
                {
                    /* new site is smallest */
                    //trace("smallest: new site " + newSite);

                    // Step 8:
                    lbnd = edgeList.EdgeListLeftNeighbor(newSite.Coordinates);  // the Halfedge just to the left of newSite
                                                                                //trace("lbnd: " + lbnd);
                    rbnd = lbnd.edgeListRightNeighbor;      // the Halfedge just to the right
                                                            //trace("rbnd: " + rbnd);
                    bottomSite = RightRegion(lbnd, bottomMostSite);     // this is the same as leftRegion(rbnd)
                                                                        // this Site determines the region containing the new site
                                                                        //trace("new Site is in region of existing site: " + bottomSite);

                    // Step 9:
                    edge = Edge.CreateBisectingEdge(bottomSite, newSite);
                    //trace("new edge: " + edge);
                    Edges.Add(edge);

                    bisector = Halfedge.Create(edge, LR.Left);
                    halfEdges.Add(bisector);
                    // inserting two Halfedges into edgeList constitutes Step 10:
                    // insert bisector to the right of lbnd:
                    edgeList.Insert(lbnd, bisector);

                    // first half of Step 11:
                    if ((vertex = Vertex.Intersect(lbnd, bisector)) != null)
                    {
                        vertices.Add(vertex);
                        heap.Remove(lbnd);
                        lbnd.vertex = vertex;
                        lbnd.ystar = vertex.Y + newSite.Distance(vertex);
                        heap.Insert(lbnd);
                    }

                    lbnd = bisector;
                    bisector = Halfedge.Create(edge, LR.Right);
                    halfEdges.Add(bisector);
                    // second Halfedge for Step 10:
                    // insert bisector to the right of lbnd:
                    edgeList.Insert(lbnd, bisector);

                    // second half of Step 11:
                    if ((vertex = Vertex.Intersect(bisector, rbnd)) != null)
                    {
                        vertices.Add(vertex);
                        bisector.vertex = vertex;
                        bisector.ystar = vertex.Y + newSite.Distance(vertex);
                        heap.Insert(bisector);
                    }

                    newSite = _sites.Next();
                }
                else if (heap.IsEmpty() == false)
                {
                    /* intersection is smallest */
                    lbnd = heap.ExtractMin();
                    llbnd = lbnd.edgeListLeftNeighbor;
                    rbnd = lbnd.edgeListRightNeighbor;
                    rrbnd = rbnd.edgeListRightNeighbor;
                    bottomSite = LeftRegion(lbnd, bottomMostSite);
                    topSite = RightRegion(rbnd, bottomMostSite);
                    // these three sites define a Delaunay triangle
                    // (not actually using these for anything...)
                    //_triangles.push(new Triangle(bottomSite, topSite, rightRegion(lbnd)));

                    v = lbnd.vertex;
                    v.SetIndex();
                    lbnd.edge.SetVertex(lbnd.leftRight, v);
                    rbnd.edge.SetVertex(rbnd.leftRight, v);
                    edgeList.Remove(lbnd);
                    heap.Remove(rbnd);
                    edgeList.Remove(rbnd);
                    leftRight = LR.Left;
                    if (bottomSite.Y > topSite.Y)
                    {
                        tempSite = bottomSite;
                        bottomSite = topSite;
                        topSite = tempSite;
                        leftRight = LR.Right;
                    }
                    edge = Edge.CreateBisectingEdge(bottomSite, topSite);
                    Edges.Add(edge);
                    bisector = Halfedge.Create(edge, leftRight);
                    halfEdges.Add(bisector);
                    edgeList.Insert(llbnd, bisector);
                    edge.SetVertex(leftRight.other(), v);
                    if ((vertex = Vertex.Intersect(llbnd, bisector)) != null)
                    {
                        vertices.Add(vertex);
                        heap.Remove(llbnd);
                        llbnd.vertex = vertex;
                        llbnd.ystar = vertex.Y + bottomSite.Distance(vertex);
                        heap.Insert(llbnd);
                    }
                    if ((vertex = Vertex.Intersect(bisector, rrbnd)) != null)
                    {
                        vertices.Add(vertex);
                        bisector.vertex = vertex;
                        bisector.ystar = vertex.Y + bottomSite.Distance(vertex);
                        heap.Insert(bisector);
                    }
                }
                else
                {
                    break;
                }
            }

            // heap should be empty now
            heap.dispose();
            edgeList.dispose();

            foreach (var halfEdge in halfEdges)
            {
                halfEdge.ReallyDispose();
            }
            halfEdges.Clear();

            // we need the vertices to clip the edges
            foreach (var e in Edges)
            {
                e.ClipVertices(PlotBounds);
            }
            // but we don't actually ever use them again!
            foreach (Vertex v0 in vertices)
            {
                v0.dispose();
            }
            vertices.Clear();
        }

        Site LeftRegion(Halfedge he, Site bottomMostSite)
        {
            Edge edge = he.edge;
            if (edge == null)
            {
                return bottomMostSite;
            }
            return edge.GetSite(he.leftRight);
        }

        Site RightRegion(Halfedge he, Site bottomMostSite)
        {
            Edge edge = he.edge;
            if (edge == null)
            {
                return bottomMostSite;
            }
            return edge.GetSite(he.leftRight.other());
        }

        public static int CompareByYThenX(Site s1, Site s2)
        {
            if (s1.Y < s2.Y)
            {
                return -1;
            }
            if (s1.Y > s2.Y)
            {
                return 1;
            }
            if (s1.X < s2.X)
            {
                return -1;
            }
            if (s1.X > s2.X)
            {
                return 1;
            }
            return 0;
        }

        public static int CompareByYThenX(Site s1, PointDouble s2)
        {
            if (s1.Y < s2.Y)
            {
                return -1;
            }
            if (s1.Y > s2.Y)
            {
                return 1;
            }
            if (s1.X < s2.X)
            {
                return -1;
            }
            if (s1.X > s2.X)
            {
                return 1;
            }
            return 0;
        }
    }

    public sealed class FortunesAlgorithm<TColor> : FortunesAlgorithm
    {
        public FortunesAlgorithm(List<PointDouble> points, List<TColor> colors, Rectangle plotBounds)
        {
            Init(points, colors, plotBounds);
            Execute();
        }

        public FortunesAlgorithm(List<PointDouble> points, List<TColor> colors)
        {
            double maxWidth = 0, maxHeight = 0;
            foreach (var p in points)
            {
                maxWidth = Math.Max(maxWidth, p.X);
                maxHeight = Math.Max(maxHeight, p.Y);
            }
            Console.Out.WriteLine(maxWidth + "," + maxHeight);
            Init(points, colors, new Rectangle(0, 0, maxWidth, maxHeight));
            Execute();
        }

        public FortunesAlgorithm(int numSites, double maxWidth, double maxHeight, Random r, List<TColor> colors)
        {
            var points = new List<PointDouble>();
            for (int i = 0; i < numSites; i++)
            {
                points.Add(new PointDouble(r.NextDouble() * maxWidth, r.NextDouble() * maxHeight));
            }
            Init(points, colors, new Rectangle(0, 0, maxWidth, maxHeight));
            Execute();
        }

        private void Init(List<PointDouble> points, List<TColor> colors, Rectangle plotBounds)
        {
            _sites = new SiteList();
            _sitesIndexedByLocation = new Dictionary<PointDouble, Site>();
            AddSites(points, colors);
            PlotBounds = plotBounds;
            _triangles = new List<Triangle>();
            Edges = new List<Edge>();
        }

        private void AddSites(List<PointDouble> points, List<TColor> colors)
        {
            int length = points.Count;
            for (int i = 0; i < length; ++i)
            {
                AddSite(points[i], colors != null ? colors[i] : default(TColor), i);
            }
        }

        private void AddSite(PointDouble p, TColor color, int index)
        {
            double weight = _random.Next(100);
            Site site = Site<TColor>.Create(p, index, weight, color);
            _sites.Push(site);
            _sitesIndexedByLocation[p] = site;
        }
    }
}