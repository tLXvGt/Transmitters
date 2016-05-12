using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Transmitters;

namespace TransmittersProblem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Node> listOfNodes = new List<Node>();
        public List<Edge> listOfEdges = new List<Edge>();

        public MainWindow()
        {
            InitializeComponent();
            PrepareCanvas();
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            PrepareCanvas();

            var graph = GenerateTransmitters(
                int.Parse(cityRadiusTextBox.Text),
                int.Parse(noOfTransmittersTextBox.Text),
                int.Parse(transmitterRadiusTextBox.Text));
            listOfNodes = graph.nodes;
            listOfEdges = graph.edges;

            foreach (var node in graph.nodes)
            {
                DrawColoredCircle(
                    node.radius * Math.Cos(node.angle * Math.PI / 180),
                    node.radius * Math.Sin(node.angle * Math.PI / 180),
                    int.Parse(transmitterRadiusTextBox.Text
                ));
            }

            foreach (var edge in graph.edges)
            {
                DrawLine(
                    edge.start.radius * Math.Cos(edge.start.angle * Math.PI / 180),
                    edge.start.radius * Math.Sin(edge.start.angle * Math.PI / 180),
                    edge.end.radius * Math.Cos(edge.end.angle * Math.PI / 180),
                    edge.end.radius * Math.Sin(edge.end.angle * Math.PI / 180)
                );
            }

        }

        public void PrepareCanvas()
        {
            DrawLine(0, canvas.Height / 2, 0, -canvas.Height / 2, thickness: 0.5, transparency: 40);
            DrawLine(-canvas.Width / 2, 0, canvas.Width / 2, 0, thickness: 0.5, transparency: 40);
            DrawColoredCircle(0, 0, int.Parse(cityRadiusTextBox.Text), thickness: 0.3);
        }

        public void DrawColoredCircle(double X, double Y, double radius,
            int color = -1, double thickness = 1, bool hasCenter = false)
        {
            Ellipse circle = new Ellipse();
            circle.Width = 2 * radius;
            circle.Height = 2 * radius;
            circle.Stroke = Brushes.Black;
            circle.StrokeThickness = thickness;
            Canvas.SetLeft(circle, canvas.Width / 2 + X - radius);
            Canvas.SetTop(circle, canvas.Height / 2 - Y - radius);

            byte transparency = 80;
            switch (color)
            {
                case 1:
                    circle.Fill = new SolidColorBrush(Color.FromArgb(transparency, 255, 0, 0));
                    break;

                case 2:
                    circle.Fill = new SolidColorBrush(Color.FromArgb(transparency, 0, 255, 0));
                    break;

                case 3:
                    circle.Fill = new SolidColorBrush(Color.FromArgb(transparency, 0, 0, 255));
                    break;

                case 4:
                    circle.Fill = new SolidColorBrush(Color.FromArgb(transparency, 255, 255, 0));
                    break;

                case 5:
                    circle.Fill = new SolidColorBrush(Color.FromArgb(transparency, 255, 0, 255));
                    break;

                case 6:
                    circle.Fill = new SolidColorBrush(Color.FromArgb(transparency, 0, 255, 255));
                    break;

                default:
                    break;
            }

            if (hasCenter)
            {
                Ellipse center = new Ellipse();

                center.Width = 2;
                center.Height = 2;
                center.Stroke = Brushes.Black;
                center.StrokeThickness = 2;

                Canvas.SetLeft(center, canvas.Width / 2 + X - 1);
                Canvas.SetTop(center, canvas.Height / 2 - Y - 1);
                canvas.Children.Add(center);
            }

            canvas.Children.Add(circle);
        }

        public void DrawLine(double X1, double Y1, double X2, double Y2, double thickness = 3, byte transparency = 40)
        {
            Line line = new Line();
            line.X1 = X1;
            line.Y1 = -Y1;
            line.X2 = X2;
            line.Y2 = -Y2;
            line.StrokeThickness = thickness;
            line.Stroke = new SolidColorBrush(Color.FromArgb(transparency, 0, 0, 0));

            Canvas.SetLeft(line, canvas.Width / 2);
            Canvas.SetTop(line, canvas.Height / 2);

            canvas.Children.Add(line);
        }

        public static Graph GenerateTransmitters(int _cityRadius, int _transmittersCount, int _transmitterRadius)
        {
            Random random = new Random();
            var nodes = new List<Node>();
            var edges = new List<Edge>();
            for (int i = 0; i < _transmittersCount; i++)
            {
                var node = new Node("v" + (i + 1).ToString(),
                                      random.NextDouble() * _cityRadius,
                                      random.NextDouble() * 360);

                foreach (var item in nodes)
                {
                    if (node.CalculateDistance(item) < 2 * _transmitterRadius)
                    {
                        edges.Add(new Edge(node, item));
                        node.neighbourCount++;
                        item.neighbourCount++;
                    }
                }

                nodes.Add(node);
            }

            return new Graph(nodes, edges);
        }

        private void solveButton_Click(object sender, RoutedEventArgs e)
        {
            var sortedListOfNodes = listOfNodes.OrderByDescending(n => n.neighbourCount);
            var currentColor = 1;

            foreach (var node in sortedListOfNodes)
            {
                if (!node.HasColor())
                {
                    while (!node.CanColor(currentColor, listOfEdges))
                    {
                        currentColor++;
                    }
                    node.color = currentColor;

                    DrawColoredCircle(
                        node.radius * Math.Cos(node.angle * Math.PI / 180),
                        node.radius * Math.Sin(node.angle * Math.PI / 180),
                        double.Parse(transmitterRadiusTextBox.Text),
                        currentColor
                    );

                    currentColor = 1;
                }

                var currentNeighbours = node.GetNeighbours(listOfEdges);
                foreach (var neighbour in currentNeighbours)
                {
                    if (!neighbour.HasColor())
                    {
                        while (!neighbour.CanColor(currentColor, listOfEdges))
                        {
                            currentColor++;
                        }
                        neighbour.color = currentColor;

                        DrawColoredCircle(
                            neighbour.radius * Math.Cos(neighbour.angle * Math.PI / 180),
                            neighbour.radius * Math.Sin(neighbour.angle * Math.PI / 180),
                            double.Parse(transmitterRadiusTextBox.Text),
                            currentColor
                        );
                        currentColor = 1;
                    }


                }

                currentNeighbours.Clear();
            }

            /*
            var best = listOfNodes.OrderByDescending(n => n.neighbourCount).First();
            resultDescriptionLabel.Content = best.name + " " + best.neighbourCount;

            var result = best.GetNeighbours(listOfEdges);
            string text = "";
            int i = 1;
            foreach (var item in result)
            {
                DrawColoredCircle(
                    item.radius * Math.Cos(item.angle * Math.PI / 180),
                    item.radius * Math.Sin(item.angle * Math.PI / 180),
                    double.Parse(transmitterRadiusTextBox.Text),
                    i
                    );
                i++;
                if (i>3)
                {
                    i = 1;
                }
                text += item.name + ", ";
            }

            MessageBox.Show(text);
            */
        }
    }
}
