using Barbar.Delaunay.Examples;
using Barbar.Delaunay.WindowsDrawing;

namespace Barbar.Delaunay.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrapper.Initialize();
            SampleGenerator.CreateVoronoiGraphAndSave();
        }
    }
}
