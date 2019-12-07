using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Expression.Controls;
using DCEL2D;


namespace Viewer2D
{    
    public class Mesh2D : DCELMesh2D
    {
        const double distance = .1;
        int kDistance = 1;
        ObservableCollection<UIElement> collection = new ObservableCollection<UIElement>();
        List<UIElement> buffer = new List<UIElement>();
        ViewMode viewMode;
        Point center = new Point();
        DCELHalfEdge2D currentSelected = null;

        #region Constructors
        public Mesh2D(string filename)
            : base(filename) { }

        public Mesh2D(string filename, ViewMode viewMode)
            : base(filename)
        {
            this.viewMode = viewMode;
        }
        #endregion

        #region Properties

        public ObservableCollection<UIElement> UIElements
        {
            get { return collection; }
        }

        public ViewMode MeshViewMode
        {
            get { return viewMode; }
            set { viewMode = value; }
        }

        public int K
        {
            get { return kDistance; }
            set { kDistance = value; }
        }

        public Point Center
        {
            get { return center; }
        }

        public DCELHalfEdge2D CurrentSelected
        {
            get { return currentSelected; }
        }

        #endregion

        #region Overrides

        public new void ResolveBoundaryEdges()
        {
            int count = HalfEdgeCount;

            base.ResolveBoundaryEdges();

            foreach (var he in HalfEdgeList.Skip(count))
            {
                InitializeEdge(he);
            }
        }

        public new void Triangulate()
        {
            if (base.Triangulate())
            {
                collection.Clear();
                InitializeMesh();
            }
        }

        #endregion

        #region UIElements Initialization
        public void InitializeEdge(DCELHalfEdge2D edge)
        {
            TranslateCoords(edge);

            edge.HeadHeight = 0.4;
            edge.HeadWidth = 1;
            edge.Stroke = Brushes.Black;
            edge.StrokeThickness = 0.1;

            double d = Math.Sqrt(Math.Pow(edge.X2 - edge.X1, 2) + Math.Pow(edge.Y2 - edge.Y1, 2));
            double r = (d - .75) / d;

            edge.RenderTransform = new ScaleTransform(r, r, (edge.X2 + edge.X1) / 2, (edge.Y2 + edge.Y1) / 2);

            edge.MouseEnter += new MouseEventHandler(edge_MouseEnter);
            edge.MouseLeave += new MouseEventHandler(edge_MouseLeave);
            edge.MouseDown += new MouseButtonEventHandler(edge_MouseDown);

            collection.Add(edge);
        }

        public void InitializeVertex(DCELVertex2D vertex)
        {
            vertex.Fill = Brushes.Black;

            vertex.MouseEnter += new MouseEventHandler(origin_MouseEnter);
            vertex.MouseLeave += new MouseEventHandler(origin_MouseLeave);

            collection.Add(vertex);
        }

        public void InitializeFace(DCELFace2D face)
        {
            face.SetCenter();
            face.Fill = Brushes.Black;
            face.RenderTransform = new ScaleTransform(0.5, 0.5, face.Center.X, face.Center.Y);

            face.MouseEnter += new MouseEventHandler(face_MouseEnter);
            face.MouseLeave += new MouseEventHandler(face_MouseLeave);
            
            collection.Add(face);
        }

        public void InitializeMesh()
        {
            foreach (var vertex in VertexList)
                InitializeVertex(vertex);

            foreach (var edge in HalfEdgeList)
                InitializeEdge(edge);

            foreach (var face in FaceList)
                InitializeFace(face);

            SetCenter();
        }
        #endregion

        public void UpdateEdge(DCELHalfEdge2D edge)
        {
            if (edge == null)
                return;

            if (edge != currentSelected)
            {
                if (currentSelected != null)
                {
                    currentSelected.Stroke = Brushes.Black;
                    if (currentSelected.Face != null)
                        currentSelected.Face.Fill = Brushes.Black;
                    currentSelected.Origin.Fill = Brushes.Black;
                }

                edge.Stroke = Brushes.Green;
                if (edge.Face != null)
                    edge.Face.Fill = Brushes.LightBlue;
                edge.Origin.Fill = Brushes.Blue;

                currentSelected = edge;
            }
            else
            {
                if (currentSelected != null)
                {
                    currentSelected.Stroke = Brushes.Black;
                    if (currentSelected.Face != null)
                        currentSelected.Face.Fill = Brushes.Black;
                    currentSelected.Origin.Fill = Brushes.Black;
                }

                currentSelected = null;
            }
        }

