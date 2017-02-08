namespace Barbar.Delaunay.Core
{
    internal static class BoundsCheck
    {
        public const int TOP = 1;
        public const int BOTTOM = 2;
        public const int LEFT = 4;
        public const int RIGHT = 8;

        /// <summary>
        /// </summary>
        /// <param name="point"></param>
        /// <param name="bounds"></param>
        /// <returns>an int with the appropriate bits set if the Point lies on the corresponding bounds lines</returns>
        public static int Check(PointDouble point, Rectangle bounds)
        {
            int value = 0;
            if (point.X == bounds.left)
            {
                value |= LEFT;
            }
            if (point.X == bounds.right)
            {
                value |= RIGHT;
            }
            if (point.Y == bounds.top)
            {
                value |= TOP;
            }
            if (point.Y == bounds.bottom)
            {
                value |= BOTTOM;
            }
            return value;
        }
    }
}
