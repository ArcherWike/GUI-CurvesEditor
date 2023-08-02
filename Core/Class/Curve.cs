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
        PathGeometry myPathGeometry = new PathGeometry();
        Path segmentPath = new Path();

        public List<Curve_point> points = new List<Curve_point>();
        public List<Line> lines = new List<Line>();

        private static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200) + 40, 800 - pointPosition.Y * 200);

            return result;
        }

        public Segment_curve(Curve_point start_point, Curve_point end_point)
        {
            points.Add(start_point);

            Point newEndPos = GetCoordToCanvast(end_point.ellipse_positionID);
            Point newStartPos = GetCoordToCanvast(start_point.ellipse_positionID);

            Curve_point control_point = new Curve_point(new Point(
                start_point.ellipse_positionID.X + (Math.Abs(end_point.ellipse_positionID.X - start_point.ellipse_positionID.X)/2),
                start_point.ellipse_positionID.Y + (Math.Abs(end_point.ellipse_positionID.Y - start_point.ellipse_positionID.Y)/2)),
                new Ellipse());

            /*Curve_point control_point = new Curve_point(new Point(
                Math.Abs(newEndPos.X - newStartPos.X),
                Math.Abs(newEndPos.Y - newEndPos.Y)),
                new Ellipse());*/

            points.Add(control_point);
            
            points.Add(end_point);

            lines.Add(CreateLine(
                start_point.ellipse_positionID,
                control_point.ellipse_positionID));
            lines.Add(CreateLine(
                control_point.ellipse_positionID,
                end_point.ellipse_positionID));
        }
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

            return segmentPath;
        }

        public Curve_point GetControlPoint()
        {
            return points[1];
        }

        private Line CreateLine(Point start, Point end)
        {
            Line line = new Line();

            line.X1 = start.X;
            line.Y1 = start.Y;
            line.X2 = end.X;
            line.Y2 = end.Y;

            return line;
        }
        public List<Line> GetUpdatedLinesArray()
        {
            lines[0].X1 = points[0].ellipse_positionID.X;
            lines[0].Y1 = points[0].ellipse_positionID.Y;
            lines[0].X2 = points[1].ellipse_positionID.X;
            lines[0].Y2 = points[1].ellipse_positionID.Y;

            lines[1].X1 = points[1].ellipse_positionID.X;
            lines[1].Y1 = points[1].ellipse_positionID.Y;
            lines[1].X2 = points[2].ellipse_positionID.X;
            lines[1].Y2 = points[2].ellipse_positionID.Y;

            return lines;
        }
        private void Create_controlPoint()
        {
            

            /*Curve_point end_point = points[points.Count - 1];

            points[points.Count() - 1] = new_control_point;
            points.Add(end_point);*/
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

    public class LineEventArgs : EventArgs
    {
        public List<Line> linesArray { get; private set; }
        public LineEventArgs(List<Line> line_)
        {
            linesArray = line_;
        }
    }

    public class CurvePointEventArgs : EventArgs
    {
        public Curve_point curvePoint { get; private set; }
        public CurvePointEventArgs(Curve_point point_) 
        { 
            curvePoint = point_;
        }
    }

    internal class Curve
    {
        public List<Curve_point> base_pointArray = new List<Curve_point>();
        public List<Curve_point> pointArray = new List<Curve_point>();
        public List<Segment_curve> segmentsArray = new List<Segment_curve>();

        public delegate void PathHandler(object sender, PathEventArgs e);
        public event PathHandler PathGeomertyAddToViewport;

        public delegate void LineHandler(object sender, LineEventArgs e);
        public event LineHandler OnLineAdded;
        public event LineHandler UpdateLine;

        public delegate void CurvePointHandler(object sender, CurvePointEventArgs e);
        public event CurvePointHandler OnCurvePointAdded;

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
        

        public void DeleteCurves()
        {

            base_pointArray = new List<Curve_point>();
            segmentsArray = new List<Segment_curve>();
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

            if (base_pointArray.Count() > 0)
            {
                Segment_curve segment_Curve = new Segment_curve(base_pointArray[base_pointArray.Count() - 1], curve_Point);
                segmentsArray.Add(segment_Curve);

                LineEventArgs eventArgs = new LineEventArgs(segment_Curve.GetUpdatedLinesArray());
                OnLineAdded(this, eventArgs);

                CurvePointEventArgs eventArgs1 = new CurvePointEventArgs(segment_Curve.GetControlPoint());
                OnCurvePointAdded(this, eventArgs1);
                pointArray.Add(eventArgs1.curvePoint);

                AddSegment_to_viewport(segment_Curve);
            }

            base_pointArray.Add(curve_Point);
        }

        private void OnSegmentLineAdded(object sender, LineEventArgs e)
        {
           
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
            foreach (Segment_curve segment in segmentsArray)
            {
                for (int i = 0; i < segment.points.Count; i++)
                {
                    if (segment.points[i].ellipseID == sender)
                    {
                        AddSegment_to_viewport(segment);

                        LineEventArgs eventArgs = new LineEventArgs(segment.GetUpdatedLinesArray());
                        //OnLineAdded(this, eventArgs);
                        break;
                    }
                }
            }

        }
    }
}