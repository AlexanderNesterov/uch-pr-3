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
        private bool isDrawFigure = false;
        private bool startDrawFigure = false;
        private bool isFill = false;
        private double previousX = -1;
        private double previousY = -1;
        private double sizeDifference;
        private double thickness = 1.0;
        private SolidColorBrush fillColor = Brushes.White;
        private Point startPoint;
        private Shape figure;

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

            if (isDrawFigure)
            {
                drawFigure(e);
                return;
            }
        }

        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawFigure && startDrawFigure)
            {
                changeFigureSize(e);
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
            if (isDrawFigure)
            {
                startDrawFigure = false;
            }

            if (isDrawWithPencil)
            {
                startDrawWithPencil = false;
            }
        }

        private void pencilDraw_Click(object sender, RoutedEventArgs e)
        {
            isDrawWithPencil = !isDrawWithPencil;
            isDrawFigure = false;
            isFill = false;
        }

        private void drawRectangle_Click(object sender, RoutedEventArgs e)
        {
            if (!isDrawFigure)
            {
                isDrawFigure = true;
            }
            figure = new Rectangle();
            isDrawWithPencil = false;
            isFill = false;
        }

        private void drawEllipse_Click(object sender, RoutedEventArgs e)
        {
            if (!isDrawFigure)
            {
                isDrawFigure = true;
            }
            figure = new Ellipse();
            isDrawWithPencil = false;
            isFill = false;
        }

        private void drawTriangle_Click(object sender, RoutedEventArgs e)
        {
        }

        private void redFill_Click(object sender, RoutedEventArgs e)
        {
            fillColor = Brushes.Red;
            isFill = true;
            isDrawWithPencil = false;
            isDrawFigure = false;
        }

        private void blueFill_Click(object sender, RoutedEventArgs e)
        {
            fillColor = Brushes.Blue;
            isFill = true;
            isDrawWithPencil = false;
            isDrawFigure = false;
        }

        private void oneThickness_Click(object sender, RoutedEventArgs e)
        {
            thickness = 1.0;
        }

        private void twoThickness_Click(object sender, RoutedEventArgs e)
        {
            thickness = 2.0;
        }

        private void drawFigure(MouseButtonEventArgs e)
        {
            figure = createFigure();
            startPoint = e.GetPosition(canvas);
            figure.Stroke = Brushes.Black;
            figure.Fill = Brushes.White;
            figure.StrokeThickness = thickness;
            figure.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.clickOnFigure);

            Canvas.SetLeft(figure, startPoint.X);
            Canvas.SetTop(figure, startPoint.Y);

            startDrawFigure = true;

            canvas.Children.Add(figure);
        }

        private void clickOnFigure(object sender, MouseButtonEventArgs e)
        {
            if (isFill)
            {
                var fig = sender as Shape;
                fig.Fill = fillColor;
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

        private void changeFigureSize(MouseEventArgs e)
        {
            var pos = e.GetPosition(canvas);

            var x = Math.Min(pos.X, startPoint.X);
            var y = Math.Min(pos.Y, startPoint.Y);

            var w = Math.Max(pos.X, startPoint.X) - x;
            var h = Math.Max(pos.Y, startPoint.Y) - y;

            figure.Width = w;
            figure.Height = h;

            Canvas.SetLeft(figure, x);
            Canvas.SetTop(figure, y);
        }

        private Shape createFigure()
        {
            if (figure.GetType() == typeof(Rectangle))
            {
                return new Rectangle();
            }

            if (figure.GetType() == typeof(Ellipse))
            {
                return new Ellipse();
            }

            return null;
        }
    }
}
