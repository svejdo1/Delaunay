using System;
using System.Linq;
using System.Collections.Generic;
using Barbar.Delaunay.Core;
using Barbar.Delaunay.Drawing;

namespace Barbar.Delaunay.Voronoi
{
    public abstract class VoronoiGraph
    {
        public readonly List<Edge> edges = new List<Edge>();
        public readonly List<Corner> corners = new List<Corner>();
        public readonly List<Center> centers = new List<Center>();
        public readonly Rectangle bounds;
        private readonly Random r;
        protected PortableColor Ocean, River, Lake, Beach;
        public readonly IImage pixelCenterMap;

        double[][] noise;
        double ISLAND_FACTOR = 1.07;  // 1.0 means no small islands; 2.0 leads to a lot
        int bumps;
        double startAngle;
        double dipAngle;
        double dipWidth;

        public VoronoiGraph(FortunesAlgorithm<PortableColor> v, int numLloydRelaxations, Random r)
        {
            this.r = r;
            bumps = r.Next(5) + 1;
            startAngle = r.NextDouble() * 2 * Math.PI;
            dipAngle = r.NextDouble() * 2 * Math.PI;
            dipWidth = r.NextDouble() * .5 + .2;
            bounds = v.PlotBounds;
            for (int i = 0; i < numLloydRelaxations; i++)
            {
                var points = v.GetSiteCoordinates();
                for (var j = 0; j < points.Count; j++)
                {
                    var region = v.GetRegion(points[j]);
                    double x = 0;
                    double y = 0;
                    foreach (var c in region)
                    {
                        x += c.X;
                        y += c.Y;
                    }
                    x /= region.Count;
                    y /= region.Count;
                    points[j] = new PointDouble(x, y);
                }
                v = new FortunesAlgorithm<PortableColor>(points, null, v.PlotBounds);
            }
            BuildGraph(v);
            ImproveCorners();

            AssignCornerElevations();
            AssignOceanCoastAndLand();
            RedistributeElevations(LandCorners());
            AssignPolygonElevations();

            CalculateDownslopes();
            AssignNormals();
            //calculateWatersheds();
            CreateRivers();
            AssignCornerMoisture();
            RedistributeMoisture(LandCorners());
            AssignPolygonMoisture();
            AssignBiomes();

            if (DrawingHook.ImageFactory != null)
            {
                pixelCenterMap = DrawingHook.ImageFactory.CreateBitmap32bppArgb((int)bounds.width, (int)bounds.width);
            }
        }

        abstract protected object GetBiome(Center p);

        abstract protected PortableColor GetColor(object biome);

        private void ImproveCorners()
        {
            var newP = new PointDouble[corners.Count];
            foreach (var c in corners)
            {
                if (c.border)
                {
                    newP[c.index] = c.loc;
                }
                else
                {
                    double x = 0;
                    double y = 0;
                    foreach (var center in c.touches)
                    {
                        x += center.loc.X;
                        y += center.loc.Y;
                    }
                    newP[c.index] = new PointDouble(x / c.touches.Count, y / c.touches.Count);
                }
            }
            corners.ForEach((c) => c.loc = newP[c.index]);
            foreach (var e in edges.Where((e) => (e.v0 != null && e.v1 != null)))
            {
                e.SetVornoi(e.v0, e.v1);
            }
        }

        private Edge EdgeWithCenters(Center c1, Center c2)
        {
            foreach (var e in c1.borders)
            {
                if (e.d0 == c2 || e.d1 == c2)
                {
                    return e;
                }
            }
            return null;
        }

        private void DrawTriangle(IGraphics g, PortableColor c, Corner c1, Corner c2, Center center)
        {
            var points = new PointInt32[]
            {
                new PointInt32((int)center.loc.X, (int)center.loc.Y),
                new PointInt32((int)c1.loc.X, (int)c1.loc.Y),
                new PointInt32((int)c2.loc.X, (int)c2.loc.Y),
            };
            g.DrawPolygon(c, points);
        }

