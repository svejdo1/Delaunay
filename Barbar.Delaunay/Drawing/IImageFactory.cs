namespace Barbar.Delaunay.Drawing
{
    public interface IImageFactory
    {
        IImage CreateBitmap32bppArgb(int width, int height);
    }
}
