using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace DCEL
{
    [Serializable]
    public class DCELFace
    {
        /// <summary>
        /// Rappresenta un singolo HalfEdge che ha questa faccia come sua faccia.
        /// </summary>
        private DCELHalfEdge edge;
        /// <summary>
        /// La normale della faccia.
        /// </summary>
        private Vector3D normal;
        /// <summary>
        /// Definisce la tipologia della faccia.
        /// </summary>
        private FaceType faceType;

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di DCELFace non infinita.
        /// </summary>
        public DCELFace()
        {
            this.faceType = FaceType.IsIterable;
        }

        /// <summary>
        /// Crea una nuova istanza di DCELFace infinita o non infinita.
        /// </summary>
        /// <param name="faceType">Il tipo di faccia.</param>
        public DCELFace(FaceType faceType)
        {
            this.faceType = faceType;
        }

        /// <summary>
        /// Crea una nuova istanza di DCELFace non infinita.
        /// </summary>
        /// <param name="edge">Un singolo HalfEdge che ha questa faccia come sua faccia.</param>
        /// <param name="normal">La normale della faccia.</param>
        public DCELFace(DCELHalfEdge edge, Vector3D normal) : this()
        {
            this.edge = edge;
            this.normal = normal;
        }

        #endregion

        /// <summary>
        /// Chissà a cosa serve questo metodo?
        /// </summary>
        public DCELHalfEdge Edge
        {
            get { return edge; }
            set
            {
                if (!this.IsInfinite())
                    edge = value;
            }
        }

        public Vector3D Normal
        {
            get { return normal; }
            set
            {
                if (!this.IsInfinite())
                    normal = value;
            }
        }

        /// <summary>
        /// Verifica che tutti i riferimenti dell'oggetto non puntino a null.
        /// </summary>
        /// <returns>True se l'oggetto è consistente, false altrimenti.</returns>
        public bool IsConsistent()
        {
            if (faceType == FaceType.IsInfinite)
                return true;
            if (Edge != null && Normal != null)
                return true;

            return false;
        }

        /// <summary>
        /// Verifica se la faccia è infinita. Una faccia definita infinita è non attraversabile.
        /// </summary>
        /// <returns>True se la faccia è infinita, false altrimenti.</returns>
        public bool IsInfinite()
        {
            if (faceType == FaceType.IsInfinite)
                return true;

            return false;
        }

        /// <summary>
        /// Verifica se la faccia è convessa. Un poligono semplice è strettamente convesso se ogni
        /// angolo interno è strettamente inferiore a 180 gradi.
        /// </summary>
        /// <returns>True se la faccia è convessa, false altrimenti.</returns>
        public bool IsConvex()
        {
            DCELHalfEdge he = this.Edge;
            DCELHalfEdge prev = he.Previous();
            DCELVertex first = he.Origin;

            do
            {
                Vector3D v1 = new Vector3D(
                    prev.Origin.Coordinates.X - he.Origin.Coordinates.X,
                    prev.Origin.Coordinates.Y - he.Origin.Coordinates.Y,
                    prev.Origin.Coordinates.Z - he.Origin.Coordinates.Z);

                Vector3D v2 = new Vector3D(
                    he.Next.Origin.Coordinates.X - he.Origin.Coordinates.X,
                    he.Next.Origin.Coordinates.Y - he.Origin.Coordinates.Y,
                    he.Next.Origin.Coordinates.Z - he.Origin.Coordinates.Z);

                double dotProduct = (v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z);
                double norm1 = Math.Sqrt(Math.Pow(v1.X, 2) + Math.Pow(v1.Y, 2) + Math.Pow(v1.Z, 2));
                double norm2 = Math.Sqrt(Math.Pow(v2.X, 2) + Math.Pow(v2.Y, 2) + Math.Pow(v2.Z, 2));
                double angle = (Math.Acos(dotProduct / (norm1 * norm2)) * 180) / Math.PI;

                if (angle > 180)
                    return false;

                prev = he;
                he = he.Next;
            }
            while (he.Origin != first);

            //returns true if every internal angle is less than or equal to 180 degrees
            return true;
        }

        /// <summary>
        /// Restituisce tutte le facce confinanti.
        /// </summary>
        /// <returns>DCELFace collection.</returns>
        public IEnumerable<DCELFace> Neighbours()
        {
            DCELHalfEdge he = this.Edge;
            DCELVertex first = he.Origin;

            do
            {
                if (!he.Twin.Face.IsInfinite())
                    yield return he.Twin.Face;
                he = he.Next;
            }
            while (he.Origin != first);
        }

        /// <summary>
        /// Restituisce i vertici della faccia.
        /// </summary>
        /// <returns>DCELVertex collection.</returns>
        public IEnumerable<DCELVertex> Vertices()
        {
            DCELHalfEdge he = this.Edge;
            DCELVertex first = he.Origin;

            do
            {
                yield return he.Origin;
                he = he.Next;
            }
            while (he.Origin != first);
        }

        /// <summary>
        /// Restituisce i lati della faccia.
        /// </summary>
        /// <returns>DCELHalfEdge collection.</returns>
        public IEnumerable<DCELHalfEdge> Sides()
        {
            DCELHalfEdge he = this.Edge;
            DCELVertex first = he.Origin;

            do
            {
                yield return he;
                he = he.Next;
            }
            while (he.Origin != first);
        }

        /// <summary>
        /// Calcola la normale della faccia.
        /// </summary>
        /// <returns></returns>
        public void CalculateNormal()
        {
            MathUtils.CalculateNormal(this);
        }

        #region Deprecated
        /*
        /// <summary>
        /// Restituisce una lista contenente tutte le facce confinanti.
        /// </summary>
        /// <returns>DCELFace collection.</returns>
        public List<DCELFace> Neighbours()
        {
            List<DCELFace> neighbours = new List<DCELFace>();
            DCELHalfEdge he = this.Edge;
            DCELVertex first = he.Origin;

            do
            {
                if (!he.Twin.Face.IsInfinite())
                    neighbours.Add(he.Twin.Face);
                he = he.Next;
            }
            while (he.Origin != first);

            return neighbours;
        }
        */
        /*
        /// <summary>
        /// Restituisce una lista contenente i vertici della faccia.
        /// </summary>
        /// <returns>DCELVertex collection.</returns>
        public List<DCELVertex> Vertices()
        {
            List<DCELVertex> vertices = new List<DCELVertex>();
            DCELHalfEdge he = this.Edge;
            DCELVertex first = he.Origin;

            do
            {
                vertices.Add(he.Origin);
                he = he.Next;
            }
            while (he.Origin != first);

            return vertices;
        }
        */
        /*
        /// <summary>
        /// Restituisce una lista contenente i lati della faccia.
        /// </summary>
        /// <returns>DCELHalfEdge collection.</returns>
        public List<DCELHalfEdge> Sides()
        {
            List<DCELHalfEdge> edges = new List<DCELHalfEdge>();
            DCELHalfEdge he = this.Edge;
            DCELVertex first = he.Origin;

            do
            {
                edges.Add(he);
                he = he.Next;
            }
            while (he.Origin != first);

            return edges;
        }
        */
        #endregion

    }

    public enum FaceType
    {
        IsInfinite,
        IsIterable
    }
}
