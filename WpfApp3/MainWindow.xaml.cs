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
        private bool isDrawWithPencil = false;
        private bool startDrawWithPencil = false;
        private bool isFill = false;
        private bool isDrawRect = false;
        private bool startDrawRect = false;
        private bool isDrawEllipse = false;
        private bool startDrawEllipse = false;
        private double previousX = -1;
        private double previousY = -1;
        private double sizeDifference;
        private double thickness = 1.0;
        private SolidColorBrush fillColor = Brushes.White;
        private Rectangle rectangle;
        private Ellipse ellipse;
        private Point startPoint;

        public MainWindow()
        {
            InitializeComponent();
            sizeDifference = canvas.Margin.Top - 8;
        }

        private void save_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
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

        private void open_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
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
            if (isDrawWithPencil)
            {
                startDrawWithPencil = true;

                previousX = e.GetPosition(null).X;
                previousY = e.GetPosition(null).Y - sizeDifference;

                drawPoint(e);
                return;
            }

            if (isDrawRect)
            {
                drawRectangle(e);
                return;
            }

            if (isDrawEllipse)
            {
                drawEllipse(e);
                return;
            }
        }

        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawRect && startDrawRect)
            {
                changeRectangleSize(e);
                return;
            }

            if (isDrawEllipse && startDrawEllipse)
            {
                changeEllipseSize(e);
                return;
            }

            if (isDrawWithPencil && startDrawWithPencil)
            {
                drawLine(e);
                return;
            }
        }

        private void canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawRect)
            {
                startDrawRect = false;
            }

            if (isDrawEllipse)
            {
                startDrawEllipse = false;
            }

            if (isDrawWithPencil)
            {
                startDrawWithPencil = false;
            }
        }

        private void pencilDraw_Click(object sender, RoutedEventArgs e)
        {
            isDrawWithPencil = !isDrawWithPencil;
            //isFill = false;
            isDrawRect = false;

        }

        private void drawRectangle_Click(object sender, RoutedEventArgs e)
        {
            isDrawRect = !isDrawRect;
            isDrawWithPencil = false;
            isDrawEllipse = false;
            //isFill = false;
        }

        private void drawEllipse_Click(object sender, RoutedEventArgs e)
        {
            isDrawEllipse = !isDrawEllipse;
            isDrawWithPencil = false;
            //isFill = false;
            isDrawRect = false;

        }

        private void redFill_Click(object sender, RoutedEventArgs e)
        {
            fillColor = Brushes.Red;
            isDrawWithPencil = false;
            isDrawEllipse = false;
            isDrawRect = false;
        }

        private void blueFill_Click(object sender, RoutedEventArgs e)
        {
            fillColor = Brushes.Blue;
            isDrawWithPencil = false;
            isDrawEllipse = false;
            isDrawRect = false;
        }

        private void oneThickness_Click(object sender, RoutedEventArgs e)
        {
            thickness = 1.0;
        }

        private void twoThickness_Click(object sender, RoutedEventArgs e)
        {
            thickness = 2.0;
        }

        private void drawRectangle(MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            rectangle = new Rectangle();
            rectangle.Stroke = Brushes.Black;
            rectangle.Fill = Brushes.White;
            rectangle.StrokeThickness = thickness;
            rectangle.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.clickOnRect);

            Canvas.SetLeft(rectangle, startPoint.X);
            Canvas.SetTop(rectangle, startPoint.Y);

            startDrawRect = true;

            canvas.Children.Add(rectangle);
        }

        private void drawEllipse(MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            ellipse = new Ellipse();
            ellipse.Stroke = Brushes.Black;
            ellipse.Fill = Brushes.White;
            ellipse.StrokeThickness = thickness;
            ellipse.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.clickOnEllipse);

            Canvas.SetLeft(ellipse, startPoint.X);
            Canvas.SetTop(ellipse, startPoint.Y);

            startDrawEllipse = true;

            canvas.Children.Add(ellipse);
        }

        private void clickOnRect(object sender, MouseButtonEventArgs e)
        {
            if (isDrawRect)
                return;
            else
            {
                var rect = sender as Rectangle;
                rect.Fill = fillColor;
            }
        }

        private void clickOnEllipse(object sender, MouseButtonEventArgs e)
        {
            if (isDrawEllipse)
                return;
            else
            {
                var rect = sender as Ellipse;
                ellipse.Fill = fillColor;
            }
        }

        private void drawPoint(MouseButtonEventArgs e)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 1;
            ellipse.Height = 1;
            ellipse.Fill = Brushes.Black;
            ellipse.StrokeThickness = thickness;
            ellipse.Margin = new Thickness(e.GetPosition(null).X, e.GetPosition(null).Y - sizeDifference, 0, 0);

            canvas.Children.Add(ellipse);
        }

        private void drawLine(MouseEventArgs e)
        {
            Line line = new Line();
            line.Stroke = Brushes.Black;
            line.X1 = previousX;
            line.Y1 = previousY;
            line.X2 = previousX = e.GetPosition(null).X;
            line.Y2 = previousY = e.GetPosition(null).Y - sizeDifference;
            line.StrokeThickness = thickness;

            canvas.Children.Add(line);
        }

        private void changeRectangleSize(MouseEventArgs e)
        {
            var pos = e.GetPosition(canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            rectangle.Width = w;
            rectangle.Height = h;

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
        }

        private void changeEllipseSize(MouseEventArgs e)
        {
            var pos = e.GetPosition(canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            ellipse.Width = w;
            ellipse.Height = h;

            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);
        }
    }
}