        public ObservableCollection<UIElement> GetGeometry(Brush fillColor, Brush strokeColor)
        {
            ObservableCollection<UIElement> geometry = new ObservableCollection<UIElement>();
            PointCollection myPointCollection;
            Polygon myPolygon;

            foreach (var face in FaceList)
            {
                myPointCollection = new PointCollection();
                myPolygon = new Polygon();

                foreach (var vertex in face.Vertices())
                    myPointCollection.Add(vertex.Coordinates);

                myPolygon.Points = myPointCollection;
                myPolygon.Fill = fillColor;
                myPolygon.Stroke = strokeColor;
                myPolygon.StrokeLineJoin = PenLineJoin.Round;
                myPolygon.StrokeThickness = .05;

                geometry.Add(myPolygon);
            }

            return geometry;
        }

        #region Private

        private void TranslateCoords(DCELHalfEdge2D edge)
        {
            double theta = Math.Atan2(
                edge.Origin.Coordinates.Y - edge.Next.Origin.Coordinates.Y,
                edge.Origin.Coordinates.X - edge.Next.Origin.Coordinates.X);
            double sint = Math.Sin(theta);
            double cost = Math.Cos(theta);

            edge.X1 = edge.Origin.Coordinates.X - (distance * sint);
            edge.Y1 = edge.Origin.Coordinates.Y + (distance * cost);
            edge.X2 = edge.Next.Origin.Coordinates.X + (-distance * sint);
            edge.Y2 = edge.Next.Origin.Coordinates.Y + (distance * cost);
        }
        
        public void SetCenter()
        {
            var query = from face in FaceList
                        select face.Center;

            foreach (var origin in query)
            {
                center.X += origin.X;
                center.Y += origin.Y;
            }

            center.X /= FaceCount;
            center.Y /= FaceCount;
        }

        #endregion

        #region Events

        void edge_MouseLeave(object sender, MouseEventArgs e)
        {
            var edge = sender as DCEL2D.DCELHalfEdge2D;

            if (MeshViewMode == ViewMode.DCELStructure)
            {
                edge.Stroke = Brushes.Black;
                edge.Next.Stroke = Brushes.Black;
                if (edge.Twin != null)
                    edge.Twin.Stroke = Brushes.Black;
                if (edge.Face != null)
                    edge.Face.Fill = Brushes.Black;
                edge.Origin.Fill = Brushes.Black;
            }
        }

        void edge_MouseEnter(object sender, MouseEventArgs e)
        {
            var edge = sender as DCEL2D.DCELHalfEdge2D;

            switch (MeshViewMode)
            {
                case ViewMode.Geometry:
                    break;
                case ViewMode.Wireframe:
                    break;
                case ViewMode.DCELStructure:
                    edge.Stroke = Brushes.Green;
                    edge.Next.Stroke = Brushes.Orange;
                    if (edge.Twin != null)
                        edge.Twin.Stroke = Brushes.Red;
                    if (edge.Face != null)
                        edge.Face.Fill = Brushes.LightBlue;
                    edge.Origin.Fill = Brushes.Blue;
                    break;
                default:
                    break;
            }
        }

        void face_MouseLeave(object sender, MouseEventArgs e)
        {
            var face = sender as DCEL2D.DCELFace2D;

            switch (MeshViewMode)
            {
                case ViewMode.Geometry:
                    break;
                case ViewMode.Wireframe:
                    break;
                case ViewMode.DCELStructure:
                    face.Fill = Brushes.Black;
                    face.Edge.Stroke = Brushes.Black;
                    break;
                case ViewMode.FaceNeighbours:
                    face.Fill = Brushes.Black;
                    foreach (var item in buffer)
                    {
                        DCELFace2D f = item as DCELFace2D;
                        f.Fill = Brushes.Black;
                    }
                    buffer.Clear();
                    break;
                case ViewMode.FaceSides:
                    face.Fill = Brushes.Black;
                    foreach (var item in buffer)
                    {
                        DCELHalfEdge2D f = item as DCELHalfEdge2D;
                        f.Stroke = Brushes.Black;
                    }
                    buffer.Clear();
                    break;
                case ViewMode.FaceVertices:
                    face.Fill = Brushes.Black;
                    foreach (var item in buffer)
                    {
                        DCELVertex2D v = item as DCELVertex2D;
                        v.Fill = Brushes.Black;
                    }
                    buffer.Clear();
                    break;
                default:
                    break;
            }
        }

