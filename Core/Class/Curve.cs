using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using UiDesign;

namespace Curves_editor.Core.Class
{
    public class Curve_point
    {
        public Point ellipse_positionID { get; set; }
        public Ellipse ellipseID { get; set; }
        public Curve_point(Point position, Ellipse ellipse)
        {
            ellipse_positionID = position;
            ellipseID = ellipse;
        }
    }
    public class AddLineEventArgs : EventArgs
    {
        public Line line { get; private set; }
        public AddLineEventArgs(Line line_)
        {
            line = line_;
        }
    }
    internal class Curve
    {
        public List<Curve_point> pointArray = new List<Curve_point>();
        public List<Line> lines = new List<Line>();

        public delegate void AddLineHandler(object sender, AddLineEventArgs e);
        public event AddLineHandler OnLineAdded;
        public event AddLineHandler UpdateLinePosition;

        public int activeIndex = 0;

        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point curve_Point = new Curve_point(point, ellipse);
            pointArray.Add(curve_Point);

            if (pointArray.Count() >= 2)
            {
                Line line = new Line();

                Point start = MainWindow.GetCoordToCanvast(
                    pointArray[pointArray.Count() - 2].ellipse_positionID);
                line.X1 = start.X;
                line.Y1 = start.Y;

                Point end = MainWindow.GetCoordToCanvast(
                    pointArray[pointArray.Count() - 1].ellipse_positionID);

                line.X2 = end.X;
                line.Y2 = end.Y;

                lines.Add(line);

                if (OnLineAdded != null)
                {
                    AddLineEventArgs eventArgs = new AddLineEventArgs(line);
                    OnLineAdded(this, eventArgs);
                }
            }
        }

        public void UpdatePointPosition(Ellipse sender, Point e)
        {
            int index = 0;
            foreach (Curve_point curve_Point in pointArray) 
            {
                if (sender == curve_Point.ellipseID)
                {
                    curve_Point.ellipse_positionID = e;

                    activeIndex = index;

                    if (pointArray.Count() - 1 == index) //last
                    {
                        activeIndex = index - 1;

                        lines[index - 1].X2 = pointArray[index].ellipse_positionID.X;
                        lines[index - 1].Y2 = pointArray[index].ellipse_positionID.Y;
                        AddLineEventArgs eventArgs = new AddLineEventArgs(lines[index - 1]);
                        UpdateLinePosition(this, eventArgs);
                    }
                    else if (index == 0)
                    {
                        lines[index].X1 = pointArray[index].ellipse_positionID.X;
                        lines[index].Y1 = pointArray[index].ellipse_positionID.Y;
                        AddLineEventArgs eventArgs = new AddLineEventArgs(lines[index]);
                        UpdateLinePosition(null, eventArgs);
                    }
                    else
                    {
                        lines[index].X1 = pointArray[index].ellipse_positionID.X;
                        lines[index].Y1 = pointArray[index].ellipse_positionID.Y;
                        AddLineEventArgs eventArgs = new AddLineEventArgs(lines[index]);
                        UpdateLinePosition(null, eventArgs); //(this,)

                        activeIndex = index - 1;

                        lines[index - 1].X2 = pointArray[index].ellipse_positionID.X;
                        lines[index - 1].Y2 = pointArray[index].ellipse_positionID.Y;
                        eventArgs = new AddLineEventArgs(lines[index - 1]);
                        UpdateLinePosition(this, eventArgs);
                    }
                    break;
                }
                index++;
            } 
        }
    }
}
