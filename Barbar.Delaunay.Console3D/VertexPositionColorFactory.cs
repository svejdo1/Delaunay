using Barbar.Delaunay.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Barbar.Delaunay.Console3D
{
    public sealed class VertexPositionColorFactory : IVertexFactory<VertexPositionColor>
    {
        public VertexPositionColor CreateVertex(Vector3D position, Vector3D normal, PortableColor color)
        {
            return new VertexPositionColor(new Vector3(position.X, position.Y, position.Z), new Color(color.R, color.G, color.B));
        }
    }
}