        private bool CloseEnough(double d1, double d2, double diff)
        {
            return Math.Abs(d1 - d2) <= diff;
        }

        public IImage CreateMap()
        {
            int size = (int)bounds.width;

            var img = DrawingHook.ImageFactory.CreateBitmap32bppArgb(size, size);
            using (var g = img.GetGraphics())
            {
                Paint(g);
            }

            return img;
        }

        public void Paint(IGraphics g)
        {
            Paint(g, true, true, false, false, false);
        }

        public IList<T> Paint3D<T>(IVertexFactory<T> factory)
        {
            var result = new List<T>(centers.Count * 3);
            //draw via triangles
            foreach (var c in centers)
            {
                DrawPolygon3D(c, GetColor(c.biome), factory, result);
            }
            return result;
        }

        private Vector3D GetNormal(Vector3D p, Vector3D q, Vector3D r)
        {
            return new Vector3D(0, -1, 0);
            var result = ((q - p).CrossProduct(r - p)).Normalize();
            if (result.Dot(result) < 0)
            {
                return ((q - r).CrossProduct(p - r)).Normalize();
            }
            return result;
        }

        private void DrawPolygon3D<T>(Center c, PortableColor color, IVertexFactory<T> factory, List<T> vertexBuffer)
        {
            //only used if Center c is on the edge of the graph. allows for completely filling in the outer polygons
            Corner edgeCorner1 = null;
            Corner edgeCorner2 = null;
            foreach (Center n in c.neighbors)
            {
                
                var e = EdgeWithCenters(c, n);

                if (e.v0 == null)
                {
                    //outermost voronoi edges aren't stored in the graph
                    continue;
                }

                //find a corner on the exterior of the graph
                //if this Edge e has one, then it must have two,
                //finding these two corners will give us the missing
                //triangle to render. this special triangle is handled
                //outside this for loop
                var cornerWithOneAdjacent = e.v0.border ? e.v0 : e.v1;
                if (cornerWithOneAdjacent.border)
                {
                    if (edgeCorner1 == null)
                    {
                        edgeCorner1 = cornerWithOneAdjacent;
                    }
                    else
                    {
                        edgeCorner2 = cornerWithOneAdjacent;
                    }
                }
                var p = Transform((float)c.loc.X, (float)c.loc.Y, (float)c.elevation);
                var q = Transform((float)e.v0.loc.X, (float)e.v0.loc.Y, (float)e.v0.elevation);
                var r = Transform((float)e.v1.loc.X, (float)e.v1.loc.Y, (float)e.v1.elevation);
                var normal = GetNormal(p, q, r);


                vertexBuffer.Add(factory.CreateVertex(p, normal, color));
                vertexBuffer.Add(factory.CreateVertex(q, normal, color));
                vertexBuffer.Add(factory.CreateVertex(r, normal, color));
            }

            //handle the missing triangle
            if (edgeCorner2 != null)
            {
                //if these two outer corners are NOT on the same exterior edge of the graph,
                //then we actually must render a polygon (w/ 4 points) and take into consideration
                //one of the four corners (either 0,0 or 0,height or width,0 or width,height)
                //note: the 'missing polygon' may have more than just 4 points. this
                //is common when the number of sites are quite low (less than 5), but not a problem
                //with a more useful number of sites. 
                //TODO: find a way to fix this

                if (CloseEnough(edgeCorner1.loc.X, edgeCorner2.loc.X, 1))
                {
                    var p = Transform((float)c.loc.X, (float)c.loc.Y, (float)c.elevation);
                    var q = Transform((float)edgeCorner1.loc.X, (float)edgeCorner1.loc.Y, (float)edgeCorner1.elevation);
                    var r = Transform((float)edgeCorner2.loc.X, (float)edgeCorner2.loc.Y, (float)edgeCorner2.elevation);
                    var normal = GetNormal(p, q, r);

                    vertexBuffer.Add(factory.CreateVertex(p, normal, color));
                    vertexBuffer.Add(factory.CreateVertex(q, normal, color));
                    vertexBuffer.Add(factory.CreateVertex(r, normal, color));
                }
                else
                {
                    /*
                    var points = new PointInt32[] {
                        new PointInt32((int)c.loc.X, (int)c.loc.Y),
                        new PointInt32((int)edgeCorner1.loc.X, (int)edgeCorner1.loc.Y),
                        new PointInt32((int)((CloseEnough(edgeCorner1.loc.X, bounds.x, 1) || CloseEnough(edgeCorner2.loc.X, bounds.x, .5)) ? bounds.x : bounds.right), (int)((CloseEnough(edgeCorner1.loc.Y, bounds.y, 1) || CloseEnough(edgeCorner2.loc.Y, bounds.y, .5)) ? bounds.y : bounds.bottom)),
                        new PointInt32((int)edgeCorner2.loc.X, (int)edgeCorner2.loc.Y),
                    };

                    g.FillPolygon(color, points);
                    c.area += 0; //TODO: area of polygon given vertices*/
                }
            }
        }

