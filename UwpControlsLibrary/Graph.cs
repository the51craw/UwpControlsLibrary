using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace UwpControlsLibrary
{
    /// <summary>
    /// A Graph a background image and a canvas to draw graphs on.
    /// A border size limits the size of the graph by limiting when drawing.
    /// The GraphDraw accepts a list of points to draw lines between.
    /// Position denotes the upper left corner for  poitioning the graph
    /// in respect to its parent, gridControls or a CompoundControl.
    /// </summary>

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Graph class
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class Graph : ControlBase
    {
        public Canvas Canvas;
        public int LineWidth;
        public List<Point> Points;
        public double CurrentAppWidth;
        public double OriginalAppWidth;
        public double CurrentAppHeight;
        public double OriginalAppHeight;

        public Grid gridCanvas;
        public Canvas canvas;
        private Brush color;

        public Graph(Controls controls, int Id, Grid gridControls, Image[] imageList, Point Position, Brush Color, int LineWidth = 2)
        {
            this.Id = Id;
            canvas = new Canvas();
            Points = new List<Point>();
            gridCanvas = new Grid();
            gridControls.Children.Add(gridCanvas);
            canvas = new Canvas();
            gridCanvas.Children.Add(canvas);
            GridControls = gridControls;
            this.LineWidth = LineWidth;
            Double width;
            Double height;
            //this.HitTarget = HitTarget;
            color = Color;

            if (imageList != null && imageList.Length > 0)
            {
                width = imageList[0].ActualWidth;
                height = imageList[0].ActualHeight;
            }
            else
            {
                throw new Exception("A Graph must have a background image.");
            }

            HitArea = new Rect(Position.X, Position.Y, width, height);
            CopyImages(imageList);
            ControlSizing = new ControlSizing(controls, this);

            Canvas = new Canvas();
            Canvas.Margin = new Thickness(Position.X, Position.Y,
                gridControls.ActualWidth - Position.X - imageList[0].ActualWidth,
                gridControls.ActualHeight - Position.Y - imageList[0].ActualHeight);
            gridControls.Children.Add(Canvas);
        }

        public void SetValue(Point position)
        {
        }

        public void AddPoint(Point point)
        {
            CurrentAppWidth = ((ControlBase)this).ControlSizing.Controls.imgClickArea.ActualWidth;
            OriginalAppWidth = ((ControlBase)this).ControlSizing.Controls.OriginalWidth;
            CurrentAppHeight = ((ControlBase)this).ControlSizing.Controls.imgClickArea.ActualHeight;
            OriginalAppHeight = ((ControlBase)this).ControlSizing.Controls.OriginalHeight;
            Points.Add(point);
        }

        public void RemovePoint(Point point)
        {
            foreach (Point p in Points)
            {
                if (Math.Abs(p.X - point.X) < 20 && Math.Abs(p.Y - point.Y) < 20)
                {
                    Points.Remove(p);
                    return;
                }
            }
        }

        public void CopyPoints(List<Point> PointsToCopy)
        {
            Points.Clear();
            foreach (Point point in PointsToCopy)
            {
                Points.Add(new Point(point.X, point.Y));
            }
        }

        public void CopyPoints(Point[] PointsToCopy)
        {
            Points.Clear();
            foreach (Point point in PointsToCopy)
            {
                Points.Add(new Point(point.X, point.Y));
            }
        }

        public void SortByX()
        {
            Points.Sort((a, b) => a.X.CompareTo(b.X));
        }

        public void SortByY()
        {
            Points.Sort((a, b) => a.Y.CompareTo(b.Y));
        }

        public void SortByXReversed()
        {
            Points.Sort((b, a) => a.X.CompareTo(b.X));
        }

        public void SortByYReversed()
        {
            Points.Sort((b, a) => a.Y.CompareTo(b.Y));
        }

        public void Erase()
        {
            Canvas.Children.Clear();
        }

        public void Draw()
        {
            Draw(Points.ToArray());
        }

        public void Draw(List<Point> points)
        {
            Draw(points.ToArray());
        }

        public void Draw(Point[] points)
        {
            CurrentAppWidth = ((ControlBase)this).ControlSizing.Controls.imgClickArea.ActualWidth;
            OriginalAppWidth = ((ControlBase)this).ControlSizing.Controls.OriginalWidth;
            CurrentAppHeight = ((ControlBase)this).ControlSizing.Controls.imgClickArea.ActualHeight;
            OriginalAppHeight = ((ControlBase)this).ControlSizing.Controls.OriginalHeight;
            Canvas.Children.Clear();

            try
            {
                for (int i = 1; i < points.Length; i++)
                {
                    Line line = new Line();
                    line.Stroke = color;
                    line.StrokeThickness = LineWidth / OriginalAppWidth * CurrentAppWidth;
                    line.X1 = points[i - 1].X / OriginalAppWidth * CurrentAppWidth;
                    line.Y1 = points[i - 1].Y / OriginalAppHeight * CurrentAppHeight;
                    line.X2 = points[i].X / OriginalAppWidth * CurrentAppWidth;
                    line.Y2 = points[i].Y / OriginalAppHeight * CurrentAppHeight;
                    Canvas.Children.Add(line);
                }
            }
            catch (Exception e)
            {
                string msg = "Error in Graph.Draw! \n Points.Count = " + points.Length.ToString();
                msg += "\nOriginalAppWidt = " + OriginalAppWidth.ToString();
                msg += "\nCurrentAppWidt = " + CurrentAppWidth.ToString();
                msg += "\nCanvas = " + Canvas.ToString();
                throw new Exception(msg);
            }
        }
    }
}
