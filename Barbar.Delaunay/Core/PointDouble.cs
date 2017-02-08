using System;

namespace Barbar.Delaunay.Core
{
    public struct PointDouble {
        public readonly double X, Y;

        public static PointDouble Empty = new PointDouble(0, 0);

        public PointDouble(double x, double y) {
            X = x;
            Y = y;
        }

        public override string ToString() {
            return string.Format("{0}, {1}", X, Y);
        }

        public double Length {
            get { return Math.Sqrt(X * X + Y * Y); }
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            var another = (PointDouble)obj;
            return X == another.X && Y == another.Y;
        }

        public static double Distance(PointDouble _coord, PointDouble _coord0)
        {
            return Math.Sqrt((_coord.X - _coord0.X) * (_coord.X - _coord0.X) + (_coord.Y - _coord0.Y) * (_coord.Y - _coord0.Y));
        }

        public static bool operator ==(PointDouble c1, PointDouble c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(PointDouble c1, PointDouble c2)
        {
            return !c1.Equals(c2);
        }
    }
}