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

        public float Dot(Vector3D vector)
        {
            return X * vector.X + Y * vector.Y + Z * vector.Z;
        }

        public Vector3D CrossProduct(Vector3D vector)
        {
            return new Vector3D(Y * vector.Z - vector.Y * Z,
                                -(X * vector.Z - vector.X * Z),
                                X * vector.Y - vector.X * Y);
        }

        public Vector3D Normalize()
        {
            float factor = 1 / Length;
            return new Vector3D(X * factor, Y * factor, Z * factor);
        }

        public static Vector3D operator +(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3D operator -(Vector3D a, Vector3D b)
        {
            return new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }


        public static Vector3D operator /(Vector3D a, float denominator)
        {
            return new Vector3D(a.X / denominator, a.Y / denominator, a.Z / denominator);
        }
    }
}