        void face_MouseEnter(object sender, MouseEventArgs e)
        {
            var face = sender as DCEL2D.DCELFace2D;

            switch (MeshViewMode)
            {
                case ViewMode.Geometry:
                    break;
                case ViewMode.Wireframe:
                    break;
                case ViewMode.DCELStructure:
                    face.Fill = Brushes.LightBlue;
                    face.Edge.Stroke = Brushes.Green;
                    break;
                case ViewMode.FaceNeighbours:
                    face.Fill = Brushes.LightBlue;
                    foreach (var item in face.Neighbours())
                    {
                        buffer.Add(item);
                        item.Fill = Brushes.Green;
                    }
                    break;
                case ViewMode.FaceSides:
                    face.Fill = Brushes.LightBlue;
                    foreach (var item in face.Sides())
                    {
                        buffer.Add(item);
                        item.Stroke = Brushes.Green;
                    }
                    break;
                case ViewMode.FaceVertices:
                    face.Fill = Brushes.LightBlue;
                    foreach (var item in face.Vertices())
                    {
                        buffer.Add(item);
                        item.Fill = Brushes.Orange;
                    }
                    break;
                default:
                    break;
            }
        }

        void origin_MouseLeave(object sender, MouseEventArgs e)
        {
            var origin = sender as DCEL2D.DCELVertex2D;

            switch (MeshViewMode)
            {
                case ViewMode.Geometry:
                    break;
                case ViewMode.Wireframe:
                    break;
                case ViewMode.DCELStructure:
                    origin.Fill = Brushes.Black;
                    origin.Leaving.Stroke = Brushes.Black;
                    break;
                case ViewMode.LeavingEdges:
                    origin.Fill = Brushes.Black;
                    foreach (var item in buffer)
                    {
                        DCELHalfEdge2D he = item as DCELHalfEdge2D;
                        he.Stroke = Brushes.Black;
                    }
                    buffer.Clear();
                    break;
                case ViewMode.AdjacentFaces:
                    origin.Fill = Brushes.Black;
                    foreach (var item in buffer)
                    {
                        DCELFace2D f = item as DCELFace2D;
                        f.Fill = Brushes.Black;
                    }
                    buffer.Clear();
                    break;
                case ViewMode.AdjacentVertices:
                    origin.Fill = Brushes.Black;
                    foreach (var item in buffer)
                    {
                        DCELVertex2D v = item as DCELVertex2D;
                        v.Fill = Brushes.Black;
                    }
                    buffer.Clear();
                    break;
                case ViewMode.KStar:
                    origin.Fill = Brushes.Black;
                    foreach (var item in buffer)
                    {
                        DCELFace2D f = item as DCELFace2D;
                        f.Fill = Brushes.Black;
                    }
                    buffer.Clear();
                    break;
                default:
                    break;
            }           
        }

        void origin_MouseEnter(object sender, MouseEventArgs e)
        {
            var origin = sender as DCEL2D.DCELVertex2D;

            switch (MeshViewMode)
            {
                case ViewMode.Geometry:
                    break;
                case ViewMode.Wireframe:
                    break;
                case ViewMode.DCELStructure:
                    origin.Fill = Brushes.Blue;
                    origin.Leaving.Stroke = Brushes.Green;
                    break;
                case ViewMode.LeavingEdges:                    
                    origin.Fill = Brushes.Blue;
                    foreach (var item in LINQueries.LeavingEdges(this, origin, LINQueries.QueryExecutionMode.Default))
                    {
                        buffer.Add(item);
                        item.Stroke = Brushes.Green;
                    }
                    break;
                case ViewMode.AdjacentFaces:
                    origin.Fill = Brushes.Blue;
                    foreach (var item in origin.AdjacentFaces())
                    {
                        buffer.Add(item);
                        item.Fill = Brushes.Green;
                    }
                    break;
                case ViewMode.AdjacentVertices:
                    origin.Fill = Brushes.Blue;
                    foreach (var item in origin.AdjacentVertices())
                    {
                        buffer.Add(item);
                        item.Fill = Brushes.Orange;
                    }
                    break;
                case ViewMode.KStar:
                    origin.Fill = Brushes.Blue;
                    foreach (var item in origin.KStar(K))
                    {
                        buffer.Add(item);
                        item.Fill = Brushes.Green;
                    }
                    break;
                default:
                    break;
            }
        }

        void edge_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MeshViewMode != ViewMode.Navigation)
                return;

            DCELHalfEdge2D edge = sender as DCELHalfEdge2D;

            UpdateEdge(edge);
        }

        #endregion
    }
}