        private void DrawPolygon(IGraphics g, Center c, PortableColor color)
        {
            //only used if Center c is on the edge of the graph. allows for completely filling in the outer polygons
            Corner edgeCorner1 = null;
            Corner edgeCorner2 = null;
            c.area = 0;
            foreach (Center n in c.neighbors)
            {
                Edge e = EdgeWithCenters(c, n);

                if (e.v0 == null)
                {
                    //outermost voronoi edges aren't stored in the graph
                    continue;
                }

                //find a corner on the exterior of the graph
                //if this Edge e has one, then it must have two,
                //finding these two corners will give us the missing
                //triangle to render. this special triangle is handled
                //outside this for loop
                Corner cornerWithOneAdjacent = e.v0.border ? e.v0 : e.v1;
                if (cornerWithOneAdjacent.border)
                {
                    if (edgeCorner1 == null)
                    {
                        edgeCorner1 = cornerWithOneAdjacent;
                    }
                    else
                    {
                        edgeCorner2 = cornerWithOneAdjacent;
                    }
                }

                DrawTriangle(g, color, e.v0, e.v1, c);
                c.area += Math.Abs(c.loc.X * (e.v0.loc.Y - e.v1.loc.Y)
                        + e.v0.loc.X * (e.v1.loc.Y - c.loc.Y)
                        + e.v1.loc.X * (c.loc.Y - e.v0.loc.Y)) / 2;
            }

            //handle the missing triangle
            if (edgeCorner2 != null)
            {
                //if these two outer corners are NOT on the same exterior edge of the graph,
                //then we actually must render a polygon (w/ 4 points) and take into consideration
                //one of the four corners (either 0,0 or 0,height or width,0 or width,height)
                //note: the 'missing polygon' may have more than just 4 points. this
                //is common when the number of sites are quite low (less than 5), but not a problem
                //with a more useful number of sites. 
                //TODO: find a way to fix this

                if (CloseEnough(edgeCorner1.loc.X, edgeCorner2.loc.X, 1))
                {
                    DrawTriangle(g, color, edgeCorner1, edgeCorner2, c);
                }
                else
                {
                    var points = new PointInt32[] {
                        new PointInt32((int)c.loc.X, (int)c.loc.Y),
                        new PointInt32((int)edgeCorner1.loc.X, (int)edgeCorner1.loc.Y),
                        new PointInt32((int)((CloseEnough(edgeCorner1.loc.X, bounds.x, 1) || CloseEnough(edgeCorner2.loc.X, bounds.x, .5)) ? bounds.x : bounds.right), (int)((CloseEnough(edgeCorner1.loc.Y, bounds.y, 1) || CloseEnough(edgeCorner2.loc.Y, bounds.y, .5)) ? bounds.y : bounds.bottom)),
                        new PointInt32((int)edgeCorner2.loc.X, (int)edgeCorner2.loc.Y),
                    };

                    g.FillPolygon(color, points);
                    c.area += 0; //TODO: area of polygon given vertices
                }
            }
        }

