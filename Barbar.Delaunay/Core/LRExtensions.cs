namespace Barbar.Delaunay.Core
{
    public static class LRExtensions
    {
        public static LR other(this LR leftRight)
        {
            return leftRight == LR.Left ? LR.Right : LR.Left;
        }
    }
}
