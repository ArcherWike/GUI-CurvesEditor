using Curves_editor.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using UiDesign;

namespace Curves_editor.Core.Class
{
    public enum BezierType
    {
        Line,
        Cubic,
        Quadratic,
    }
    public class Curve_point
    {
        public Point ellipse_positionID { get; set; }
        public Ellipse ellipseID { get; set; }

        public int default_segment_index { get; set; }

        public Curve_point(Point position, Ellipse ellipse)
        {
            ellipse_positionID = position;
            ellipseID = ellipse; 
        }
    }
        
    public class Segment_curve
    {

        BezierType cuveType = BezierType.Quadratic;

        PathGeometry myPathGeometry = new PathGeometry();
        Path segmentPath = new Path();

        public List<Curve_point> points = new List<Curve_point>();  //this check m-
        List<Line> m_lines = new List<Line>();

        int m_controlP_margin = 0;
        public bool visualadded = false;
        
        static Point GetCanvastToCoord(Point mousePosition)
        {
            Point result = new Point((mousePosition.X - 40) / 200, 4 - (mousePosition.Y / 200));

            return result;
        }
        static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200) + 40, 800 - pointPosition.Y * 200);

            return result;
        }

        public Segment_curve(Curve_point start_point, Curve_point end_point, BezierType bezierType = BezierType.Quadratic)
        {
            points.Add(start_point);
            points.Add(end_point);
            SetBezierType(bezierType);
        }
    
        public void SetBezierType(BezierType bezierType = BezierType.Quadratic)
        {
            //Setting up value of m_lines and control points for Bezier curve
            cuveType = bezierType;
            m_lines.Clear();

            switch (cuveType)
            {
                case BezierType.Line:

                    //calculation of location invisible control point to 1/2 length
                    Point controlStartPoint = GetCanvastToCoord(new Point(
                        points[0].ellipse_positionID.X, 
                        points[0].ellipse_positionID.Y));
                    Point controlEndPoint = GetCanvastToCoord(new Point(
                        points[points.Count() - 1].ellipse_positionID.X, 
                        points[points.Count() - 1].ellipse_positionID.Y));
                    Curve_point control_point = new Curve_point(GetCoordToCanvast(new Point(
                                Math.Abs(controlStartPoint.X + controlEndPoint.X) / 2,
                                Math.Abs(controlStartPoint.Y + controlEndPoint.Y) / 2)
                        ), new Ellipse());

                    List<Curve_point> newPoints = new List<Curve_point>();
                    newPoints.Add(points[0]);
                    newPoints.Add(control_point);
                    newPoints.Add(points[points.Count() - 1]);

                    points = newPoints;

                    break;

                case BezierType.Cubic:
                    
                    //calculation of location control points to 1/3 length
                    Point supportStartPoint1 = GetCanvastToCoord(new Point(
                        points[0].ellipse_positionID.X, points[0].ellipse_positionID.Y));
                    Point supportEndPoint1 = GetCanvastToCoord(new Point(
                        points[points.Count() - 1].ellipse_positionID.X,
                        points[points.Count() - 1].ellipse_positionID.Y));
                    Point supportCenterPoint1 = (new Point(
                                Math.Abs(supportStartPoint1.X + supportEndPoint1.X) / 2,
                                Math.Abs(supportStartPoint1.Y + supportEndPoint1.Y) / 2));

                    Curve_point control_point_1 = new Curve_point(
                        GetCoordToCanvast(new Point(
                            Math.Abs(supportStartPoint1.X + supportCenterPoint1.X) / 2,
                            Math.Abs(supportStartPoint1.Y + supportCenterPoint1.Y) / 2)),
                        new Ellipse());

                    Curve_point control_point_2 = new Curve_point(
                        GetCoordToCanvast(new Point(
                            Math.Abs(supportCenterPoint1.X + supportEndPoint1.X) / 2,
                            Math.Abs(supportCenterPoint1.Y + supportEndPoint1.Y) / 2)),
                        new Ellipse());

                    //create new list and add new generated point
                    List<Curve_point> newPoints2 = new List<Curve_point>();
                    newPoints2.Add(points[0]);
                    newPoints2.Add(control_point_1);
                    newPoints2.Add(control_point_2);
                    newPoints2.Add(points[points.Count() - 1]);

                    points = newPoints2;

                    //create m_lines conecting control and base points
                    m_lines.Add(CreateLine(
                        points[0].ellipse_positionID,
                        control_point_1.ellipse_positionID));
                    m_lines.Add(CreateLine(
                        points[points.Count() - 1].ellipse_positionID,
                        control_point_2.ellipse_positionID));

                    break;

                case BezierType.Quadratic:

                    //calculation of location control point to 1/2 length
                    Point controlStartPoint1 = GetCanvastToCoord(new Point(
                        points[0].ellipse_positionID.X, points[0].ellipse_positionID.Y));
                    Point controlEndPoint1 = GetCanvastToCoord(new Point(
                        points[points.Count() - 1].ellipse_positionID.X, 
                        points[points.Count() - 1].ellipse_positionID.Y));
                    Curve_point control_point1 = new Curve_point(GetCoordToCanvast(new Point(
                                Math.Abs(controlStartPoint1.X + controlEndPoint1.X) / 2,
                                Math.Abs(controlStartPoint1.Y + controlEndPoint1.Y) / 2)
                        ), new Ellipse());

                    List<Curve_point> newPoints1 = new List<Curve_point>();
                    newPoints1.Add(points[0]);
                    newPoints1.Add(control_point1);
                    newPoints1.Add(points[points.Count() - 1]);

                    points = newPoints1;

                    //create m_lines conecting control and base points
                    m_lines.Add(CreateLine(
                        points[0].ellipse_positionID,
                        control_point1.ellipse_positionID));
                    m_lines.Add(CreateLine(
                        control_point1.ellipse_positionID,
                        points[points.Count() - 1].ellipse_positionID));

                    break;
            }
        }

        public Curve_point UpdateControlPointPosition()
        {
            for (int i = 1; i < points.Count() - 1; i++) 
            { 
                if (points[i].ellipse_positionID.X - m_controlP_margin < points[0].ellipse_positionID.X)
                {
                    points[i].ellipse_positionID = new Point(
                        points[0].ellipse_positionID.X + m_controlP_margin, 
                        points[i].ellipse_positionID.Y);

                    return points[i];

                }
                else if (points[i].ellipse_positionID.X + m_controlP_margin > points[points.Count() - 1].ellipse_positionID.X)
                {
                    points[i].ellipse_positionID = new Point(
                        points[points.Count() - 1].ellipse_positionID.X - m_controlP_margin,
                        points[i].ellipse_positionID.Y);
                    return points[i];
                }
            }
            return null;
        }

        public Path SegmentGet_pathGeometry() //chech breakpoint //dla kazdego segmentu
        {          
            segmentPath.Stroke = Brushes.Red;
            segmentPath.StrokeThickness = 3;

            BezierSegment new_segment;///
            new_segment = new BezierSegment(
                    points[0].ellipse_positionID,
                    points[1].ellipse_positionID,
                    points[2].ellipse_positionID,
                    true);/////////////

            myPathGeometry.Clear();
            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = points[0].ellipse_positionID;

            switch (cuveType)
            {

                case BezierType.Line:
                    Point controlStartPoint = GetCanvastToCoord(new Point(points[0].ellipse_positionID.X, points[0].ellipse_positionID.Y));
                    Point controlEndPoint = GetCanvastToCoord(new Point(points[points.Count() - 1].ellipse_positionID.X, points[points.Count() - 1].ellipse_positionID.Y));
                    points[1].ellipse_positionID = GetCoordToCanvast(new Point(
                                Math.Abs(controlStartPoint.X + controlEndPoint.X) / 2,
                                Math.Abs(controlStartPoint.Y + controlEndPoint.Y) / 2));


                   /* QuadraticBezierSegment();*/
                    

                    new_segment = new BezierSegment(
                        points[0].ellipse_positionID,
                        points[1].ellipse_positionID,
                        points[2].ellipse_positionID,
                        true);
                    pathFigure.Segments.Add(new_segment);
                    myPathGeometry.Figures.Add(pathFigure);
                    break;

                case BezierType.Cubic:
                    //myBezierSegment.Points = myPointCollection;
                    
                    PointCollection myPointCollection = new PointCollection();


                    myPointCollection.Add(points[1].ellipse_positionID);
                    myPointCollection.Add(points[2].ellipse_positionID);
                    myPointCollection.Add(points[3].ellipse_positionID);
                    /*foreach (Curve_point point in points)
                    {
                        myPointCollection.Add(point.ellipse_positionID);
                    }*/

                    PolyBezierSegment myBezierSegment = new PolyBezierSegment();
                    myBezierSegment.Points = myPointCollection;

                    PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
                    myPathSegmentCollection.Add(myBezierSegment);

                    pathFigure.Segments = myPathSegmentCollection;

                    PathFigureCollection myPathFigureCollection = new PathFigureCollection();
                    myPathFigureCollection.Add(pathFigure);


                    myPathGeometry.Figures = myPathFigureCollection;
                    /*PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
                    myPathSegmentCollection.Add(myBezierSegment);*/

                    //pathFigure.Segments.Add(myBezierSegment);

                    break;

                case BezierType.Quadratic:
                    new_segment = new BezierSegment(
                    points[0].ellipse_positionID,
                    points[1].ellipse_positionID,
                    points[2].ellipse_positionID,
                    true);
                    pathFigure.Segments.Add(new_segment);
                    myPathGeometry.Figures.Add(pathFigure);
                    break;             
            }
            
            segmentPath.Data = myPathGeometry;
            return segmentPath;
        }

        public List<Curve_point> GetControlPointArray()
        {
            List<Curve_point> controlPointArray = new List<Curve_point>();
            
            if (cuveType == BezierType.Line)
            {
                return controlPointArray;
            }

            for (int i = 0; i < points.Count() - 2; i++)
            {
                controlPointArray.Add(points[i + 1]);
            }
            return controlPointArray;
        }

        private Line CreateLine(Point start, Point end)
        {
            Line line = new Line();

            line.X1 = start.X - 5;
            line.Y1 = start.Y - 5;
            line.X2 = end.X - 5;
            line.Y2 = end.Y - 5;

            return line;
        }
        public List<Line> GetUpdatedLinesArray()
        {
            switch (cuveType)
            {
                case BezierType.Line:
                    break;
                case BezierType.Cubic:
                    m_lines[0].X1 = points[0].ellipse_positionID.X;
                    m_lines[0].Y1 = points[0].ellipse_positionID.Y;
                    m_lines[0].X2 = points[1].ellipse_positionID.X - 5;
                    m_lines[0].Y2 = points[1].ellipse_positionID.Y - 5;

                    m_lines[1].X1 = points[2].ellipse_positionID.X - 5;
                    m_lines[1].Y1 = points[2].ellipse_positionID.Y - 5;
                    m_lines[1].X2 = points[3].ellipse_positionID.X;
                    m_lines[1].Y2 = points[3].ellipse_positionID.Y;
                    break;
                case BezierType.Quadratic:
                    m_lines[0].X1 = points[0].ellipse_positionID.X;
                    m_lines[0].Y1 = points[0].ellipse_positionID.Y;
                    m_lines[0].X2 = points[1].ellipse_positionID.X - 5;
                    m_lines[0].Y2 = points[1].ellipse_positionID.Y - 5;

                    m_lines[1].X1 = points[1].ellipse_positionID.X - 5;
                    m_lines[1].Y1 = points[1].ellipse_positionID.Y - 5;
                    m_lines[1].X2 = points[2].ellipse_positionID.X;
                    m_lines[1].Y2 = points[2].ellipse_positionID.Y;
                    break;
            }
            return m_lines;
        }   

        public void SetControlPMargin(int value = 0)
        {
            m_controlP_margin = value;
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

    public class CurvePointsListEventArgs : EventArgs
    {
        public List<Curve_point> curvePointArray { get; private set; }
        public CurvePointsListEventArgs(List<Curve_point> point_) 
        { 
            curvePointArray = point_;
        }
    }

    public class CurvePointEventArgs : EventArgs
    {
        public Curve_point curvePoint { get; set; }

        public CurvePointEventArgs(Curve_point curvePoint_) 
        {
            curvePoint = new Curve_point(curvePoint_.ellipse_positionID, curvePoint_.ellipseID);
        }
    }

    internal class Curve
    {
        BezierType globalCuveType = BezierType.Quadratic;

        //----------------------------------------Arrays keeping points
        List<Curve_point> m_pointArray = new List<Curve_point>();
        List<Curve_point> m_base_pointArray = new List<Curve_point>();
        List<Curve_point> m_control_pointArray = new List<Curve_point>();

        //----------------------------------------Arrays keeping segments
        List<Segment_curve> m_segmentsArray = new List<Segment_curve>();

        int m_point_margin = 30; //distance of minimum gap between points

        //----------------------------------------Events
        public delegate void PathHandler(object sender, PathEventArgs e);
        public event PathHandler PathGeomertyAddToViewport;

        public delegate void LineHandler(object sender, LineEventArgs e);
        public event LineHandler OnLineAdded;
        public event LineHandler DestroyLine;

        public delegate void CurvePointsListHandler(object sender, CurvePointsListEventArgs e);
        public event CurvePointsListHandler OnCurvePointAdded;
        public event CurvePointsListHandler DestroyCurvePoint;

        public delegate void CurvePointHandler(object sender, CurvePointEventArgs e);
        public event CurvePointHandler CurvePointMove;
        //----------------------------------------
        public List<Curve_point> SortPoints(List<Curve_point> array)
        {
            for (int step = 1; step < array.Count; step++)
            {
                Curve_point key = new Curve_point(
                    array[step].ellipse_positionID, 
                    array[step].ellipseID);
                int j = step - 1;

                Point swapPoint = new Point(0, 0);
                Ellipse swapEllipse = new Ellipse();

                while (j >= 0 && key.ellipse_positionID.X < array[j].ellipse_positionID.X)
                {
                    swapPoint = new Point(
                        array[j].ellipse_positionID.X,
                        array[j].ellipse_positionID.Y);

                    swapEllipse = array[j].ellipseID;

                    array[j + 1].ellipse_positionID = swapPoint;
                    array[j + 1].ellipseID = swapEllipse;

                    j--;

                }

                swapPoint = new Point(
                            key.ellipse_positionID.X,
                            key.ellipse_positionID.Y);

                swapEllipse = key.ellipseID;

                array[j + 1].ellipse_positionID = swapPoint;
                array[j + 1].ellipseID = swapEllipse;
            }
            return array;
        }
        
        public static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200) + 40, 800 - pointPosition.Y * 200);

            return result;
        }

        void UpdateSegment(Segment_curve segment)
        {
            Curve_point point_to_update = segment.UpdateControlPointPosition();
            if (point_to_update != null)
            {
                CurvePointEventArgs eventArgs = new CurvePointEventArgs(point_to_update);
                CurvePointMove(this, eventArgs);
            }
            if (!segment.visualadded)
            {
                PathEventArgs eventArgs = new PathEventArgs(segment.SegmentGet_pathGeometry());
                PathGeomertyAddToViewport(this, eventArgs);
                //AddSegment_to_viewport(segment);

                segment.visualadded = true;
            }
            else
            {
                segment.SegmentGet_pathGeometry();
                segment.GetUpdatedLinesArray();
            }
        }

        void ChangePointPosition(Point point, Curve_point curve_point)
        {
            curve_point.ellipse_positionID = point;
            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_point);
            CurvePointMove(this, eventArgs);
        }

        void AddSegment_to_viewport(Segment_curve segment)
        {   //updating segments 
            PathEventArgs eventArgs = new PathEventArgs(segment.SegmentGet_pathGeometry());
            PathGeomertyAddToViewport(this, eventArgs);
        }
        public void ChangeSegmentBezierType(BezierType bezierType)
        {
            globalCuveType = bezierType;

            foreach (Segment_curve segment in m_segmentsArray)
            {   //destroy old points, m_lines from editor
                LineEventArgs eventArgs = new LineEventArgs(segment.GetUpdatedLinesArray());
                DestroyLine(this, eventArgs);

                CurvePointsListEventArgs eventArgs1 = new CurvePointsListEventArgs(
                    segment.GetControlPointArray());
                DestroyCurvePoint(this, eventArgs1);

                //update and create new segment
                segment.SetBezierType(bezierType);

                eventArgs = new LineEventArgs(segment.GetUpdatedLinesArray());
                OnLineAdded(this, eventArgs);

                eventArgs1 = new CurvePointsListEventArgs(segment.GetControlPointArray());
                foreach (Curve_point curvepoint in eventArgs1.curvePointArray)
                {
                    m_pointArray.Add(curvepoint);
                }
                OnCurvePointAdded(this, eventArgs1);

                segment.SegmentGet_pathGeometry();
            }
        }

        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point curve_Point = new Curve_point(point, ellipse);
            m_pointArray.Add(curve_Point);

            if (m_base_pointArray.Count() > 0)
            {
                Segment_curve segment_Curve = new Segment_curve(
                    m_base_pointArray[m_base_pointArray.Count() - 1], 
                    curve_Point, globalCuveType);
                m_segmentsArray.Add(segment_Curve);
                segment_Curve.SetControlPMargin(m_point_margin + 20);
      
                LineEventArgs eventArgs = new LineEventArgs(segment_Curve.GetUpdatedLinesArray());
                OnLineAdded(this, eventArgs);

                curve_Point.default_segment_index = m_segmentsArray.Count();

                CurvePointsListEventArgs eventArgs1 = new CurvePointsListEventArgs(segment_Curve.GetControlPointArray());
                foreach (Curve_point curvepoint in eventArgs1.curvePointArray)
                {
                    curvepoint.default_segment_index = curve_Point.default_segment_index;
                    m_pointArray.Add(curvepoint);
                    m_control_pointArray.Add(curvepoint);
                }
                OnCurvePointAdded(this, eventArgs1);

                //AddSegment_to_viewport(segment_Curve);

                /*if (m_base_pointArray[m_base_pointArray.Count() - 1].default_segment == null)
                {

                    m_base_pointArray[m_base_pointArray.Count() - 1].default_segment = segment_Curve;
                    
                }*/

            }
            
            m_base_pointArray.Add(curve_Point);
        }
        public void UpdatePointPosition(Ellipse sender, Point e)
        {
            bool executed = false;
            int index_pointArray = 0;


            //Curve_point moved_point = null;

            int activeSegmentIndex = 0;

            //change the position of moved point
            foreach (Curve_point curve_Point in m_base_pointArray)
            {
                //Find the displaced point
                if (sender == curve_Point.ellipseID)
                {
                    activeSegmentIndex = curve_Point.default_segment_index;
                    //first point - move
                    if (index_pointArray == 0)
                    {
                        if (m_pointArray.Count >= 2) 
                        { 
                            if (GetCoordToCanvast(e).X + m_point_margin <= 
                                m_base_pointArray[index_pointArray + 1].ellipse_positionID.X)
                            {
                                ChangePointPosition(GetCoordToCanvast(e), curve_Point);
                            }
                            else
                            {
                                ChangePointPosition(new Point(
                                    (curve_Point.ellipse_positionID.X),
                                    GetCoordToCanvast(e).Y),
                                    curve_Point);
                            }
                        }
                        else
                        {
                            curve_Point.ellipse_positionID = GetCoordToCanvast(e);
                            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_Point);
                            CurvePointMove(this, eventArgs);
                        }
                    }
                    //last point
                    else if (index_pointArray == (m_base_pointArray.Count() - 1))
                    {
                        if (GetCoordToCanvast(e).X - m_point_margin >= m_base_pointArray[index_pointArray - 1].ellipse_positionID.X)
                        {
                            curve_Point.ellipse_positionID = GetCoordToCanvast(e);
                            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_Point);
                            CurvePointMove(this, eventArgs);
                        }
                        else
                        {
                            curve_Point.ellipse_positionID = new Point(
                                curve_Point.ellipse_positionID.X,
                                GetCoordToCanvast(e).Y);
                            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_Point);
                            CurvePointMove(this, eventArgs);
                        }
                    }
                    //point in the middle
                    else
                    {
                        if (GetCoordToCanvast(e).X - m_point_margin >= m_base_pointArray[index_pointArray - 1].ellipse_positionID.X
                            && GetCoordToCanvast(e).X + m_point_margin <= m_base_pointArray[index_pointArray + 1].ellipse_positionID.X)
                        {
                            //between points - in are
                            curve_Point.ellipse_positionID = GetCoordToCanvast(e);
                            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_Point);
                            CurvePointMove(this, eventArgs);
                        }
                        else
                        {//outside the possible area
                            curve_Point.ellipse_positionID = new Point(
                                curve_Point.ellipse_positionID.X,
                                GetCoordToCanvast(e).Y);
                            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_Point);
                            CurvePointMove(this, eventArgs);
                        }
                    }
                    executed = true;
                    break;
                }
                index_pointArray++;
            }

            //find and update control point
            if (!executed)
            {
                for (int i = 0; i < m_control_pointArray.Count(); i++)
                {
                    if (m_control_pointArray[i].ellipseID == sender)
                    {
                        m_control_pointArray[i].ellipse_positionID = GetCoordToCanvast(e);
                        CurvePointEventArgs eventArgs = new CurvePointEventArgs(m_control_pointArray[i]);
                        CurvePointMove(this, eventArgs);
                        activeSegmentIndex = m_control_pointArray[i].default_segment_index;

                        break;
                    }
                }
            }

            if (activeSegmentIndex < m_segmentsArray.Count())
            {
                if (activeSegmentIndex > 0)
                {
                    UpdateSegment(m_segmentsArray[activeSegmentIndex - 1]);
                }
                UpdateSegment(m_segmentsArray[activeSegmentIndex]);         
            }
        }
    }
}