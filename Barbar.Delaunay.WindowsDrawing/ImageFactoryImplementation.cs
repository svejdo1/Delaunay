using Barbar.Delaunay.Drawing;
using System.Drawing;
using System.Drawing.Imaging;

namespace Barbar.Delaunay.WindowsDrawing
{
    internal sealed class ImageFactoryImplementation : IImageFactory
    {
        public IImage CreateBitmap32bppArgb(int width, int height)
        {
            return new ImageImplementation(new Bitmap(width, height, PixelFormat.Format32bppArgb));
        }
    }
}
