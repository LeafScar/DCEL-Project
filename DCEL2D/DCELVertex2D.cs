using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace DCEL2D
{
    [Serializable]
    public sealed class DCELVertex2D : Shape
    {
        public Point Coordinates { get; set; }
        public DCELHalfEdge2D Leaving { get; set; }

        #region Constructors

        public DCELVertex2D() { }

        public DCELVertex2D(double x, double y, DCELHalfEdge2D leaving)
        {
            Coordinates = new Point(x, y);
            Leaving = leaving;
        }

        public DCELVertex2D(Point coordinates, DCELHalfEdge2D leaving)
        {
            Coordinates = coordinates;
            Leaving = leaving;
        }

        #endregion

        /// <summary>
        /// Verifica che tutti i riferimenti dell'oggetto non puntino a null.
        /// </summary>
        /// <returns>True se l'oggetto è consistente, false altrimenti.</returns>
        public bool IsConsistent()
        {
            if (Coordinates != null && Leaving != null)
                return true;

            return false;
        }
        
        /// <summary>
        /// Restituisce tutti gli HalfEdge che partono da questo vertice.
        /// </summary>
        /// <returns>DCELHalfEdge collection.</returns>
        public IEnumerable<DCELHalfEdge2D> LeavingEdges()
        {
            if (this.Leaving == null)
                yield break;

            DCELHalfEdge2D he = this.Leaving;
            
            do
            {
                yield return he;
                he = he.Previous().Twin;
                if (he == null)
                    yield break;
            }
            while (he != this.Leaving);
        }

        /// <summary>
        /// Restituisce tutte le facce adiacenti a questo vertice.
        /// </summary>
        /// <returns>DCELFace collection.</returns>
        public IEnumerable<DCELFace2D> AdjacentFaces()
        {
            foreach (var edge in LeavingEdges())
                if (!edge.Face.IsInfinite())
                    yield return edge.Face;
        }

        /// <summary>
        /// Restituisce tutti i vertici che si trovano a distanza 1.
        /// </summary>
        /// <returns>DCELVertex collection.</returns>
        public IEnumerable<DCELVertex2D> AdjacentVertices()
        {
            foreach (var edge in LeavingEdges())
                yield return edge.Next.Origin;
        }
        
        /// <summary>
        /// Restituisce tutti i vertici che si trovano a distanza k.
        /// </summary>
        /// <param name="k">Distanza k dal vertice.</param>
        /// <returns>DCELVertex collection</returns>
        public IEnumerable<DCELVertex2D> KDistanceVertices(int k)
        {
            if (k < 0)
                yield break;

            //lista vertici visitati
            List<DCELVertex2D> visited = new List<DCELVertex2D>();
            //lista di appoggio
            List<DCELVertex2D> auxList = new List<DCELVertex2D>();
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
        public IEnumerable<DCELFace2D> KStar(int k)
        {
            if (k <= 0)
                yield break;
            
            //lista visite
            List<DCELFace2D> visited = new List<DCELFace2D>();
            //lista di appoggio
            List<DCELFace2D> auxList = new List<DCELFace2D>();
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

        protected override Geometry DefiningGeometry
        {
            get { return new EllipseGeometry(Coordinates, .20, .20); }
        }
    }
}
