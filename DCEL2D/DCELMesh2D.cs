using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace DCEL2D
{
    [Serializable]
    public class DCELMesh2D
    {
        private int edgeCount = 0, faceCount = 0, vertexCount = 0;
        private List<DCELVertex2D> vertexList = new List<DCELVertex2D>();
        private List<DCELHalfEdge2D> edgeList = new List<DCELHalfEdge2D>();
        private List<DCELFace2D> faceList = new List<DCELFace2D>();
        private DCELFace2D dummyFace = new DCELFace2D(FaceType.IsInfinite);

        public DCELMesh2D() { }

        public DCELMesh2D(string filename)
        {
            DCELMesh2D mesh = Tools.LoadFromOFF(filename);

            this.vertexList = mesh.VertexList;
            this.vertexCount = mesh.VertexCount;
            this.edgeList = mesh.HalfEdgeList;
            this.edgeCount = mesh.HalfEdgeCount;
            this.faceList = mesh.FaceList;
            this.faceCount = mesh.FaceCount;
        }

        #region Get Methods

        public List<DCELVertex2D> VertexList
        {
            get { return vertexList; }
        }

        public List<DCELFace2D> FaceList
        {
            get { return faceList; }
        }

        public List<DCELHalfEdge2D> HalfEdgeList
        {
            get { return edgeList; }
        }

        public int VertexCount
        {
            get { return vertexCount; }
        }

        public int FaceCount
        {
            get { return faceCount; }
        }

        public int HalfEdgeCount
        {
            get { return edgeCount; }
        }

        #endregion

        #region Add Methods

        public bool AddVertex(DCELVertex2D vertex)
        {
            vertexList.Add(vertex);
            vertexCount++;

            return true;
        }

        public bool AddFace(DCELFace2D face)
        {
            faceList.Add(face);
            faceCount++;

            return true;
        }

        public bool AddHalfEdge(DCELHalfEdge2D edge)
        {
            edgeList.Add(edge);
            edgeCount++;

            return true;
        }

        #endregion

        #region Remove Methods

        public bool Clear()
        {
            vertexCount = faceCount = edgeCount = 0;
            vertexList.Clear();
            faceList.Clear();
            edgeList.Clear();

            return true;
        }

        public bool RemoveFace(DCELFace2D face)
        {
            if (faceList.Remove(face))
            {
                faceCount--;
                return true;
            }
            return false;
        }

        public bool RemoveHalfEdge(DCELHalfEdge2D edge)
        {
            if (edgeList.Remove(edge))
            {
                edgeCount--;
                return true;
            }
            return false;
        }

        public bool RemoveVertex(DCELVertex2D vertex)
        {
            if (vertexList.Remove(vertex))
            {
                vertexCount--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Rimuove dalla mesh tutti i vertici che non hanno alcun halfEdge in uscita.
        /// </summary>
        public void RemoveUnusedVertices()
        {
            for (int i = 0; i < vertexCount; i++)
                if (vertexList[i].Leaving == null)
                    this.RemoveVertex(vertexList[i]);
        }

        #endregion

        #region Queries

        public bool Contains(DCELVertex2D vertex)
        {
            return vertexList.Contains(vertex);
        }

        public bool Contains(DCELFace2D face)
        {
            return faceList.Contains(face);
        }

        public bool Contains(DCELHalfEdge2D edge)
        {
            return edgeList.Contains(edge);
        }

        /// <summary>
        /// Crea un nuovo Twin per ogni halfEdge di confine. Per ogni nuovo halfEdge creato viene impostata come
        /// faccia una faccia infinita e come next un riferimento al twin del successivo halfEdge di confine.
        /// </summary>
        public void ResolveBoundaryEdges()
        {
            Dictionary<DCELVertex2D, DCELHalfEdge2D> nextEdges =
                new Dictionary<DCELVertex2D, DCELHalfEdge2D>();

            var query = from edge in HalfEdgeList
                        where edge.Twin == null
                        select new DCELHalfEdge2D(edge.Next.Origin, dummyFace, edge, null);

            foreach (var item in query.ToList())
            {
                nextEdges.Add(item.Origin, item);
                item.Twin.Twin = item;
                this.AddHalfEdge(item);
            }

            foreach (var item in nextEdges.Values)
                item.Next = nextEdges[item.Twin.Origin];
        }

        /// <summary>
        /// Verifica che tutti i riferimenti dell'oggetto non puntino a null.
        /// </summary>
        /// <returns>True se l'oggetto è consistente, false altrimenti.</returns>
        public bool IsConsistent()
        {
            for (int i = 0; i < edgeCount; i++)
                if (edgeList[i].IsConsistent() == false)
                    return false;

            for (int i = 0; i < vertexCount; i++)
                if (vertexList[i].IsConsistent() == false)
                    return false;

            for (int i = 0; i < faceCount; i++)
                if (faceList[i].IsConsistent() == false)
                    return false;

            return true;
        }

        #endregion

        #region Triangulation

        public bool Triangulate()
        {
            //lista di appoggio
            List<DCELFace2D> toRemove = new List<DCELFace2D>();

            //aggiungo alla lista di appoggio solo le facce che hanno più di 3 lati
            foreach (var face in faceList)
            {
                if (face.Vertices().Count() > 3)
                    toRemove.Add(face);
            }

            if (toRemove.Count == 0)
                return false;

            //triangolazione delle facce
            foreach (var face in toRemove)
            {
                TriangulateFace(face);
            }

            return true;
        }

        public bool TriangulateFace(DCELFace2D face)
        {
            //lista di appoggio
            List<DCELHalfEdge2D> edges = face.Sides().ToList();
            //sides è il numero di lati della faccia
            int sides = edges.Count;
            //first è il vertice a partire dal quale viene suddiviso il poligono
            DCELVertex2D first = edges[0].Origin;

            //se la faccia non esiste o se esiste ma ha meno di 4 lati o non è convessa restituisco false
            //if (sides <= 3 || !this.Contains(face) || !face.IsConvex())
            //    return false;
            
            for (int i = 0; i < sides; i++)
            {
                if (i == 0)
                {
                    this.AddFace(new DCELFace2D(edges[i]));
                    edges[i].Face = faceList[FaceCount - 1];
                    i++;
                    edges[i].Face = faceList[FaceCount - 1];
                    this.AddHalfEdge(new DCELHalfEdge2D(edges[i].Next.Origin, faceList[FaceCount - 1], null, edges[0]));
                    edges[i].Next = edgeList[HalfEdgeCount - 1];
                }
                else if (i == sides - 2)
                {
                    this.AddFace(new DCELFace2D(null));
                    this.AddHalfEdge(new DCELHalfEdge2D(first, faceList[FaceCount - 1], edgeList[HalfEdgeCount - 1], edges[i]));
                    edges[i].Face = faceList[FaceCount - 1];
                    i++;
                    edges[i].Face = faceList[FaceCount - 1];                    
                    edgeList[HalfEdgeCount - 2].Twin = edgeList[HalfEdgeCount - 1];
                    faceList[FaceCount - 1].Edge = edgeList[HalfEdgeCount - 1];
                    edges[i].Next = edgeList[HalfEdgeCount - 1];
                }
                else
                {
                    this.AddFace(new DCELFace2D(null));
                    this.AddHalfEdge(new DCELHalfEdge2D(first, faceList[FaceCount - 1], edgeList[HalfEdgeCount - 1], edges[i]));
                    faceList[FaceCount - 1].Edge = edgeList[HalfEdgeCount - 1];
                    edges[i].Face = faceList[FaceCount - 1];
                    edgeList[HalfEdgeCount - 2].Twin = edgeList[HalfEdgeCount - 1];
                    this.AddHalfEdge(new DCELHalfEdge2D(edges[i].Next.Origin, faceList[faceCount - 1], null, edgeList[HalfEdgeCount - 1]));
                    edges[i].Next = edgeList[HalfEdgeCount - 1];
                }
            }
            //ora è possibile rimuovere la faccia dalla lista
            return RemoveFace(face);
        }

        #endregion
    }
}
