using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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

    public class AddPointEventArgs : EventArgs
    {
        public Point point { get; private set; }

        public AddPointEventArgs(Point point_)
        {
            point = point_;
        }
    }

    internal class Curve
    {
        public List<Curve_point> pointArray = new List<Curve_point>();
        public List<Line> lines = new List<Line>(); //??

        public delegate void AddLineHandler(object sender, AddLineEventArgs e);
        public event AddLineHandler OnLineAdded;
        public event AddLineHandler UpdateLinePosition;
        public event AddLineHandler UpdateLines;

        public delegate void AddPointHandler(object sender, AddPointEventArgs e);
        public event AddPointHandler AddNewSegmentPoint;


        public int activeIndex = 0;
        
        public PointCollection CurvePointCollection = new PointCollection();

        private static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200) + 40, 800 - pointPosition.Y * 200);

            return result;
        }
        public List<Curve_point> SortPoints(List<Curve_point> array)
        {
            for (int step = 1; step < array.Count; step++) 
            { 
                Curve_point key = new Curve_point(array[step].ellipse_positionID, array[step].ellipseID);
                int j = step - 1;

                Point swapPoint = new Point(0,0);
                Ellipse swap_ellipseID = null;

                while (j >= 0 && key.ellipse_positionID.X < array[j].ellipse_positionID.X) 
                    {
                        swapPoint = new Point(
                            array[j].ellipse_positionID.X, 
                            array[j].ellipse_positionID.Y);

                        swap_ellipseID = array[j].ellipseID;

                        array[j + 1].ellipse_positionID = swapPoint;
                        array[j + 1].ellipseID = swap_ellipseID;
                        
                        j--;
                        
                    }
                
                swapPoint = new Point(
                            key.ellipse_positionID.X,
                            key.ellipse_positionID.Y);

                swap_ellipseID = key.ellipseID;

                array[j + 1].ellipse_positionID = swapPoint;
                array[j + 1].ellipseID = swap_ellipseID;

            }
            return array;
        }
        public void AddControlPoint(Curve_point curve_Point_ellipse)
        {
            int index = 0;
            foreach (Curve_point curve_Point in pointArray)
            {
                if (curve_Point_ellipse == curve_Point && index > 0)
                {
                    double distance = Math.Abs(curve_Point_ellipse.ellipse_positionID.X - pointArray[index - 1].ellipse_positionID.X);

                    AddPointEventArgs eventArgs = new AddPointEventArgs(new Point((curve_Point.ellipse_positionID.X - distance / 2) - 0.1, curve_Point.ellipse_positionID.Y + distance * 2.75));
                    AddNewSegmentPoint(this, eventArgs);
                    //eventArgs = new AddPointEventArgs(new Point((curve_Point.ellipse_positionID.X - distance / 2) + 1, curve_Point.ellipse_positionID.Y + distance * 2.75));
                    eventArgs = new AddPointEventArgs(curve_Point_ellipse.ellipse_positionID);
                    AddNewSegmentPoint(this, eventArgs);

                    break;
                }
                index++;
            }
        }

        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point curve_Point = new Curve_point(point, ellipse);
            pointArray.Add(curve_Point);

            AddControlPoint(curve_Point);

            AddPointEventArgs eventArgs = new AddPointEventArgs(point);
            AddNewSegmentPoint(this, eventArgs);

            

            //test_myPointCollection.Add(new Point(point.X, point.Y));

            pointArray = SortPoints(pointArray);

            if (pointArray.Count() >= 2)
            {
                Line line = new Line();

                /*Point start = MainWindow.GetCoordToCanvast(
                    pointArray[pointArray.Count() - 2].ellipse_positionID);
                line.X1 = start.X;
                line.Y1 = start.Y;

                Point end = MainWindow.GetCoordToCanvast(
                    pointArray[pointArray.Count() - 1].ellipse_positionID);

                line.X2 = end.X;
                line.Y2 = end.Y;*/

                lines.Add(line);

                /*if (OnLineAdded != null)
                {
                    AddLineEventArgs eventArgs = new AddLineEventArgs(line);
                    OnLineAdded(this, eventArgs);
                    UpdateLines(this, eventArgs);
                }*/
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

                    //test_myPointCollection[index] = e;

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

                        lines[index].X2 = pointArray[index].ellipse_positionID.X;
                        lines[index].Y2 = pointArray[index].ellipse_positionID.Y;
                        eventArgs = new AddLineEventArgs(lines[index]);
                        UpdateLinePosition(this, eventArgs);


                        //pointArray = SortPoints(pointArray);
                        //UpdateLines(null, eventArgs);
                    }
                    break;

                }
                index++;
            } 
        }
    }
}
