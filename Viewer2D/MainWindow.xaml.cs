using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Expression.Controls;
using Microsoft.Win32;
using DCEL2D;

namespace Viewer2D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point origin;
        private Point start;
        private Mesh2D mesh = new Mesh2D(@"shapes/hexagon.off", ViewMode.Geometry);
        public delegate void Call();

        public MainWindow()
        {
            InitializeComponent();
            InitializeMesh();

            geometryMenuItem.IsChecked = true;
            kTextBox.Text = mesh.K.ToString();
            DataObject.AddPastingHandler(kTextBox, OnCancelCommand);
            DataObject.AddCopyingHandler(kTextBox, OnCancelCommand);
            UpdateInfoLabel();
        }

        private void InitializeMesh()
        {
            mesh.InitializeMesh();

            if (mesh.MeshViewMode == ViewMode.Geometry)
                canvas.ItemsSource = mesh.GetGeometry(Brushes.LightBlue, Brushes.Black);
            else
                canvas.ItemsSource = mesh.UIElements;

            double scalePoint = mainWindow.Height / mesh.Center.Y;

            tt.X = Width / 2;
            tt.Y = Height / 2;
            xform.CenterX = mesh.Center.X;
            xform.CenterY = mesh.Center.Y;
            xform.ScaleX = scalePoint;
            xform.ScaleY = scalePoint;
        }

        #region Menu Events

        private void modify_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            switch (menuItem.Name)
            {
                case "resolveMenuItem":
                    Dispatcher.Invoke(
                        new Call(mesh.ResolveBoundaryEdges));
                    UpdateInfoLabel();
                    break;
                case "triang1MenuItem":
                    mesh.Triangulate();
                    if (mesh.MeshViewMode == ViewMode.Geometry)
                        canvas.ItemsSource = mesh.GetGeometry(Brushes.LightBlue, Brushes.Black);
                    UpdateInfoLabel();
                    break;
                default:
                    break;
            }
        }

        private void fileOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "Object File Format files (*.off)|*.off";

            // Show open file dialog box
            Nullable<bool> result = openfile.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = openfile.FileName;

                if (filename != null && File.Exists(filename))
                {
                    string ext = System.IO.Path.GetExtension(filename);

                    if (ext == ".off")
                    {
                        mesh = new Mesh2D(filename, ViewMode.Geometry);
                        geometryMenuItem.IsChecked = true;
                        InitializeMesh();
                    }
                    UpdateInfoLabel();
                }
                else
                {
                    MessageBox.Show("Errore durante l'apertura del file", "Errore!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            string message = "DCEL 2D Viewer rev 21.11.10\n\n\nAuthor: Slifer87";
            string caption = "About DCEL Viewer";

            MessageBox.Show(message, caption);
        }

        private void OnInfoChecked(object sender, RoutedEventArgs e)
        {
            meshInfo.Visibility = Visibility.Visible;
        }

        private void OnInfoUnchecked(object sender, RoutedEventArgs e)
        {
            meshInfo.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Mouse events

        private void border_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double zoom = e.Delta > 0 ? .5 : -.5;
            xform.ScaleX += zoom;
            xform.ScaleY += zoom;
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCoordLabel(e.GetPosition(canvas));
        }

        private void border_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateCoordLabel(e.GetPosition(canvas));

            if (!border.IsMouseCaptured) return;

            Vector v = start - e.GetPosition(border);
            tt.X = origin.X - v.X;
            tt.Y = origin.Y - v.Y;
        }

        private void border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            border.Cursor = Cursors.ScrollAll;
            border.CaptureMouse();
            start = e.GetPosition(border);
            origin = new Point(tt.X, tt.Y);
        }

        private void border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            border.Cursor = Cursors.Arrow;
            border.ReleaseMouseCapture();
        }

        private void border_MouseLeave(object sender, MouseEventArgs e)
        {
            border.ReleaseMouseCapture();
        }

        #endregion

        #region View Mode Events

        private void geometry_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.Geometry;

            canvas.ItemsSource = mesh.GetGeometry(Brushes.LightBlue, Brushes.Black);
        }

        private void geometry_Unchecked(object sender, RoutedEventArgs e)
        {
            canvas.ItemsSource = mesh.UIElements;
        }

        private void geometry_Click(object sender, RoutedEventArgs e)
        {
            geometryMenuItem.IsChecked = true;
        }

        private void dcelStructure_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.DCELStructure;
        }

        private void dcelStructure_Click(object sender, RoutedEventArgs e)
        {
            dcelStructureMenuItem.IsChecked = true;
        }

        private void faceNeighbours_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.FaceNeighbours;
        }

        private void faceNeighbours_Click(object sender, RoutedEventArgs e)
        {
            faceNeighboursMenuItem.IsChecked = true;
        }

        private void faceSides_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.FaceSides;
        }

        private void faceSides_Click(object sender, RoutedEventArgs e)
        {
            faceSidesMenuItem.IsChecked = true;
        }

        private void faceVertices_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.FaceVertices;
        }

        private void faceVertices_Click(object sender, RoutedEventArgs e)
        {
            faceVerticesMenuItem.IsChecked = true;
        }

        private void leavingEdges_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.LeavingEdges;
        }

        private void leavingEdges_Click(object sender, RoutedEventArgs e)
        {
            leavingEdgesMenuItem.IsChecked = true;
        }

        private void adjacentFaces_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.AdjacentFaces;
        }

        private void adjacentFaces_Click(object sender, RoutedEventArgs e)
        {
            adjacentFacesMenuItem.IsChecked = true;
        }

        private void adjacentVertices_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.AdjacentVertices;
        }

        private void adjacentVertices_Click(object sender, RoutedEventArgs e)
        {
            adjacentVerticesMenuItem.IsChecked = true;
        }

        private void kStar_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.KStar;

            SetKVisibility(Visibility.Visible);
        }

        private void kStar_Click(object sender, RoutedEventArgs e)
        {
            kStarMenuItem.IsChecked = true;

            SetKVisibility(Visibility.Visible);
        }

        private void kStarMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            SetKVisibility(Visibility.Hidden);
        }

        private void navigation_Click(object sender, RoutedEventArgs e)
        {
            navigationMenuItem.IsChecked = true;
        }

        private void navigation_Checked(object sender, RoutedEventArgs e)
        {
            mesh.MeshViewMode = ViewMode.Navigation;

            grid1.Visibility = Visibility.Visible;
        }

        private void navigation_Unchecked(object sender, RoutedEventArgs e)
        {
            grid1.Visibility = Visibility.Hidden;

            mesh.UpdateEdge(mesh.CurrentSelected);
        }

        #endregion

        #region Private

        private void UpdateCoordLabel(Point currentPos)
        {
            coordLabel.Content = "X: " + currentPos.X.ToString("N1") + " Y: " + currentPos.Y.ToString("N1");
        }
        
        private void kTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (kTextBox.Text == "" || kTextBox.Text[0] == ' ')
            {
                kTextBox.Text = "";
                return;
            }

            mesh.K = Convert.ToInt32(kTextBox.Text);
        }

        private void UpdateInfoLabel()
        {
            meshInfo.Content =
                "Vertex count: " + mesh.VertexCount +
                "\nFace count: " + mesh.FaceCount +
                "\nHalf-Edge count: " + mesh.HalfEdgeCount +
                "\nIs consistent: " + mesh.IsConsistent().ToString();
        }

        private void OnCancelCommand(object sender, DataObjectEventArgs e)
        {
            e.CancelCommand();
        }

        private void SetKVisibility(Visibility v)
        {
            kTextBox.Visibility = v;
            kLabel.Visibility = v;
        }

        private void kTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return new Regex("[0-9]").IsMatch(text);
        }
        
        #endregion

        #region Buttons Events

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (mesh.CurrentSelected == null)
                return;

            mesh.UpdateEdge(mesh.CurrentSelected.Next);
        }

        private void previousButton_Click(object sender, RoutedEventArgs e)
        {
            if (mesh.CurrentSelected == null)
                return;

            mesh.UpdateEdge(mesh.CurrentSelected.Previous());
        }

        private void twinButton_Click(object sender, RoutedEventArgs e)
        {
            if (mesh.CurrentSelected == null)
                return;

            mesh.UpdateEdge(mesh.CurrentSelected.Twin);
        }

        #endregion
    }

    public enum ViewMode
    {
        Geometry,
        Wireframe,
        Navigation,
        DCELStructure,
        FaceNeighbours,
        FaceSides,
        FaceVertices,
        LeavingEdges,
        AdjacentFaces,
        AdjacentVertices,
        KDistanceVertices,
        KStar
    }

}
