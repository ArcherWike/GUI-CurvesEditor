using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using UiDesign;

namespace Curves_editor.Core.Class
{
    enum pointType
    {
        Normal,
        Control
    }
    public class Curve_point
    {
        public Point ellipse_positionID { get; set; }
        public Ellipse ellipseID { get; set; }

        //pointType ellipse_type;

        public Curve_point(Point position, Ellipse ellipse)
        {
            ellipse_positionID = position;
            ellipseID = ellipse; 
        }
    }

    
    public class Segment_curve
    {
        public Path SegmentGet_pathGeometry()
        {
            segmentPath.Stroke = Brushes.Black;
            segmentPath.StrokeThickness = 5;

            if (points.Count() < 4)
            {
                BezierSegment new_segment;
                PathFigure pathFigure = new PathFigure();

                myPathGeometry.Clear();
                pathFigure.StartPoint = points[0].ellipse_positionID;
                if (points.Count() == 2)
                {
                    Curve_point control_point = new Curve_point(new Point(
                      Math.Abs(points[1].ellipse_positionID.X - points[0].ellipse_positionID.X),
                      points[0].ellipse_positionID.Y + Math.Abs(points[1].ellipse_positionID.Y - points[0].ellipse_positionID.Y)),
                      new Ellipse());

                    new_segment = new BezierSegment(
                        points[0].ellipse_positionID,
                        control_point.ellipse_positionID,
                        points[1].ellipse_positionID,
                        true);
                }
                else
                {
                    new_segment = new BezierSegment(
                        points[0].ellipse_positionID,
                        points[1].ellipse_positionID,
                        points[2].ellipse_positionID,
                        true);
                }
                pathFigure.Segments.Add(new_segment);
                myPathGeometry.Figures.Add(pathFigure);

                segmentPath.Data = myPathGeometry;
            }
            else
            {
                myPathFigure.StartPoint = points[0].ellipse_positionID;
                polyBezierSegment.Points = pointCollection;

                myPathSegmentCollection.Clear();
                myPathSegmentCollection.Add(polyBezierSegment);

                myPathFigure.Segments = myPathSegmentCollection;

                PathFigureCollection myPathFigureCollection = new PathFigureCollection();
                myPathFigureCollection.Add(myPathFigure);


                myPathGeometry.Figures = myPathFigureCollection;




                segmentPath.Data = myPathGeometry;
            }
            return segmentPath;
        }

        public void Add_controlPoint(Curve_point new_control_point)
        {
            if (points.Count < 4)
            {
                Curve_point end_point = points[points.Count - 1];

                points[points.Count() - 1] = new_control_point;
                points.Add(end_point);

                pointCollection[pointCollection.Count() - 1] = new_control_point.ellipse_positionID;
                //pointCollection.Remove(end_point.ellipse_positionID);
                //pointCollection[points.Count() - 1] = new_control_point.ellipse_positionID;
                pointCollection.Add(end_point.ellipse_positionID);
            }       
        }
        PathGeometry myPathGeometry = new PathGeometry();
        Path segmentPath = new Path();
        PathFigure myPathFigure = new PathFigure();
        LineGeometry newGeometry;

        PolyBezierSegment polyBezierSegment = new PolyBezierSegment();
        PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();

        PointCollection pointCollection = new PointCollection();
        public List<Curve_point> points = new List<Curve_point>();
        public Segment_curve(Curve_point start_point, Curve_point end_point)
        {
            points.Add(start_point);
            //pointCollection.Add(start_point.ellipse_positionID);
            /*Curve_point control_point = new Curve_point(new Point(
                Math.Abs(end_point.ellipse_positionID.X - start_point.ellipse_positionID.X),
                start_point.ellipse_positionID.Y + Math.Abs(end_point.ellipse_positionID.Y - start_point.ellipse_positionID.Y)), 
                new Ellipse());
            points.Add(control_point);*/
            points.Add(end_point);
            pointCollection.Add(end_point.ellipse_positionID);

        }
    }

    public class PathEventArgs : EventArgs
    {
        public Path pathGeometry { get; private set; }
        public PathEventArgs(Path path_)
        {
            pathGeometry = path_;
        }
    }




    internal class Curve
    {
        public List<Curve_point> base_pointArray = new List<Curve_point>();
        public List<Curve_point> pointArray = new List<Curve_point>();

        public List<Segment_curve> segments = new List<Segment_curve>();

        public delegate void PathHandler(object sender, PathEventArgs e);
        public event PathHandler PathGeomertyAddToViewport;


        public List<Curve_point> SortPoints(List<Curve_point> array)
        {
            for (int step = 1; step < array.Count; step++)
            {
                Curve_point key = new Curve_point(array[step].ellipse_positionID, array[step].ellipseID);
                int j = step - 1;

                Point swapPoint = new Point(0, 0);

                while (j >= 0 && key.ellipse_positionID.X < array[j].ellipse_positionID.X)
                {
                    swapPoint = new Point(
                        array[j].ellipse_positionID.X,
                        array[j].ellipse_positionID.Y);

                    array[j + 1].ellipse_positionID = swapPoint;

                    j--;

                }

                swapPoint = new Point(
                            key.ellipse_positionID.X,
                            key.ellipse_positionID.Y);

                array[j + 1].ellipse_positionID = swapPoint;


            }
            return array;
        }
        
        public bool CheckNextPoint_isNormal(Point point)
        {
            if (base_pointArray.Count() == 0 || base_pointArray[base_pointArray.Count() - 1].ellipse_positionID.X < point.X)
            {
                return true;
            }
            return false;
        }

        public void DeleteCurves()
        {

            base_pointArray = new List<Curve_point>();
            segments = new List<Segment_curve>();
        }

        private static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200) + 40, 800 - pointPosition.Y * 200);

            return result;
        }


        void AddSegment_to_viewport(Segment_curve segment)
        {
            PathEventArgs eventArgs = new PathEventArgs(segment.SegmentGet_pathGeometry());
            PathGeomertyAddToViewport(this, eventArgs);
        }


        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point curve_Point = new Curve_point(point, ellipse);
            pointArray.Add(curve_Point);

            if (CheckNextPoint_isNormal(point))
            {
                if (base_pointArray.Count() > 0)
                {
                    Segment_curve segment_Curve = new Segment_curve(base_pointArray[base_pointArray.Count() - 1], curve_Point);
                    segments.Add(segment_Curve);
                    AddSegment_to_viewport(segment_Curve);
                }
                else
                {
                    /*pathFigure.StartPoint = (curve_Point.ellipse_positionID);*/
                }

                base_pointArray.Add(curve_Point);
            }
            else
            {
                foreach (Segment_curve segment in segments)
                {
                    if (segment.points[0].ellipse_positionID.X <= point.X && segment.points[segment.points.Count() - 1].ellipse_positionID.X >= point.X)
                    {
                        segment.Add_controlPoint(curve_Point);
                        /*segment.points[1] = pointArray[pointArray.Count() - 1];*/
                        AddSegment_to_viewport(segment);

                        break;
                    }
                }
            }
            
        }

        public void UpdatePointPosition(Ellipse sender, Point e)
        {
            foreach (Curve_point curve_Point in pointArray)
            {
                if (sender == curve_Point.ellipseID)
                {
                    curve_Point.ellipse_positionID = GetCoordToCanvast(e);

                    break;
                }
            }
            foreach (Segment_curve segment in segments)
            {
                for (int i = 0; i < segment.points.Count; i++)
                {
                    if (segment.points[i].ellipseID == sender)
                    {
                        AddSegment_to_viewport(segment);

                        break;
                    }
                }
            }

        }
    }
}