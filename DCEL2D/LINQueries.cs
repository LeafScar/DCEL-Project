using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCEL2D;
using Brahma.DirectX;

namespace DCEL2D
{
    public static class LINQueries
    {
        public static IEnumerable<DCELHalfEdge2D> LeavingEdges(DCELMesh2D mesh, DCELVertex2D vertex, QueryExecutionMode executionMode)
        {
            var query = mesh.HalfEdgeList.AsEnumerable();

            switch (executionMode)
            {
                case QueryExecutionMode.Default:
                    query = query.Where(x => x.Origin == vertex);
                    break;
                case QueryExecutionMode.AsParallel:
                    query = query.AsParallel().Where(x => x.Origin == vertex);
                    break;
                case QueryExecutionMode.ForEachLoop:
                    foreach (var leavingEdge in mesh.HalfEdgeList)
                        if (leavingEdge.Origin == vertex)
                            yield return leavingEdge;
                    yield break;
                case QueryExecutionMode.ForLoop:
                    for (int i = 0; i < mesh.HalfEdgeCount; i++)
                        if (mesh.HalfEdgeList[i].Origin == vertex)
                            yield return mesh.HalfEdgeList[i];
                    yield break;
                case QueryExecutionMode.ForWithAssignment:
                    int n = mesh.HalfEdgeCount;
                    for (int i = 0; i < n; i++)
                        if (mesh.HalfEdgeList[i].Origin == vertex)
                            yield return mesh.HalfEdgeList[i];
                    yield break;
            }

            foreach (var leavingEdge in query)
                yield return leavingEdge;
        }

        public enum QueryExecutionMode
        {
            Default,
            AsParallel,
            ForEachLoop,
            ForLoop,
            ForWithAssignment
        }
    }
}
