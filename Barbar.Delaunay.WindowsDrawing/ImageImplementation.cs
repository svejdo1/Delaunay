using Barbar.Delaunay.Drawing;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Barbar.Delaunay.WindowsDrawing
{
    internal sealed class ImageImplementation : IImage
    {
        private Image _image;

        public ImageImplementation(Image image)
        {
            _image = image;
        }


        public int Height
        {
            get
            {
                return _image.Height;
            }
        }

        public int Width
        {
            get
            {
                return _image.Width;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ImageImplementation()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
            if (_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }

        public IGraphics GetGraphics()
        {
            return new GraphicsImplementation(Graphics.FromImage(_image));
        }

        public void SaveAsPng(Stream stream)
        {
            _image.Save(stream, ImageFormat.Png);
        }
    }
}
