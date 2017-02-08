namespace Barbar.Delaunay.Core
{
    public struct Rectangle
    {
        public readonly double x, y, width, height, right, bottom, left, top;

        public Rectangle(double x, double y, double width, double height)
        {
            left = this.x = x;
            top = this.y = y;
            this.width = width;
            this.height = height;
            right = x + width;
            bottom = y + height;
        }

        public bool LiesOnAxes(PointDouble p)
        {
            return GenUtils.CloseEnough(p.X, x, 1) || GenUtils.CloseEnough(p.Y, y, 1) || GenUtils.CloseEnough(p.X, right, 1) || GenUtils.CloseEnough(p.Y, bottom, 1);
        }

        public bool InBounds(PointDouble p)
        {
            return InBounds(p.X, p.Y);
        }

        public bool InBounds(double x0, double y0)
        {
            return !(x0 < x || x0 > right || y0 < y || y0 > bottom);
        }
    }
}