        //also records the area of each voronoi cell
        public void Paint(IGraphics g, bool drawBiomes, bool drawRivers, bool drawSites, bool drawCorners, bool drawDelaunay)
        {
            int numSites = centers.Count;

            PortableColor[] defaultColors = null;
            if (!drawBiomes)
            {
                defaultColors = new PortableColor[numSites];
                for (int i = 0; i < defaultColors.Length; i++)
                {
                    defaultColors[i] = new PortableColor(255, (byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255));
                }
            }

            using (var pixelCenterGraphics = pixelCenterMap.GetGraphics())
            {

                //draw via triangles
                foreach (Center c in centers)
                {
                    DrawPolygon(g, c, drawBiomes ? GetColor(c.biome) : defaultColors[c.index]);
                    DrawPolygon(pixelCenterGraphics, c, PortableColor.FromUInt32((uint)c.index));
                }
            }

            foreach (var e in edges)
            {
                if (drawDelaunay)
                {
                    g.DrawLine(PortableColor.Yellow, (int)e.d0.loc.X, (int)e.d0.loc.Y, (int)e.d1.loc.X, (int)e.d1.loc.Y);
                }
                if (drawRivers && e.river > 0)
                {
                    g.DrawLine(River, (int)e.v0.loc.X, (int)e.v0.loc.Y, (int)e.v1.loc.X, (int)e.v1.loc.Y);
                }
            }

            if (drawSites)
            {
                centers.ForEach((s) =>
                {
                    g.FillEllipse(PortableColor.Black, (int)(s.loc.X - 2), (int)(s.loc.Y - 2), 4, 4);
                });
            }

            if (drawCorners)
            {
                corners.ForEach((c) =>
                {
                    g.FillEllipse(PortableColor.White, (int)(c.loc.X - 2), (int)(c.loc.Y - 2), 4, 4);
                });
            }
            g.DrawRectangle(PortableColor.White, (int)bounds.x, (int)bounds.y, (int)bounds.width, (int)bounds.height);
        }

        private void BuildGraph(FortunesAlgorithm<PortableColor> v)
        {
            var pointCenterMap = new Dictionary<PointDouble, Center>();
            var points = v.GetSiteCoordinates();
            points.ForEach((p) =>
            {
                var c = new Center();
                c.loc = p;
                c.index = centers.Count;
                centers.Add(c);
                pointCenterMap[p] = c;
            });

            //bug fix
            centers.ForEach((c) =>
            {
                v.GetRegion(c.loc);
            });

            var libedges = v.Edges;
            var pointCornerMap = new Dictionary<int, Corner>();

            foreach (var libedge in libedges)
            {
                LineSegment vEdge = libedge.VoronoiEdge();
                LineSegment dEdge = libedge.DelaunayLine();

                var edge = new Edge();
                edge.index = edges.Count;
                edges.Add(edge);

                edge.v0 = MakeCorner(pointCornerMap, vEdge.p0);
                edge.v1 = MakeCorner(pointCornerMap, vEdge.p1);
                edge.d0 = pointCenterMap[dEdge.p0];
                edge.d1 = pointCenterMap[dEdge.p1];

                // Centers point to edges. Corners point to edges.
                if (edge.d0 != null)
                {
                    edge.d0.borders.Add(edge);
                }
                if (edge.d1 != null)
                {
                    edge.d1.borders.Add(edge);
                }
                if (edge.v0 != null)
                {
                    edge.v0.protrudes.Add(edge);
                }
                if (edge.v1 != null)
                {
                    edge.v1.protrudes.Add(edge);
                }

                // Centers point to centers.
                if (edge.d0 != null && edge.d1 != null)
                {
                    AddToCenterList(edge.d0.neighbors, edge.d1);
                    AddToCenterList(edge.d1.neighbors, edge.d0);
                }

                // Corners point to corners
                if (edge.v0 != null && edge.v1 != null)
                {
                    AddToCornerList(edge.v0.adjacent, edge.v1);
                    AddToCornerList(edge.v1.adjacent, edge.v0);
                }

                // Centers point to corners
                if (edge.d0 != null)
                {
                    AddToCornerList(edge.d0.corners, edge.v0);
                    AddToCornerList(edge.d0.corners, edge.v1);
                }
                if (edge.d1 != null)
                {
                    AddToCornerList(edge.d1.corners, edge.v0);
                    AddToCornerList(edge.d1.corners, edge.v1);
                }

                // Corners point to centers
                if (edge.v0 != null)
                {
                    AddToCenterList(edge.v0.touches, edge.d0);
                    AddToCenterList(edge.v0.touches, edge.d1);
                }
                if (edge.v1 != null)
                {
                    AddToCenterList(edge.v1.touches, edge.d0);
                    AddToCenterList(edge.v1.touches, edge.d1);
                }
            }
        }

