using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Markup;
using System.Diagnostics;
using Microsoft.Win32;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool draw = false;
        private bool fillSomething = false;
        private bool drawRect = false;
        private bool startDrawRect = false;
        private double previousX = -1;
        private double previousY = -1;
        private double sizeDifference;
        private Rectangle rectangle;

        public MainWindow()
        {
            InitializeComponent();
            sizeDifference = canvas.Margin.Top - 8;
        }

        private void save_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("save button");

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "dat files (*.dat)|*.dat";
            dialog.ShowDialog();

            string path = dialog.FileName;

            if (path.Equals(""))
            {
                return;
            }

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                string mystrXAML = XamlWriter.Save(canvas);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(mystrXAML);
                sw.Close();
                fs.Close();
            }
        }

        private void open_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("open button");

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "dat files (*.dat)|*.dat";
            dialog.ShowDialog();

            string path = dialog.FileName;

            if (path.Equals(""))
            {
                return;
            }

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                StreamReader sr = new StreamReader(fs);
                Canvas openedCanvas = (Canvas)XamlReader.Parse(sr.ReadToEnd());
                canvas.Children.Clear();

                for (int i = 0; i < openedCanvas.Children.Count; i++)
                {
                    UIElement el = openedCanvas.Children[i];
                    openedCanvas.Children.Remove(el);
                    i--;

                    canvas.Children.Add(el);
                }
            }
        }

        private void canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine(canvas.Height + " " + window.Height + " " + sizeDifference);

            if (drawRect)
            {
                rectangle = new Rectangle();
                rectangle.Margin = new Thickness(e.GetPosition(null).X, e.GetPosition(null).Y - sizeDifference, 0, 0);
                //rect.Height = 30;
                //rect.Width = 50;
                rectangle.Stroke = Brushes.Black;
                rectangle.StrokeThickness = 4;

                startDrawRect = true;

                return;
                //canvas.Children.Add(rect);
            }

            if (fillSomething)
            {
                Polygon polygon = new Polygon();
                PointCollection collection = new PointCollection();
                
                foreach (UIElement el in canvas.Children)
                {
                    if (el.GetType() != typeof(Line))
                    {
                        continue;
                    }

                    Point point = new Point(((Line)el).X1, ((Line)el).Y1);
                    collection.Add(point);
                }

                polygon.Points = collection;
                polygon.Fill = Brushes.Red;

                canvas.Children.Add(polygon);
                return;
            }
            //Console.WriteLine("canvas mouse down");
            previousX = e.GetPosition(null).X;
            previousY = e.GetPosition(null).Y - sizeDifference;

            Ellipse ellipse = new Ellipse();
            ellipse.Width = 1;
            ellipse.Height = 1;
            ellipse.Fill = Brushes.Black;
            //ellipse.StrokeThickness = 2;
            //ellipse.Stroke = Brushes.Black;
            ellipse.Margin = new Thickness(e.GetPosition(null).X, e.GetPosition(null).Y - sizeDifference, 0, 0);

            canvas.Children.Add(ellipse);

            draw = true;
        }

        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (drawRect && startDrawRect)
            {
                //Console.WriteLine(rectangle.Margin.Top + " " + rectangle.Margin.Left);
                rectangle.Height = Math.Abs(rectangle.Margin.Top - e.GetPosition(null).Y + sizeDifference);
                rectangle.Width = Math.Abs(rectangle.Margin.Left - e.GetPosition(null).X);

                return;
            }

            //Console.WriteLine("canvas mouse move");
            if (draw)
            {
                //Ellipse ellipse = new Ellipse();
                //ellipse.Width = 4;
                //ellipse.Height = 4;
                //ellipse.Fill = Brushes.Black;
                //ellipse.StrokeThickness = 2;
                //ellipse.Stroke = Brushes.Black;
                //ellipse.Margin = new Thickness(e.GetPosition(null).X - 1, e.GetPosition(null).Y - 10, 0, 0);

                //canvas.Children.Add(ellipse);

                //Console.WriteLine("canvas mouse move");
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.X1 = previousX;
                line.Y1 = previousY;
                line.X2 = previousX = e.GetPosition(null).X;
                line.Y2 = previousY = e.GetPosition(null).Y - sizeDifference;

                //line.X2 = 100;
                //line.Y2 = 100;

                //line.Stretch = Stretch.Fill;
                //line.StrokeThickness = 4;

                canvas.Children.Add(line);
            }
        }

        private void canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Console.WriteLine("canvas mouse up");
            if (drawRect)
            {
                canvas.Children.Add(rectangle);
                startDrawRect = false;
            }


            draw = false;
        }

        private void fill_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            fillSomething = !fillSomething;
        }

        private void rect_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            drawRect = !drawRect;
        }
    }
}
