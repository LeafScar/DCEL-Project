using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DCEL2D
{
    public static class Tools
    {
        /// <summary>
        /// Carica una mesh poligonale da un Object File Format su una struttura DCEL.
        /// </summary>
        /// <param name="filename">Il percorso completo del file da caricare.</param>
        /// <returns>Una mesh DCEL se caricata correttamente, null altrimenti.</returns>
        public static DCELMesh2D LoadFromOFF(string filename)
        {
            Dictionary<int, List<int>> leavingEdges = new Dictionary<int, List<int>>();
            Hashtable vIndexes = new Hashtable();
            DCELMesh2D mesh = new DCELMesh2D();
            StreamReader reader;
            string line;
            char[] separators = new char[] { ' ', '\t' };
            int vertex_count = 0, face_count = 0;

            try { reader = new StreamReader(filename); }
            catch (FileNotFoundException) { return null; }

            while ((line = reader.ReadLine()) != null)
                if (line == "OFF" || line == "NOFF")
                    break;
           
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

                            // x, y coordinates
                            mesh.AddVertex(new DCELVertex2D(new Point(
                                Double.Parse(values[0].Replace('.', ',')),
                                Double.Parse(values[1].Replace('.', ','))), null));

                            leavingEdges.Add(i, new List<int>());

                            //non possono esistere due vertici con le stesse coordinate
                            try { vIndexes.Add(mesh.VertexList[i], i); }
                            catch (Exception) { return null; }                                
                        }
                        else i--;
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
                            mesh.AddFace(new DCELFace2D());

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
                                    mesh.AddHalfEdge(new DCELHalfEdge2D());
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
                                    mesh.AddHalfEdge(new DCELHalfEdge2D());
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
                        }
                        else i--;
                    }
                    //
                    #endregion
                        
                    #region Leaving Edges

                    for (int i = 0; i < vertex_count; i++)
                    {
                        if (leavingEdges[i].Count > 0)
                        {
                            mesh.VertexList[i].Leaving = mesh.HalfEdgeList[leavingEdges[i][0]];                                
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
            //if null is returned, something is gone wrong!
            return null;
        }

        #region Private

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