        // Helper functions for the following for loop; ideally these
        // would be inlined
        private void AddToCornerList(List<Corner> list, Corner c)
        {
            if (c != null && !list.Contains(c))
            {
                list.Add(c);
            }
        }

        private void AddToCenterList(List<Center> list, Center c)
        {
            if (c != null && !list.Contains(c))
            {
                list.Add(c);
            }
        }

        //ensures that each corner is represented by only one corner object
        private Corner MakeCorner(Dictionary<int, Corner> pointCornerMap, PointDouble p)
        {
            if (p == PointDouble.Empty)
            {
                return null;
            }
            int index = (int)((int)p.X + (int)(p.Y) * bounds.width * 2);
            Corner c;
            if (!pointCornerMap.TryGetValue(index, out c) || c == null)
            {
                c = new Corner();
                c.loc = p;
                c.border = bounds.LiesOnAxes(p);
                c.index = corners.Count;
                corners.Add(c);
                pointCornerMap[index] = c;
            }
            return c;
        }

        // todo: remove hardcoded stuff
        private Vector3D Transform(float x, float y, float z)
        {
            return new Vector3D(x / 1000f, z / 8f, y / 1000f);
        }

        private void AssignNormals()
        {
            //foreach (var center in centers)
            //{
            //    float nx = 0, ny = 0, nz = 0;
            //    for (var i = 0; i < center.corners.Count; i++)
            //    {
            //        var current = Transform((float)center.corners[i].loc.X, (float)center.corners[i].loc.Y, (float)center.corners[i].elevation);
            //        var j = (i + 1) % center.corners.Count;
            //        var next = Transform((float)center.corners[j].loc.X, (float)center.corners[j].loc.Y, (float)center.corners[j].elevation);

            //        nx += ((current.Y - next.Y) * (current.Z + next.Z));
            //        ny += ((current.Z - next.Z) * (current.X + next.X));
            //        nz += ((current.X - next.X) * (current.Y + next.Y));
            //    }

            //    center.normal = new Vector3D(nx, ny, nz).Normalize();
            //    if (center.normal.Dot(center.normal) < 0)
            //    {
            //        center.normal = new Vector3D(-center.normal.X, -center.normal.Y, -center.normal.Z).Normalize();
            //    }
            //}
            //foreach(var corner in corners)
            //{
            //    var normal = Vector3D.Zero;
            //    foreach(var n in corner.touches)
            //    {
            //        corner.normal += n.normal;
            //    }
            //    corner.normal = (corner.normal / corner.touches.Count).Normalize();
            //}
        }

        private void AssignCornerElevations()
        {
            var queue = new Queue<Corner>();
            foreach (var c in corners)
            {
                c.water = IsWater(c.loc);
                if (c.border)
                {
                    c.elevation = 0;
                    queue.Enqueue(c);
                }
                else
                {
                    c.elevation = double.MaxValue;
                }
            }

            while (queue.Count != 0)
            {
                Corner c = queue.Dequeue();
                foreach (Corner a in c.adjacent)
                {
                    double newElevation = 0.01 + c.elevation;
                    if (!c.water && !a.water)
                    {
                        newElevation += 1;
                    }
                    if (newElevation < a.elevation)
                    {
                        a.elevation = newElevation;
                        queue.Enqueue(a);
                    }
                }
            }
        }

