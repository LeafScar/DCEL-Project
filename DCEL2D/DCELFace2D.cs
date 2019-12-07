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
    public sealed class DCELFace2D : Shape
    {
        /// <summary>
        /// Rappresenta un singolo HalfEdge che ha questa faccia come sua faccia.
        /// </summary>
        private DCELHalfEdge2D edge;
        /// <summary>
        /// Definisce la tipologia della faccia.
        /// </summary>
        private FaceType faceType;
        /// <summary>
        /// Rappresenta le coordinate del centro della faccia.
        /// </summary>
        private Point center;

        #region Constructors

        /// <summary>
        /// Crea una nuova istanza di DCELFace non infinita.
        /// </summary>
        public DCELFace2D()
        {
            this.faceType = FaceType.IsIterable;
        }

        /// <summary>
        /// Crea una nuova istanza di DCELFace infinita o non infinita.
        /// </summary>
        /// <param name="faceType">Il tipo di faccia.</param>
        public DCELFace2D(FaceType faceType)
        {
            this.faceType = faceType;
        }

        /// <summary>
        /// Crea una nuova istanza di DCELFace non infinita.
        /// </summary>
        /// <param name="edge">Un singolo HalfEdge che ha questa faccia come sua faccia.</param>
        /// <param name="normal">La normale della faccia.</param>
        public DCELFace2D(DCELHalfEdge2D edge) : this()
        {
            this.edge = edge;
        }

        #endregion

        public DCELHalfEdge2D Edge
        {
            get { return edge; }
            set
            {
                if (!this.IsInfinite())
                    edge = value;
            }
        }

        public Point Center
        {
            get { return center; }
        }

        /// <summary>
        /// Verifica che tutti i riferimenti dell'oggetto non puntino a null.
        /// </summary>
        /// <returns>True se l'oggetto è consistente, false altrimenti.</returns>
        public bool IsConsistent()
        {
            if (faceType == FaceType.IsInfinite)
                return true;
            if (Edge != null)
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
            DCELHalfEdge2D he = this.Edge;
            DCELHalfEdge2D prev = he.Previous();
            DCELVertex2D first = he.Origin;

            do
            {
                Vector v1 = new Vector(
                    prev.Origin.Coordinates.X - he.Origin.Coordinates.X,
                    prev.Origin.Coordinates.Y - he.Origin.Coordinates.Y);

                Vector v2 = new Vector(
                    he.Next.Origin.Coordinates.X - he.Origin.Coordinates.X,
                    he.Next.Origin.Coordinates.Y - he.Origin.Coordinates.Y);

                double dotProduct = (v1.X * v2.X + v1.Y * v2.Y);
                double norm1 = Math.Sqrt(Math.Pow(v1.X, 2) + Math.Pow(v1.Y, 2));
                double norm2 = Math.Sqrt(Math.Pow(v2.X, 2) + Math.Pow(v2.Y, 2));
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

        public void SetCenter()
        {
            center = new Point(0, 0);
            int count = 0;

            foreach (var item in this.Vertices())
            {
                center.X += item.Coordinates.X;
                center.Y += item.Coordinates.Y;
                count++;
            }

            center.X /= count;
            center.Y /= count;
        }
        

        /// <summary>
        /// Restituisce tutte le facce confinanti.
        /// </summary>
        /// <returns>DCELFace collection.</returns>
        public IEnumerable<DCELFace2D> Neighbours()
        {
            DCELHalfEdge2D he = this.Edge;
            DCELVertex2D first = he.Origin;

            do
            {
                if (he.Twin != null)
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
        public IEnumerable<DCELVertex2D> Vertices()
        {
            DCELHalfEdge2D he = this.Edge;
            DCELVertex2D first = he.Origin;

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
        public IEnumerable<DCELHalfEdge2D> Sides()
        {
            DCELHalfEdge2D he = this.Edge;
            DCELVertex2D first = he.Origin;

            do
            {
                yield return he;
                he = he.Next;
            }
            while (he.Origin != first);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                PathFigure pathFigure = new PathFigure();
                pathFigure.IsClosed = true;
                bool first = true;

                foreach (var v in this.Vertices())
                {
                    if (first)
                    {
                        pathFigure.StartPoint = v.Coordinates;
                        first = false;
                    }
                    else
                        pathFigure.Segments.Add(new LineSegment(v.Coordinates, false));
                }

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                return pathGeometry;
            }
        }

    }

    public enum FaceType
    {
        IsInfinite,
        IsIterable
    }
}
