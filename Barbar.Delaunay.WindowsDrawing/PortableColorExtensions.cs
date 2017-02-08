using Barbar.Delaunay.Drawing;
using System.Drawing;

namespace Barbar.Delaunay.WindowsDrawing
{
    internal static class PortableColorExtensions
    {
        public static Color ToColor(this PortableColor color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
