using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace DCEL
{
    public static class DCELTools
    {
        /// <summary>
        /// Carica una mesh poligonale da un Object File Format su una struttura DCEL.
        /// </summary>
        /// <param name="filename">Il percorso completo del file da caricare.</param>
        /// <returns>Una mesh DCEL se caricata correttamente, null altrimenti.</returns>
        public static DCELMesh LoadFromOFF(string filename)
        {
            Dictionary<int, List<int>> leavingEdges = new Dictionary<int, List<int>>();
            Hashtable vIndexes = new Hashtable();
            DCELMesh mesh = new DCELMesh();
            StreamReader reader;
            char[] separators = new char[] { ' ', '\t' };
            int vertex_count = 0, face_count = 0;

            try
            {
                reader = new StreamReader(filename);
            }
            catch (FileNotFoundException)
            {
                return null;
            }            
            string line = reader.ReadLine();

            if (line == "OFF" || line == "NOFF")
            {
                while ((line = reader.ReadLine()) != null)
                {                    
                    if (CheckLine(line))
                    {
                        string[] values = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                        vertex_count = Convert.ToInt32(values[0]);
                        face_count = Convert.ToInt32(values[1]);

                        //MESH INITIALIZATION 
                        
                        #region Vertices Creation
                        //
                        for (int i = 0; i < vertex_count; i++)
                        {
                            line = reader.ReadLine();

                            if (CheckLine(line))
                            {
                                values = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                                // x, y, z coordinates
                                mesh.AddVertex(new DCELVertex(new Point3D(
                                    Double.Parse(values[0].Replace(',', '.')),
                                    Double.Parse(values[1].Replace(',', '.')),
                                    Double.Parse(values[2].Replace(',', '.'))), null));

                                leavingEdges.Add(i, new List<int>());

                                try
                                {
                                    vIndexes.Add(mesh.VertexList[i], i);
                                }
                                catch (Exception)
                                {
                                    //non possono esistere due vertici con le stesse coordinate
                                    return null;
                                }
                                
                            }
                            else
                            {
                                i--;
                            }
                        }
                        //
                        #endregion
                        
                        #region Faces Creation
                        //
                        int he_counter = 0;

                        for (int i = 0; i < face_count; i++)
                        {
                            line = reader.ReadLine();

                            if (CheckLine(line))
                            {
                                values = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                                //k è il numero di vertici della faccia corrente
                                int k = Convert.ToInt32(values[0]);

                                int starting_index = he_counter;
                                bool check_first = true;
                                mesh.AddFace(new DCELFace());

                                //for each vertex
                                for (int j = 1; j <= k; j++)
                                {
                                    //@vertex_index è l'indice del vertice all'interno della collezione VertexList
                                    int vertex_index = Convert.ToInt32(values[j]);

                                    #region Half Edges Calculation

                                    if (check_first)
                                    {
                                        check_first = false;
                                        //creo un nuovo halfedge e lo aggiungo alla lista
                                        mesh.AddHalfEdge(new DCELHalfEdge());
                                        //imposto come vertice di origine il point3d corrispondente all'indice vertex_index in VertexList
                                        mesh.HalfEdgeList[he_counter].Origin = mesh.VertexList[vertex_index];
                                        //imposto il primo halfedge come halfedge di riferimento della faccia corrente
                                        mesh.FaceList[i].Edge = mesh.HalfEdgeList[he_counter];
                                        //l'halfedge corrente si collega alla faccia corrente
                                        mesh.HalfEdgeList[he_counter].Face = mesh.FaceList[i];
                                        //
                                        leavingEdges[vertex_index].Add(he_counter);
                                        //incremento il contatore
                                        he_counter += 1;
                                    }
                                    else
                                    {
                                        //creo un nuovo halfedge e lo aggiungo alla lista
                                        mesh.AddHalfEdge(new DCELHalfEdge());
                                        //l'halfedge successivo al precedente è quello corrente. Incremento face_count e assegno
                                        mesh.HalfEdgeList[he_counter - 1].Next = mesh.HalfEdgeList[he_counter];
                                        //assegno il punto d'origine corrispondente all'indice vertex_index in Mesh.VertexList
                                        mesh.HalfEdgeList[he_counter].Origin = mesh.VertexList[vertex_index];
                                        //collego l'halfedge alla faccia corrente
                                        mesh.HalfEdgeList[he_counter].Face = mesh.FaceList[i];
                                        //
                                        leavingEdges[vertex_index].Add(he_counter);
                                        //incremento il contatore
                                        he_counter += 1;
                                    }
                                    //finito di iterare sui vertici, l'ultimo halfedge ha come next l'halfedge di partenza
                                    mesh.HalfEdgeList[he_counter - 1].Next = mesh.HalfEdgeList[starting_index];

                                    #endregion
                                }
                                //face normal
                                MathUtils.CalculateNormal(mesh.FaceList[i]);
                            }
                            else
                            {
                                i--;
                            }
                        }
                        //
                        #endregion
                        
                        #region Leaving Edges and Vertex Normals Calculation

                        for (int i = 0; i < vertex_count; i++)
                        {
                            if (leavingEdges[i].Count > 0)
                            {
                                mesh.VertexList[i].Leaving = mesh.HalfEdgeList[leavingEdges[i][0]];
                                //face normal
                                mesh.VertexList[i].Normal = new Vector3D();
                                //for each leaving edge from the current vertex
                                foreach (var lEdgeIndex in leavingEdges[i])
                                    //sum the face normals for every triangle containing the vertex
                                    mesh.VertexList[i].Normal += mesh.HalfEdgeList[lEdgeIndex].Face.Normal;

                                //get average normal
                                mesh.VertexList[i].Normal /= leavingEdges[i].Count;
                                mesh.VertexList[i].Normal = MathUtils.Normalize(mesh.VertexList[i].Normal);
                            }
                        }

                        #endregion
                        
                        #region Twin Edges Calculation

                        foreach (var edge in mesh.HalfEdgeList)
                        {
                            //index è la posizione in VertexList del vertice dell'edge successivo a quello corrente
                            int index = (int)vIndexes[edge.Next.Origin];

                            //edge_index è la posizione in EdgeList dell'edge con origine nel vertice dell'indice di cui sopra!
                            foreach (var edge_index in leavingEdges[index])
                            {
                                if (edge.Origin == mesh.HalfEdgeList[edge_index].Next.Origin)
                                {
                                    edge.Twin = mesh.HalfEdgeList[edge_index];
                                    break;
                                }
                            }
                        }
                        //
                        #endregion
                        
                        reader.Close();
                        //restituisco la mesh caricata
                        return mesh;
                    }                    
                }
            }
            //if null is returned, something is gone wrong!
            return null;
        }

        /// <summary>
        /// Esporta una mesh poligonale in formato OFF.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static bool SaveToOFF(string filename, DCELMesh mesh)
        {
            StreamWriter writer = File.CreateText(filename);
            string v;

            //header
            writer.WriteLine("OFF");
            //vertex and face count
            writer.Write(mesh.VertexCount + " " + mesh.FaceCount + " " + mesh.HalfEdgeCount / 2);
            writer.WriteLine();
            //vertex list
            foreach (var vertex in mesh.VertexList)
            {
                v = vertex.Coordinates.X + " " + vertex.Coordinates.Y + " " + vertex.Coordinates.Z;
                writer.WriteLine(v.Replace(",", "."));
            }
            //face list
            foreach (var face in mesh.FaceList)
            {
                writer.Write(face.Vertices().Count());

                foreach (var vertex in face.Vertices())
                {
                    writer.Write(" " + mesh.VertexList.IndexOf(vertex));
                }
                writer.WriteLine();
            }
            writer.Close();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static MeshGeometry3D GetMeshGeometry(DCELMesh mesh)
        {
            MeshGeometry3D mesh3d = new MeshGeometry3D();
            Hashtable vIndexes = new Hashtable();
            int n = 0;
            mesh.Triangulate();

            foreach (var point3d in mesh.VertexList)
            {
                mesh3d.Positions.Add(point3d.Coordinates);
                mesh3d.Normals.Add(point3d.Normal);
                vIndexes.Add(point3d, n++);
            }

            foreach (var face in mesh.FaceList)
            {
                DCELHalfEdge he = face.Edge;
                DCELVertex first = he.Origin;

                do
                {
                    mesh3d.TriangleIndices.Add((int)vIndexes[he.Origin]);
                    he = he.Next;
                }
                while (he.Origin != first);
            }
            /*
            foreach (var face in mesh.FaceList)
            {
                DCELHalfEdge he = face.Edge;
                DCELVertex first = he.Origin;

                do
                {
                    mesh3d.Positions.Add(he.Origin.Coordinates);
                    mesh3d.Normals.Add(he.Origin.Normal);
                    mesh3d.TriangleIndices.Add(n++);
                    he = he.Next;
                }
                while (he.Origin != first);
            }
            */
            return mesh3d;
        }

        #region Serialization Methods

        public static bool SerializeMesh(string filename, DCELMesh meshToSerialize)
        {
            Stream stream = File.Open(filename, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, meshToSerialize);
            stream.Close();

            return true;
        }

        public static DCELMesh DeserializeMesh(string filename)
        {
            DCELMesh deserializedMesh;
            Stream stream = File.Open(filename, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            deserializedMesh = (DCELMesh)formatter.Deserialize(stream);
            stream.Close();

            return deserializedMesh;
        }

        #endregion

        #region Private Methods

        private static bool CheckLine(string line)
        {
            if (line != "")
                if (line[0] != '#')
                    return true;

            return false;
        }

        #endregion

    }
}
