using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Windows.Media.Media3D;
using DCEL;

namespace Primitives3D
{
    public class DCELPrimitive : GeometricPrimitive
    {
        public DCELPrimitive(GraphicsDevice graphicsDevice, DCELMesh mesh, float size)
        {
            int n = 0;
            mesh.Triangulate();

            foreach (var face in mesh.FaceList)
            {                
                DCELHalfEdge he = face.Edge;
                DCELVertex first = he.Origin;

                do
                {
                    AddVertex(
                        GetVector3(he.Origin.Coordinates) * size,
                        GetVector3(he.Origin.Normal));
                    AddIndex(n++);
                    he = he.Next;
                }
                while (he.Origin != first);
            }

            InitializePrimitive(graphicsDevice);
        }

        private Vector3 GetVector3(Point3D point3d)
        {
            return new Vector3((float)point3d.X, (float)point3d.Y, (float)point3d.Z);
        }

        private Vector3 GetVector3(Vector3D vector3d)
        {
            return new Vector3((float)vector3d.X, (float)vector3d.Y, (float)vector3d.Z);
        }
    }
}
