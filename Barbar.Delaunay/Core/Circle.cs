namespace Barbar.Delaunay.Core
{
    public struct Circle
    {
        public readonly PointDouble Center;
        public readonly double Radius;

        public Circle(double centerX, double centerY, double radius)
        {
            Center = new PointDouble(centerX, centerY);
            Radius = radius;
        }

        public override string ToString()
        {
            return string.Format("Circle (center: {0}; radius: {1})", Center, Radius);
        }
    }
}