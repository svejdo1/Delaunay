using System.Collections.Generic;

namespace Barbar.Delaunay.Core
{
    public sealed class Halfedge
    {

        private static Stack<Halfedge> _pool = new Stack<Halfedge>();

        public Halfedge edgeListLeftNeighbor, edgeListRightNeighbor;
        public Halfedge nextInPriorityQueue;
        public Edge edge;
        public LR leftRight;
        public Vertex vertex;
        // the vertex's y-coordinate in the transformed Voronoi space V*
        public double ystar;

        public Halfedge(Edge edge, LR lr)
        {
            Init(edge, lr);
        }

        private Halfedge Init(Edge edge, LR lr)
        {
            this.edge = edge;
            leftRight = lr;
            nextInPriorityQueue = null;
            vertex = null;
            return this;
        }

        public override string ToString()
        {
            return "Halfedge (leftRight: " + leftRight + "; vertex: " + vertex + ")";
        }

        public void dispose()
        {
            if (edgeListLeftNeighbor != null || edgeListRightNeighbor != null)
            {
                // still in EdgeList
                return;
            }
            if (nextInPriorityQueue != null)
            {
                // still in PriorityQueue
                return;
            }
            edge = null;
            //leftRight = null;
            vertex = null;
            _pool.Push(this);
        }

        public void ReallyDispose()
        {
            edgeListLeftNeighbor = null;
            edgeListRightNeighbor = null;
            nextInPriorityQueue = null;
            edge = null;
            //leftRight = null;
            vertex = null;
            _pool.Push(this);
        }

        public bool IsLeftOf(PointDouble p)
        {
            Site topSite;
            bool rightOfSite, above, fast;
            double dxp, dyp, dxs, t1, t2, t3, yl;

            topSite = edge.RightSite;
            rightOfSite = p.X > topSite.X;
            if (rightOfSite && this.leftRight == LR.Left)
            {
                return true;
            }
            if (!rightOfSite && this.leftRight == LR.Right)
            {
                return false;
            }

            if (edge.a == 1.0)
            {
                dyp = p.Y - topSite.Y;
                dxp = p.X - topSite.X;
                fast = false;
                if ((!rightOfSite && edge.b < 0.0) || (rightOfSite && edge.b >= 0.0))
                {
                    above = dyp >= edge.b * dxp;
                    fast = above;
                }
                else
                {
                    above = p.X + p.Y * edge.b > edge.c;
                    if (edge.b < 0.0)
                    {
                        above = !above;
                    }
                    if (!above)
                    {
                        fast = true;
                    }
                }
                if (!fast)
                {
                    dxs = topSite.X - edge.LeftSite.X;
                    above = edge.b * (dxp * dxp - dyp * dyp)
                            < dxs * dyp * (1.0 + 2.0 * dxp / dxs + edge.b * edge.b);
                    if (edge.b < 0.0)
                    {
                        above = !above;
                    }
                }
            }
            else /* edge.b == 1.0 */
            {
                yl = edge.c - edge.a * p.X;
                t1 = p.Y - yl;
                t2 = p.X - topSite.X;
                t3 = yl - topSite.Y;
                above = t1 * t1 > t2 * t2 + t3 * t3;
            }
            return this.leftRight == LR.Left ? above : !above;
        }

        public static Halfedge Create(Edge edge, LR lr)
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop().Init(edge, lr);
            }
            else
            {
                return new Halfedge(edge, lr);
            }
        }

        public static Halfedge CreateDummy()
        {
            return Create(null, LR.None);
        }
    }
}