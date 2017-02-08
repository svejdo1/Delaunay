using System;
using System.IO;

namespace Barbar.Delaunay.Drawing
{
    public interface IImage : IDisposable
    {
        int Width { get; }
        int Height { get; }
        void SaveAsPng(Stream stream);
        IGraphics GetGraphics();
    }
}
