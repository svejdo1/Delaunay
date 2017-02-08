using Barbar.Delaunay.Core;

namespace Barbar.Delaunay.Voronoi
{
    public class Edge
    {
        public int index;
        public Center d0, d1;  // Delaunay edge
        public Corner v0, v1;  // Voronoi edge
        public PointDouble midpoint;  // halfway between v0,v1
        public int river;

        public void SetVornoi(Corner v0, Corner v1)
        {
            this.v0 = v0;
            this.v1 = v1;
            midpoint = new PointDouble((v0.loc.X + v1.loc.X) / 2, (v0.loc.Y + v1.loc.Y) / 2);
        }
    }
}