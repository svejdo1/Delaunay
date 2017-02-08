using System;

namespace Barbar.Delaunay.Core
{
    internal static class GenUtils
    {
        public static bool CloseEnough(double d1, double d2, double diff)
        {
            return Math.Abs(d1 - d2) <= diff;
        }
    }
}