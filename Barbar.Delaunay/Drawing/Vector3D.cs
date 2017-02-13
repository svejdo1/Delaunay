using System;

namespace Barbar.Delaunay.Drawing
{
    public struct Vector3D
    {
        public readonly float X, Y, Z;

        public static readonly Vector3D Zero = new Vector3D(0, 0, 0);

        public Vector3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
                
        public float Length
        {
            get { return (float)Math.Sqrt((X * X) + (Y * Y) + (Z * Z)); }
        }

        public Vector3D Normalize()
        {
            var length = Length;
            return new Vector3D(X / length, Y / length, Z / length);
        }

        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3D operator /(Vector3D a, float denominator)
        {
            return new Vector3D(a.X / denominator, a.Y / denominator, a.Z / denominator);
        }
    }
}
