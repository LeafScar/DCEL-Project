using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Text;
using DCEL;

namespace DCEL
{
    public static class MathUtils
    {
        public static void CalculateNormal(DCELFace dcelFace)
        {
            Vector3D vector1 = new Vector3D(
                dcelFace.Edge.Next.Origin.Coordinates.X - dcelFace.Edge.Origin.Coordinates.X,
                dcelFace.Edge.Next.Origin.Coordinates.Y - dcelFace.Edge.Origin.Coordinates.Y,
                dcelFace.Edge.Next.Origin.Coordinates.Z - dcelFace.Edge.Origin.Coordinates.Z);

            Vector3D vector2 = new Vector3D(
                dcelFace.Edge.Next.Next.Origin.Coordinates.X - dcelFace.Edge.Origin.Coordinates.X,
                dcelFace.Edge.Next.Next.Origin.Coordinates.Y - dcelFace.Edge.Origin.Coordinates.Y,
                dcelFace.Edge.Next.Next.Origin.Coordinates.Z - dcelFace.Edge.Origin.Coordinates.Z);

            //normale
            Vector3D normal = CrossProduct(vector1, vector2);
            dcelFace.Normal = Normalize(normal);
        }

        public static Vector3D CrossProduct(Vector3D vector1, Vector3D vector2)
        {
            return new Vector3D(
                (vector1.Y * vector2.Z) - (vector1.Z * vector2.Y),
                (vector2.X * vector1.Z) - (vector2.Z * vector1.X),
                (vector1.X * vector2.Y) - (vector1.Y * vector2.X));
        }

        public static Vector3D Normalize(Vector3D vector)
        {
            double magnitude = Math.Sqrt(
                Math.Pow(vector.X, 2) +
                Math.Pow(vector.Y, 2) +
                Math.Pow(vector.Z, 2));

            vector.X /= magnitude;
            vector.Y /= magnitude;
            vector.Z /= magnitude;

            return vector;
        }
    }
}