        //only the radial implementation of amitp's map generation
        //TODO implement more island shapes
        private bool IsWater(PointDouble p)
        {
            p = new PointDouble(2 * (p.X / bounds.width - 0.5), 2 * (p.Y / bounds.height - 0.5));

            double angle = Math.Atan2(p.Y, p.X);
            double length = 0.5 * (Math.Max(Math.Abs(p.X), Math.Abs(p.Y)) + p.Length);

            double r1 = 0.5 + 0.40 * Math.Sin(startAngle + bumps * angle + Math.Cos((bumps + 3) * angle));
            double r2 = 0.7 - 0.20 * Math.Sin(startAngle + bumps * angle - Math.Sin((bumps + 2) * angle));
            if (Math.Abs(angle - dipAngle) < dipWidth
                    || Math.Abs(angle - dipAngle + 2 * Math.PI) < dipWidth
                    || Math.Abs(angle - dipAngle - 2 * Math.PI) < dipWidth)
            {
                r1 = r2 = 0.2;
            }
            return !(length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));

            //return false;

            /*if (noise == null) {
             noise = new Perlin2d(.125, 8, MyRandom.seed).createArray(257, 257);
             }
             int x = (int) ((p.x + 1) * 128);
             int y = (int) ((p.y + 1) * 128);
             return noise[x][y] < .3 + .3 * p.l2();*/

            /*bool eye1 = new Point(p.x - 0.2, p.y / 2 + 0.2).length() < 0.05;
             bool eye2 = new Point(p.x + 0.2, p.y / 2 + 0.2).length() < 0.05;
             bool body = p.length() < 0.8 - 0.18 * Math.sin(5 * Math.atan2(p.y, p.x));
             return !(body && !eye1 && !eye2);*/
        }

        private void AssignOceanCoastAndLand()
        {
            var queue = new Queue<Center>();
            const double waterThreshold = .3;
            foreach (var center in centers)
            {
                int numWater = 0;
                foreach (Corner c in center.corners)
                {
                    if (c.border)
                    {
                        center.border = center.water = center.ocean = true;
                        queue.Enqueue(center);
                    }
                    if (c.water)
                    {
                        numWater++;
                    }
                }
                center.water = center.ocean || ((double)numWater / center.corners.Count >= waterThreshold);
            }
            while (queue.Count != 0)
            {
                var center = queue.Dequeue();
                foreach (var n in center.neighbors)
                {
                    if (n.water && !n.ocean)
                    {
                        n.ocean = true;
                        queue.Enqueue(n);
                    }
                }
            }
            foreach (var center in centers)
            {
                bool oceanNeighbor = false;
                bool landNeighbor = false;
                foreach (var n in center.neighbors)
                {
                    oceanNeighbor |= n.ocean;
                    landNeighbor |= !n.water;
                }
                center.coast = oceanNeighbor && landNeighbor;
            }

            foreach (Corner c in corners)
            {
                int numOcean = 0;
                int numLand = 0;
                foreach (var center in c.touches)
                {
                    numOcean += center.ocean ? 1 : 0;
                    numLand += !center.water ? 1 : 0;
                }
                c.ocean = numOcean == c.touches.Count;
                c.coast = numOcean > 0 && numLand > 0;
                c.water = c.border || ((numLand != c.touches.Count) && !c.coast);
            }
        }

        private List<Corner> LandCorners()
        {
            var list = new List<Corner>();
            foreach (Corner c in corners)
            {
                if (!c.ocean && !c.coast)
                {
                    list.Add(c);
                }
            }
            return list;
        }

