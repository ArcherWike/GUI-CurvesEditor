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
    internal class Curve
    {
        public List<Curve_point> pointArray = new List<Curve_point>();

        public Point start_segmentPoint = new Point(0, 0);
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

                    Point punkt = new Point((curve_Point.ellipse_positionID.X - distance / 2), curve_Point.ellipse_positionID.Y + distance * (1/distance)); 

                    CurvePointCollection.Add(GetCoordToCanvast(punkt));
                    CurvePointCollection.Add(GetCoordToCanvast(curve_Point_ellipse.ellipse_positionID));

                    break;
                } 
                index++;
            } 
        }

        public void UpdatePointCollection()
        {
            CurvePointCollection.Clear();
            int index = 0;

            foreach (Curve_point curve_Point in pointArray)
            {
                if (index > 0)
                {
                    double distance = Math.Abs(curve_Point.ellipse_positionID.X - pointArray[index - 1].ellipse_positionID.X);
                    Point control_point = new Point((curve_Point.ellipse_positionID.X - distance / 2), curve_Point.ellipse_positionID.Y + distance * (5 / distance*distance));

                    CurvePointCollection.Add(GetCoordToCanvast(control_point));

                    CurvePointCollection.Add(GetCoordToCanvast(curve_Point.ellipse_positionID));
                    control_point = new Point((curve_Point.ellipse_positionID.X - distance / 2)+1, curve_Point.ellipse_positionID.Y + distance * (5 / distance*distance));

                    CurvePointCollection.Add(GetCoordToCanvast(curve_Point.ellipse_positionID));
                }
                else
                {
                    start_segmentPoint = GetCoordToCanvast(curve_Point.ellipse_positionID);
                }
                index++;
            }

        }

        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point curve_Point = new Curve_point(point, ellipse);
            pointArray.Add(curve_Point);

            if (CurvePointCollection.Count == 0 && start_segmentPoint == new Point(0,0))
            {
                start_segmentPoint = GetCoordToCanvast(point);
            }
            else
            {
                //AddControlPoint(curve_Point);
                //CurvePointCollection.Add(GetCoordToCanvast(point));

                pointArray = SortPoints(pointArray);
                UpdatePointCollection();
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

                    if (CurvePointCollection.Count() - 1 == index) //last
                    {
                        pointArray = SortPoints(pointArray);
                        UpdatePointCollection();
                    }
                    else if (index == 0)
                    {
                        pointArray = SortPoints(pointArray);
                        UpdatePointCollection();
                        /*start_segmentPoint = GetCoordToCanvast(e);

                        double distance = Math.Abs(pointArray[1].ellipse_positionID.X - start_segmentPoint.X);

                        Point control_point = new Point((pointArray[1].ellipse_positionID.X - distance / 2), pointArray[1].ellipse_positionID.X + distance * (1 / distance));

                        CurvePointCollection[0] = (GetCoordToCanvast(control_point));
                        CurvePointCollection[1] = (GetCoordToCanvast(pointArray[1].ellipse_positionID));*/

                    }
                    else
                    {
                        pointArray = SortPoints(pointArray);
                        UpdatePointCollection();
                        //CurvePointCollection[index] = GetCoordToCanvast(e);
                        //CurvePointCollection[index - 1] = GetCoordToCanvast(e);
                    }
                    break;
                }
                index += 3;
            } 
        }
    }
}
