using Barbar.Delaunay.Core;
using System.Collections.Generic;

namespace Barbar.Delaunay.Voronoi
{
    public class Center
    {
        public int index;
        public PointDouble loc;
        public List<Corner> corners = new List<Corner>();
        public List<Center> neighbors = new List<Center>();
        public List<Edge> borders = new List<Edge>();
        public bool border, ocean, water, coast;
        public double elevation;
        public double moisture;
        public object biome;
        public double area;

        public Center()
        {
        }

        public Center(PointDouble loc)
        {
            this.loc = loc;
        }
    }
}