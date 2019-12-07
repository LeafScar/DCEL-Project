using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
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
using System.Windows.Media.Media3D;
using System.IO;
using Microsoft.Win32;
using DCEL;

namespace DCELViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DCELMesh mesh;
        private GeometryModel3D gModel3d;
        private Stopwatch sw = new Stopwatch();
        private StreamWriter SW = File.CreateText(@"viewer.log");
        private bool mDown;
        private Point mLastPos;

        public MainWindow()
        {
            InitializeComponent();

            SW.WriteLine("#Log");

            sw.Start();
            mesh = DCELTools.LoadFromOFF(@"shapes/cube.off");
            sw.Stop();
            SW.WriteLine("Dcel loading from off: " + sw.Elapsed.TotalMilliseconds);
            SW.Close();
            
            Create3dMesh();
            MeshInfo();
        }

        private void Create3dMesh()
        {
            ModelVisual3D model3d = new ModelVisual3D();
            SW = File.AppendText(@"viewer.log");

            sw.Restart();
            MeshGeometry3D mesh3d = DCELTools.GetMeshGeometry(mesh);
            sw.Stop();
            SW.WriteLine("3d mesh creation: " + sw.Elapsed.TotalMilliseconds);

            gModel3d = new GeometryModel3D(mesh3d, new DiffuseMaterial(Brushes.LightSteelBlue));
            gModel3d.BackMaterial = new DiffuseMaterial(Brushes.LightSteelBlue);
            gModel3d.Transform = new Transform3DGroup();
            model3d.Content = gModel3d;
            
            //cancello tutti gli elementi presenti sul viewport e aggiungo il nuovo modello 3d
            ClearViewport();
            viewport3d.Children.Add(model3d);

            SW.Close();
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = 
                "3D models (*.off; *.dcel)|*.off;*.dcel|" + 
                "Object File Format files (*.off)|*.off|" + 
                "DCEL serialized objects (*.dcel)|*.dcel";

            // Show open file dialog box
            Nullable<bool> result = openfile.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = openfile.FileName;

                if (filename != null && File.Exists(filename))
                {
                    SW = File.AppendText(@"viewer.log");
                    string ext = System.IO.Path.GetExtension(filename);

                    if (ext == ".off")
                    {
                        sw.Restart();
                        mesh = DCELTools.LoadFromOFF(filename);
                        sw.Stop();
                        SW.WriteLine("Dcel loading from off: " + sw.Elapsed.TotalMilliseconds);
                        SW.Close();
                        Create3dMesh();
                    }
                    if (ext == ".dcel")
                    {
                        sw.Restart();
                        mesh = DCELTools.DeserializeMesh(filename);
                        sw.Stop();
                        SW.WriteLine("Dcel loading from object: " + sw.Elapsed.TotalMilliseconds);
                        SW.Close();
                        Create3dMesh();
                    }
                    if (wfCheck.IsChecked)
                    {
                        wfCheck.IsChecked = false;
                    }
                    MeshInfo();
                }
                else
                {
                    MessageBox.Show("Errore durante l'apertura del file", "Errore!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();

            savefile.Filter = "Object File Format (*.off)|*.off";
            savefile.RestoreDirectory = true;

            Nullable<bool> result = savefile.ShowDialog();

            if (result == true && mesh != null)
            {
                DCELTools.SaveToOFF(savefile.FileName, mesh);
            }
            else
            {
                MessageBox.Show("Nessun file salvato", "Errore!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void MeshInfo()
        {
            label1.Content = 
                "Vertex count: " + mesh.VertexCount + 
                "\nFace count: " + mesh.FaceCount + 
                "\nHalf-Edge count: " + mesh.HalfEdgeCount;
        }

        private void ClearViewport()
        {
            ModelVisual3D m;

            for (int i = viewport3d.Children.Count - 1; i > 0; i--)
            {
                m = (ModelVisual3D)viewport3d.Children[i];
                viewport3d.Children.Remove(m);
            }
        }

        #region menu items events

        /// <summary>
        /// Show the model's wireframe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWireframeChecked(object sender, RoutedEventArgs e)
        {
            //TO DO
        }

        private void OnWireframeUnchecked(object sender, RoutedEventArgs e)
        {
            //TO DO
        }

        private void OnInfoChecked(object sender, RoutedEventArgs e)
        {
            label1.Visibility = Visibility.Visible;
        }

        private void OnInfoUnchecked(object sender, RoutedEventArgs e)
        {
            label1.Visibility = Visibility.Hidden;
        }

        private void _Commands(object sender, RoutedEventArgs e)
        {
            string message = "Left click to rotate the mesh\nMouse wheel to zoom in and out";
            string caption = "Commands";

            MessageBox.Show(message, caption);
        }

        private void _About(object sender, RoutedEventArgs e)
        {
            string message = "C# DCEL Viewer rev 8.10.10\n\n-Slifer87-";
            string caption = "About DCEL Viewer";

            MessageBox.Show(message, caption);
        }

        #endregion

        #region mouse events

        private void _MouseWheel(object sender, MouseWheelEventArgs e)
        {
            camera.Position = new Point3D(camera.Position.X, camera.Position.Y, camera.Position.Z - e.Delta / 250D);
        }

        private void _MouseMove(object sender, MouseEventArgs e)
        {
            if (mDown)
            {
                Point pos = Mouse.GetPosition(viewport3d);
                Point actualPos = new Point(pos.X - viewport3d.ActualWidth / 2, viewport3d.ActualHeight / 2 - pos.Y);
                double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;

                double mouseAngle = 0;
                if (dx != 0 && dy != 0)
                {
                    mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                    if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                    else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                    else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                }
                else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

                double axisAngle = mouseAngle + Math.PI / 2;

                Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

                double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                Transform3DGroup group = gModel3d.Transform as Transform3DGroup;
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
                group.Children.Add(new RotateTransform3D(r));

                mLastPos = actualPos;
            }
        }

        private void _MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            mDown = true;
            Point pos = Mouse.GetPosition(viewport3d);
            mLastPos = new Point(pos.X - viewport3d.ActualWidth / 2, viewport3d.ActualHeight / 2 - pos.Y);
        }

        private void _MouseUp(object sender, MouseButtonEventArgs e)
        {
            mDown = false;
        }

        private void _MouseLeave(object sender, MouseEventArgs e)
        {
            mDown = false;
        }

        #endregion

    }
}