        private void RedistributeElevations(List<Corner> landCorners)
        {
            landCorners.Sort(new Comparison<Corner>((o1, o2) =>
            {
                if (o1.elevation > o2.elevation)
                {
                    return 1;
                }
                else if (o1.elevation < o2.elevation)
                {
                    return -1;
                }
                return 0;
            }));

            const double SCALE_FACTOR = 1.1;
            for (int i = 0; i < landCorners.Count; i++)
            {
                double y = (double)i / landCorners.Count;
                double x = Math.Sqrt(SCALE_FACTOR) - Math.Sqrt(SCALE_FACTOR * (1 - y));
                x = Math.Min(x, 1);
                landCorners[i].elevation = x;
            }

            foreach (var c in corners)
            {
                if (c.ocean || c.coast)
                {
                    c.elevation = 0.0;
                }
            }
        }

        private void AssignPolygonElevations()
        {
            foreach (var center in centers)
            {
                double total = 0;
                foreach (var c in center.corners)
                {
                    total += c.elevation;
                }
                center.elevation = total / center.corners.Count;
            }
        }

        private void CalculateDownslopes()
        {
            foreach (var c in corners)
            {
                Corner down = c;
                //System.out.println("ME: " + c.elevation);
                foreach (var a in c.adjacent)
                {
                    //System.out.println(a.elevation);
                    if (a.elevation <= down.elevation)
                    {
                        down = a;
                    }
                }
                c.downslope = down;
            }
        }

        private void CreateRivers()
        {
            for (int i = 0; i < bounds.width / 2; i++)
            {
                Corner c = corners[r.Next(corners.Count)];
                if (c.ocean || c.elevation < 0.3 || c.elevation > 0.9)
                {
                    continue;
                }
                // Bias rivers to go west: if (q.downslope.x > q.x) continue;
                while (!c.coast)
                {
                    if (c == c.downslope)
                    {
                        break;
                    }
                    Edge edge = LookupEdgeFromCorner(c, c.downslope);
                    if (!edge.v0.water || !edge.v1.water)
                    {
                        edge.river++;
                        c.river++;
                        c.downslope.river++;  // TODO: fix double count
                    }
                    c = c.downslope;
                }
            }
        }

        private Edge LookupEdgeFromCorner(Corner c, Corner downslope)
        {
            foreach (var e in c.protrudes)
            {
                if (e.v0 == downslope || e.v1 == downslope)
                {
                    return e;
                }
            }
            return null;
        }

        private void AssignCornerMoisture()
        {
            var queue = new Queue<Corner>();
            foreach (Corner c in corners)
            {
                if ((c.water || c.river > 0) && !c.ocean)
                {
                    c.moisture = c.river > 0 ? Math.Min(3.0, (0.2 * c.river)) : 1.0;
                    queue.Enqueue(c);
                }
                else
                {
                    c.moisture = 0.0;
                }
            }

            while (queue.Count != 0)
            {
                Corner c = queue.Dequeue();
                foreach (Corner a in c.adjacent)
                {
                    double newM = .9 * c.moisture;
                    if (newM > a.moisture)
                    {
                        a.moisture = newM;
                        queue.Enqueue(a);
                    }
                }
            }

            // Salt water
            foreach (Corner c in corners)
            {
                if (c.ocean || c.coast)
                {
                    c.moisture = 1.0;
                }
            }
        }

        private void RedistributeMoisture(List<Corner> landCorners)
        {
            landCorners.Sort(new Comparison<Corner>((o1, o2) =>
            {
                if (o1.moisture > o2.moisture)
                {
                    return 1;
                }
                else if (o1.moisture < o2.moisture)
                {
                    return -1;
                }
                return 0;
            }));
            for (int i = 0; i < landCorners.Count; i++)
            {
                landCorners[i].moisture = (double)i / landCorners.Count;
            }
        }

        private void AssignPolygonMoisture()
        {
            foreach (var center in centers)
            {
                double total = 0;
                foreach (Corner c in center.corners)
                {
                    total += c.moisture;
                }
                center.moisture = total / center.corners.Count;
            }
        }

        private void AssignBiomes()
        {
            foreach (var center in centers)
            {
                center.biome = GetBiome(center);
            }
        }
    }
}