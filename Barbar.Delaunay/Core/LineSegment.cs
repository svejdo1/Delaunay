namespace Barbar.Delaunay.Core
{
    public sealed class LineSegment
    {
        public PointDouble p0, p1;

        public LineSegment(PointDouble p0, PointDouble p1)
        {
            this.p0 = p0;
            this.p1 = p1;
        }

        public static double CompareLengthsMax(LineSegment segment0, LineSegment segment1)
        {
            double length0 = PointDouble.Distance(segment0.p0, segment0.p1);
            double length1 = PointDouble.Distance(segment1.p0, segment1.p1);
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

        public static double CompareLengths(LineSegment edge0, LineSegment edge1)
        {
            return -CompareLengthsMax(edge0, edge1);
        }
    }
}