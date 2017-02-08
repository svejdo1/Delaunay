using Barbar.Delaunay.Drawing;

namespace Barbar.Delaunay.WindowsDrawing
{
    public static class Bootstrapper
    {
        public static void Initialize()
        {
            DrawingHook.ImageFactory = new ImageFactoryImplementation();
        }
    }
}
