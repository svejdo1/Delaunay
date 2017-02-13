namespace Barbar.Delaunay.Drawing
{
    public interface IVertexFactory<T>
    {
        T CreateVertex(Vector3D position, Vector3D normal, PortableColor color);
    }
}
