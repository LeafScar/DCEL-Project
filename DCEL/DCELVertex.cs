using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace DCEL
{
    [Serializable]
    public class DCELVertex
    {
        public Point3D Coordinates { get; set; }
        public DCELHalfEdge Leaving { get; set; }
        public Vector3D Normal { get; set; }

        #region Constructors

        public DCELVertex() { }

        public DCELVertex(double x, double y, double z, DCELHalfEdge leaving)
        {
            Coordinates = new Point3D(x, y, z);
            Leaving = leaving;
        }

        public DCELVertex(double x, double y, double z, DCELHalfEdge leaving, Vector3D normal)
        {
            Coordinates = new Point3D(x, y, z);
            Leaving = leaving;
            Normal = normal;
        }

        public DCELVertex(Point3D coordinates, DCELHalfEdge leaving)
        {
            Coordinates = coordinates;
            Leaving = leaving;
        }

        public DCELVertex(Point3D coordinates, DCELHalfEdge leaving, Vector3D normal)
        {
            Coordinates = coordinates;
            Leaving = leaving;
            Normal = normal;
        }

        #endregion

        /// <summary>
        /// Verifica che tutti i riferimenti dell'oggetto non puntino a null.
        /// </summary>
        /// <returns>True se l'oggetto è consistente, false altrimenti.</returns>
        public bool IsConsistent()
        {
            if (Coordinates != null && Leaving != null && Normal != null)
                return true;

            return false;
        }
        
        /// <summary>
        /// Restituisce tutti gli HalfEdge che partono da questo vertice.
        /// </summary>
        /// <returns>DCELHalfEdge collection.</returns>
        public IEnumerable<DCELHalfEdge> LeavingEdges()
        {
            if (this.Leaving == null)
                yield break;

            DCELHalfEdge he = this.Leaving;
            
            do
            {
                yield return he;
                he = he.Previous().Twin;
            }
            while (he != this.Leaving);
        }

        /// <summary>
        /// Restituisce tutte le facce adiacenti a questo vertice.
        /// </summary>
        /// <returns>DCELFace collection.</returns>
        public IEnumerable<DCELFace> AdjacentFaces()
        {
            foreach (var edge in LeavingEdges())
                if (!edge.Face.IsInfinite())
                    yield return edge.Face;
        }

        /// <summary>
        /// Restituisce tutti i vertici che si trovano a distanza 1.
        /// </summary>
        /// <returns>DCELVertex collection.</returns>
        public IEnumerable<DCELVertex> AdjacentVertices()
        {
            foreach (var edge in LeavingEdges())
                yield return edge.Next.Origin;
        }
        
        /// <summary>
        /// Restituisce tutti i vertici che si trovano a distanza k.
        /// </summary>
        /// <param name="k">Distanza k dal vertice.</param>
        /// <returns>DCELVertex collection</returns>
        public IEnumerable<DCELVertex> KDistanceVertices(int k)
        {
            if (k < 0)
                yield break;

            //lista vertici visitati
            List<DCELVertex> visited = new List<DCELVertex>();
            //lista di appoggio
            List<DCELVertex> auxList = new List<DCELVertex>();
            //aggiungo alla lista il primo vertice
            visited.Add(this);
            //elemento corrente
            int n = 0;

            while (k > 0)
            {
                foreach (var item in visited.Skip(n))
                    auxList.Add(item);

                foreach (var current in auxList)
                {
                    foreach (var vertex in current.AdjacentVertices())
                    {
                        if (!visited.Contains(vertex))
                        {
                            visited.Add(vertex);
                            yield return vertex;
                        }
                    }
                }
                k--;
                n = auxList.Count;
                auxList.Clear();
            }
        }

        /// <summary>
        /// Restituisce tutte le facce nell'intorno k del vertice. A distanza 1
        /// verranno restituite le facce immediatamente adiacenti.
        /// </summary>
        /// <param name="k">Distanza k dal vertice.</param>
        /// <returns>DCELFace collection</returns>
        public IEnumerable<DCELFace> KStar(int k)
        {
            if (k <= 0)
                yield break;
            
            //lista visite
            List<DCELFace> visited = new List<DCELFace>();
            //lista di appoggio
            List<DCELFace> auxList = new List<DCELFace>();
            //distanza corrente
            int distance = 1;
            //primo elemento loop
            int n = 0;

            foreach (var item in AdjacentFaces())
            {
                if (k == 1)
                    yield return item;
                else
                    visited.Add(item);
            }

            while (distance != k)
            {
                foreach (var item in visited.Skip(n))
                    auxList.Add(item);

                foreach (var current in auxList)
                {
                    foreach (var face in current.Neighbours())
                    {
                        if (!visited.Contains(face))
                        {
                            visited.Add(face);
                            if (distance + 1 == k)
                                yield return face;
                        }
                    }
                }
                distance += 1;
                n = auxList.Count;
                auxList.Clear();
            }
        }

        /// <summary>
        /// Calcola la normale per il vertice. La normale è data dalla normalizzazione
        /// della media delle normali delle facce di tutti gli HalfEdge che partono da
        /// questo vertice.
        /// </summary>
        /// <returns></returns>
        public void CalculateNormal()
        {
            int count = 0;

            foreach (var edge in LeavingEdges())
            {
                if (!edge.Face.IsInfinite())
                {
                    this.Normal += edge.Face.Normal;
                    count++;
                }
            }

            this.Normal /= count;
            this.Normal = MathUtils.Normalize(this.Normal);
        }

        #region Deprecated
        /*
        /// <summary>
        /// Restituisce una lista contenente tutti gli HalfEdge che partono da questo vertice.
        /// </summary>
        /// <returns>DCELHalfEdge collection.</returns>
        public List<DCELHalfEdge> LeavingEdges()
        {
            if (this.Leaving == null)
                return null;

            List<DCELHalfEdge> edgelist = new List<DCELHalfEdge>();
            DCELHalfEdge he = this.Leaving;

            do
            {
                edgelist.Add(he);
                he = he.Previous().Twin;
            }
            while (he != this.Leaving);

            if (edgelist.Count < 3)
                return null;

            return edgelist;
        }
        */
        /*
        /// <summary>
        /// Restituisce una lista contenente tutte le facce adiacenti a questo vertice.
        /// </summary>
        /// <returns>DCELFace collection.</returns>
        public List<DCELFace> AdjacentFaces()
        {
            List<DCELFace> facelist = new List<DCELFace>();

            foreach (var edge in this.LeavingEdges())
            {
                if (!edge.Face.IsInfinite())
                    facelist.Add(edge.Face);
            }

            return facelist;
        }
        */
        /*
        /// <summary>
        /// Restituisce una lista contenente tutti i vertici che si trovano a distanza 1.
        /// </summary>
        /// <returns>DCELVertex collection.</returns>
        public List<DCELVertex> AdjacentVertices()
        {
            List<DCELVertex> vertexList = new List<DCELVertex>();

            foreach (var edge in this.LeavingEdges())
                vertexList.Add(edge.Next.Origin);

            return vertexList;
        }
        */
        /*
        /// <summary>
        /// Restituisce tutti i vertici che si trovano a distanza k.
        /// </summary>
        /// <param name="k">Distanza k dal vertice.</param>
        /// <returns>DCELVertex collection</returns>
        public List<DCELVertex> KDistanceVertices(int k)
        {
            if (k < 0)
                return null;

            //lista vertici visitati
            List<DCELVertex> visited = new List<DCELVertex>();
            //lista di appoggio
            List<DCELVertex> auxList = new List<DCELVertex>();
            //aggiungo alla lista il primo vertice
            visited.Add(this);
            //elemento corrente
            int n = 0;

            while (k > 0)
            {
                foreach (var item in visited.Skip(n))
                    auxList.Add(item);

                foreach (var current in auxList)
                {
                    foreach (var vertex in current.AdjacentVertices())
                    {
                        if (!visited.Contains(vertex))
                            visited.Add(vertex);
                    }
                }
                k--;
                n = auxList.Count;
                auxList.Clear();
            }

            return visited;
        }
         * */
        /*
        /// <summary>
        /// Restituisce una lista contenente tutte le facce nell'intorno k del vertice. A distanza 1
        /// verranno restituite le facce immediatamente adiacenti.
        /// </summary>
        /// <param name="k">Distanza k dal vertice.</param>
        /// <returns>DCELFace collection</returns>
        public List<DCELFace> KStar(int k)
        {
            if (k <= 0)
                return null;

            //lista visite
            Dictionary<DCELFace, int> visited = new Dictionary<DCELFace, int>();
            //lista di appoggio
            List<DCELFace> auxList = new List<DCELFace>();
            //distanza corrente
            int distance = 1;
            //primo elemento loop
            int n = 0;

            foreach (var item in this.AdjacentFaces())
                visited.Add(item, distance);

            while (distance != k)
            {
                foreach (var item in visited.Keys.Skip(n))
                    auxList.Add(item);

                foreach (var current in auxList)
                {
                    foreach (var face in current.Neighbours())
                    {
                        if (!visited.Keys.Contains(face))
                            visited.Add(face, distance + 1);
                    }
                }
                distance += 1;
                n = auxList.Count;
                auxList.Clear();
            }

            foreach (var item in visited)
                if (item.Value == k)
                    auxList.Add(item.Key);

            return auxList;
        }
         */
        #endregion

    }

}
