using Barbar.Delaunay.Drawing;
using Microsoft.Xna.Framework;

namespace Barbar.Delaunay.Console3D
{
    public sealed class VertexPositionColorNormalFactory : IVertexFactory<VertexPositionColorNormal>
    {
        public VertexPositionColorNormal CreateVertex(Vector3D position, Vector3D normal, PortableColor color)
        {
            return new VertexPositionColorNormal(new Vector3(position.X, position.Y, position.Z), new Color(color.R, color.G, color.B), new Vector3(normal.X, normal.Y, normal.Z));
        }
    }
}
