using Barbar.Delaunay.Core;
using System.Collections.Generic;
using Barbar.Delaunay.Drawing;

namespace Barbar.Delaunay.Voronoi
{
    public class Corner {
        public List<Center> touches = new List<Center>();
        public List<Corner> adjacent = new List<Corner>();
        public List<Edge> protrudes = new List<Edge>();
        public PointDouble loc;
        public int index;
        public bool border;
        public double elevation;
        public bool water, ocean, coast;
        public Corner downslope;
        public int river;
        public double moisture;
        public Vector3D normal;
    }
}