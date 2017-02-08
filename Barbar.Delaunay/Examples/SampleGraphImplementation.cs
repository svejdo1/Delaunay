using Barbar.Delaunay.Core;
using Barbar.Delaunay.Drawing;
using Barbar.Delaunay.Voronoi;
using System;

namespace Barbar.Delaunay.Examples
{
    public class SampleGraphImplementation : VoronoiGraph
    {
        public SampleGraphImplementation(FortunesAlgorithm<PortableColor> v, int numLloydRelaxations, Random r) : base(v, numLloydRelaxations, r)
        {
            Ocean = SampleColorData.Ocean;
            Lake = SampleColorData.Lake;
            Beach = SampleColorData.Beach;
            River = SampleColorData.River;
        }

        protected override PortableColor GetColor(object biome)
        {
            return (PortableColor)biome;
        }

        protected override object GetBiome(Center p)
        {
            if (p.ocean)
            {
                return SampleColorData.Ocean;
            }
            else if (p.water)
            {
                if (p.elevation < 0.1)
                {
                    return SampleColorData.Marsh;
                }
                if (p.elevation > 0.8)
                {
                    return SampleColorData.Ice;
                }
                return SampleColorData.Lake;
            }
            else if (p.coast)
            {
                return SampleColorData.Beach;
            }
            else if (p.elevation > 0.8)
            {
                if (p.moisture > 0.50)
                {
                    return SampleColorData.Snow;
                }
                else if (p.moisture > 0.33)
                {
                    return SampleColorData.Tundra;
                }
                else if (p.moisture > 0.16)
                {
                    return SampleColorData.Bare;
                }
                else
                {
                    return SampleColorData.Scorched;
                }
            }
            else if (p.elevation > 0.6)
            {
                if (p.moisture > 0.66)
                {
                    return SampleColorData.Taiga;
                }
                else if (p.moisture > 0.33)
                {
                    return SampleColorData.Shrubland;
                }
                else
                {
                    return SampleColorData.TemperateDesert;
                }
            }
            else if (p.elevation > 0.3)
            {
                if (p.moisture > 0.83)
                {
                    return SampleColorData.TemperateRainForest;
                }
                else if (p.moisture > 0.50)
                {
                    return SampleColorData.TemperateDeciduousForest;
                }
                else if (p.moisture > 0.16)
                {
                    return SampleColorData.Grassland;
                }
                else
                {
                    return SampleColorData.TemperateDesert;
                }
            }
            else
            {
                if (p.moisture > 0.66)
                {
                    return SampleColorData.TropicalRainForest;
                }
                else if (p.moisture > 0.33)
                {
                    return SampleColorData.TropicalSeasonalForest;
                }
                else if (p.moisture > 0.16)
                {
                    return SampleColorData.Grassland;
                }
                else
                {
                    return SampleColorData.SubtropicalDesert;
                }
            }
        }
    }
}