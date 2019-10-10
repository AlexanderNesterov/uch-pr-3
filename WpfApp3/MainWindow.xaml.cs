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
        private bool fillSomething = false;
        private bool drawRect = false;
        private bool startDrawRect = false;
        private double previousX = -1;
        private double previousY = -1;
        private double sizeDifference;
        private Rectangle rectangle;
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

            if (drawRect)
            {
                drawRectangle(e);
                return;
            }
        }

        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (drawRect && startDrawRect)
            {
                changeRectangleSize(e);
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
            if (drawRect)
            {
                startDrawRect = false;
            }

            if (isDrawWithPencil)
            {
                startDrawWithPencil = false;
            }
        }

        private void pencilDraw_Click(object sender, RoutedEventArgs e)
        {
            isDrawWithPencil = !isDrawWithPencil;
        }

        private void fill_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            fillSomething = !fillSomething;
        }

        private void rect_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            drawRect = !drawRect;
        }

        private void drawRectangle(MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(canvas);
            rectangle = new Rectangle();
            rectangle.Stroke = Brushes.Black;
            rectangle.StrokeThickness = 4;
            rectangle.Fill = Brushes.White;
            rectangle.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.clickOnRect);

            Canvas.SetLeft(rectangle, startPoint.X);
            Canvas.SetTop(rectangle, startPoint.Y);

            startDrawRect = true;

            canvas.Children.Add(rectangle);
        }

        private void clickOnRect(object sender, MouseButtonEventArgs e)
        {
            if (drawRect)
                return;
            else
            {
                var rect = sender as Rectangle;
                rect.Fill = Brushes.Red;
            }
        }

        private void drawPoint(MouseButtonEventArgs e)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Width = 1;
            ellipse.Height = 1;
            ellipse.Fill = Brushes.Black;
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
    }
}
