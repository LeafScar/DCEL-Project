using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Threading.Tasks;
using DCEL;
using DCEL2D;

namespace DCELTestApp
{
    public class TestApp
    {
        //public delegate void Call();
        public DCELMesh2D mesh = new DCELMesh2D();
        //public DCELMesh mesh = DCELTools.LoadFromOFF(@"shapes/hexagon.off");

        public TestApp()
        {
            Stopwatch sw = new Stopwatch();

            mesh.AddVertex(new DCELVertex2D(5, 5, null));

            for (int i = 0; i < 1000000; i++)
                mesh.AddHalfEdge(new DCELHalfEdge2D(mesh.VertexList[0], null, null, null));

            sw.Start();
            var q1 = LINQueries.LeavingEdges(mesh, mesh.VertexList[0],
                LINQueries.QueryExecutionMode.ForEachLoop).ToList();
            sw.Stop();
            Console.WriteLine("Foreach loop: " + sw.Elapsed.TotalMilliseconds);

            sw.Restart();
            var q2 = LINQueries.LeavingEdges(mesh, mesh.VertexList[0],
                LINQueries.QueryExecutionMode.ForLoop).ToList();
            sw.Stop();
            Console.WriteLine("For loop: " + sw.Elapsed.TotalMilliseconds);

            sw.Restart();
            var q5 = LINQueries.LeavingEdges(mesh, mesh.VertexList[0],
                LINQueries.QueryExecutionMode.ForWithAssignment).ToList();
            sw.Stop();
            Console.WriteLine("For with assignment loop: " + sw.Elapsed.TotalMilliseconds);

            sw.Restart();
            var q3 = LINQueries.LeavingEdges(mesh, mesh.VertexList[0], 
                LINQueries.QueryExecutionMode.Default).ToList();
            sw.Stop();
            Console.WriteLine("Default: " + sw.Elapsed.TotalMilliseconds);
            
            sw.Restart();
            var q4 = LINQueries.LeavingEdges(mesh, mesh.VertexList[0], 
                LINQueries.QueryExecutionMode.AsParallel).ToList();
            sw.Stop();
            Console.WriteLine("As parallel: " + sw.Elapsed.TotalMilliseconds);

            Console.ReadLine();
        }
    }
}
