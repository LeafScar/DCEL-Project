using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEL
{
    [Serializable]
    public class DCELHalfEdge
    {
        public DCELVertex Origin { get; set; }
        public DCELFace Face { get; set; }
        public DCELHalfEdge Twin { get; set; }
        public DCELHalfEdge Next { get; set; }

        public DCELHalfEdge() { }

        public DCELHalfEdge(DCELVertex origin, DCELFace face, DCELHalfEdge twin, DCELHalfEdge next)
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
        public DCELHalfEdge Previous()
        {
            DCELHalfEdge he = this;

            do
            {
                he = he.Next;
            }
            while (he.Next.Origin != this.Origin);

            return he;
        }

    }

}
