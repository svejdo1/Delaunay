using Barbar.Delaunay.Core;
using Barbar.Delaunay.Drawing;
using Barbar.Delaunay.Voronoi;
using System;
using System.IO;

namespace Barbar.Delaunay.Examples
{
    public static class SampleGenerator
    {
        public static VoronoiGraph CreateVoronoiGraph()
        {
            return CreateVoronoiGraph(1000, 30000, 2, (int)DateTime.Now.TimeOfDay.TotalMilliseconds);
        }

        public static VoronoiGraph CreateVoronoiGraph(int bounds, int numSites, int numLloydRelaxations, int seed)
        {
            var r = new Random(seed);

            //make the intial underlying voronoi structure
            var v = new FortunesAlgorithm<PortableColor>(numSites, bounds, bounds, r, null);

            //assemble the voronoi strucutre into a usable graph object representing a map
            var graph = new SampleGraphImplementation(v, numLloydRelaxations, r);

            return graph;
        }

        public static void SaveAsPng(VoronoiGraph graph, string fileName)
        {
            using (var image = graph.CreateMap())
            using (var stream = new MemoryStream(image.Width * image.Height * 4))
            {
                image.SaveAsPng(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    stream.CopyTo(fileStream);
                }
            }
        }

        public static void CreateVoronoiGraphAndSave()
        {
            int bounds = 1000;
            int numSites = 30000;
            int numLloydRelxations = 2;
            int seed = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;

            var graph = CreateVoronoiGraph(bounds, numSites, numLloydRelxations, seed);
            string fileName = $"seed-{seed}-sites-{numSites}-lloyds-{numLloydRelxations}.png";
            SaveAsPng(graph, fileName);
        }
    }
}