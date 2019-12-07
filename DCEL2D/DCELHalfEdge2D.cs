using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL2D
{
    [Serializable]
    public sealed class DCELHalfEdge2D : Arrow
    {
        public DCELVertex2D Origin { get; set; }
        public DCELFace2D Face { get; set; }
        public DCELHalfEdge2D Twin { get; set; }
        public DCELHalfEdge2D Next { get; set; }

        public DCELHalfEdge2D() { }

        public DCELHalfEdge2D(DCELVertex2D origin, DCELFace2D face, DCELHalfEdge2D twin, DCELHalfEdge2D next)
        {
            Origin = origin;
            Face = face;
            Twin = twin;
            Next = next;
        }

        /// <summary>
        /// Verifica che tutti i riferimenti dell'oggetto non puntino a null.
        /// </summary>
        /// <returns>True se l'oggetto è consistente, false altrimenti.</returns>
        public bool IsConsistent()
        {
            if (Origin != null && Face != null && Twin != null && Next != null)
                return true;

            return false;
        }
        
        /// <summary>
        /// Restituisce l'HalfEdge precedente.
        /// </summary>
        /// <returns>DCELHalfEdge.</returns>
        public DCELHalfEdge2D Previous()
        {
            DCELHalfEdge2D he = this;

            do
            {
                he = he.Next;
            }
            while (he.Next.Origin != this.Origin);

            return he;
        }

    }

}
