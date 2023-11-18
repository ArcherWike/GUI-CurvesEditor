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
using System.Windows.Input;
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
        private const double c_sampleSpan = 5;
        BezierType cuveType = BezierType.Quadratic;

        public List<Curve_point> points = new List<Curve_point>();  //this check m-
        List<Line> m_lines = new List<Line>();
        private Polyline curveGeometry = new Polyline();

        int m_controlP_margin = 0;

        PointCollection interpolator_points = new PointCollection();

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

                    List<Curve_point> newPoints = new List<Curve_point>();

                    newPoints.Add(points[0]);

                    Point controlStartPoint = GetCanvastToCoord(new Point(points[0].ellipse_positionID.X, points[0].ellipse_positionID.Y));
                    Point controlEndPoint = GetCanvastToCoord(new Point(points[points.Count() - 1].ellipse_positionID.X, points[points.Count() - 1].ellipse_positionID.Y));
                    Curve_point control_point = new Curve_point(GetCoordToCanvast(new Point(
                                Math.Abs(controlStartPoint.X + controlEndPoint.X) / 2,
                                Math.Abs(controlStartPoint.Y + controlEndPoint.Y) / 2)
                        ), new Ellipse());
                    newPoints.Add(control_point);

                    newPoints.Add(points[points.Count() - 1]);

                    points = newPoints;

                    break;

                case BezierType.Cubic:

                    List<Curve_point> newPoints2 = new List<Curve_point>();

                    newPoints2.Add(points[0]);
                    Curve_point control_point_1 = new Curve_point(new Point(
                        points[0].ellipse_positionID.X + 1, points[0].ellipse_positionID.Y + 1),
                        new Ellipse());

                    Curve_point control_point_2 = new Curve_point(new Point(
                        points[points.Count() - 1].ellipse_positionID.X + 1, points[points.Count() - 1].ellipse_positionID.Y + 1),
                        new Ellipse());

                    newPoints2.Add(control_point_1);
                    newPoints2.Add(control_point_2);

                    newPoints2.Add(points[points.Count() - 1]);

                    points = newPoints2;

                    m_lines.Add(CreateLine(
                        points[0].ellipse_positionID,
                        control_point_1.ellipse_positionID));
                    m_lines.Add(CreateLine(
                        points[points.Count() - 1].ellipse_positionID,
                        control_point_2.ellipse_positionID));

                    break;

                case BezierType.Quadratic:

                    List<Curve_point> newPoints1 = new List<Curve_point>();

                    newPoints1.Add(points[0]);

                    Point controlStartPoint1 = GetCanvastToCoord(new Point(points[0].ellipse_positionID.X, points[0].ellipse_positionID.Y));
                    Point controlEndPoint1 = GetCanvastToCoord(new Point(points[points.Count() - 1].ellipse_positionID.X, points[points.Count() - 1].ellipse_positionID.Y));
                    Curve_point control_point1 = new Curve_point(GetCoordToCanvast(new Point(
                                Math.Abs(controlStartPoint1.X + controlEndPoint1.X) / 2,
                                Math.Abs(controlStartPoint1.Y + controlEndPoint1.Y) / 2)
                        ), new Ellipse());
                    newPoints1.Add(control_point1);

                    newPoints1.Add(points[points.Count() - 1]);

                    points = newPoints1;

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

        public Shape GetCurveGeometry()
        {
            curveGeometry.Stroke = Brushes.Black;
            curveGeometry.StrokeThickness = 3;

            PointCollection curvePoints = new PointCollection();
            switch (cuveType)
            {
                case BezierType.Line:
                    curvePoints.Add(points[0].ellipse_positionID);
                    curvePoints.Add(points.Last().ellipse_positionID);
                    break;

                case BezierType.Cubic:
                    {
                        int samplesCount = (int)((points[3].ellipse_positionID.X - points[0].ellipse_positionID.X) / c_sampleSpan);
                        double localDt = 1.0 / samplesCount, t = 0.0;

                        while (true)
                        {
                            curvePoints.Add(new Point(
                                Interpolator.CubicBezier(points[0].ellipse_positionID.X, points[1].ellipse_positionID.X,
                                        points[2].ellipse_positionID.X, points[3].ellipse_positionID.X, t),
                                Interpolator.CubicBezier(points[0].ellipse_positionID.Y, points[1].ellipse_positionID.Y,
                                        points[2].ellipse_positionID.Y, points[3].ellipse_positionID.Y, t)));

                            if (t >= 1.0)
                            {
                                break;
                            }

                            t += localDt;
                            t = Math.Min(t, 1.0);
                        }
                    }
                    break;

                case BezierType.Quadratic:
                    {
                        int samplesCount = (int)((points[2].ellipse_positionID.X - points[0].ellipse_positionID.X) / c_sampleSpan);
                        double localDt = 1.0 / samplesCount, t = 0.0;

                        while (true)
                        {
                            curvePoints.Add(new Point(
                                Interpolator.QuadraticBezier(points[0].ellipse_positionID.X,
                                        points[1].ellipse_positionID.X, points[2].ellipse_positionID.X, t),
                                Interpolator.QuadraticBezier(points[0].ellipse_positionID.Y,
                                        points[1].ellipse_positionID.Y, points[2].ellipse_positionID.Y, t)));

                            if (t >= 1.0)
                            {
                                break;
                            }

                            t += localDt;
                            t = Math.Min(t, 1.0);
                        }
                    }
                    break;
            }
            curveGeometry.Points = curvePoints;

            interpolator_points = curvePoints;
            return curveGeometry;
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

        public float GetValueAt(float time)
        {
            if (interpolator_points[0].X == time)
            {
                return (float)interpolator_points[0].Y;
            }

            int low_ix = 0;
            int high_ix = interpolator_points.Count() - 1;


            while (low_ix + 1 < high_ix)
            {
                int midpoint_ix = (int)((high_ix + low_ix) / 2);

                double x = interpolator_points[midpoint_ix].X;

                if (time <= x)
                {
                    high_ix = midpoint_ix;
                }
                else
                {
                    low_ix = midpoint_ix;
                }

            }

            float factor = (float)(
                (time - interpolator_points[low_ix].X) /
                (interpolator_points[high_ix].X - interpolator_points[low_ix].X));
            return (float)(interpolator_points[low_ix].Y + factor *
                (interpolator_points[high_ix].Y -
                interpolator_points[low_ix].Y));
            return (float)(interpolator_points[low_ix].Y);
        }
    }

    public class PathEventArgs : EventArgs
    {
        public Shape pathGeometry { get; private set; }
        public PathEventArgs(Shape path_)
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

    public class Curve
    {
        BezierType globalCuveType = BezierType.Quadratic;

        //----------------------------------------Arrays keeping points
        List<Curve_point> m_pointArray = new List<Curve_point>();
        List<Curve_point> m_base_pointArray = new List<Curve_point>();
        List<Curve_point> m_control_pointArray = new List<Curve_point>();

        //----------------------------------------Arrays keeping segments
        List<Segment_curve> m_segmentsArray = new List<Segment_curve>();

        int m_point_margin = 40; //distance of minimum gap between points

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
                int j = step - 1;

                if (array[step].ellipse_positionID.X < array[j].ellipse_positionID.X)
                {
                    /*Curve_point temp = array[j];
                    array[j] = array[step]; 
                    array[step] = temp;*/

                    Curve_point key = new Curve_point(
                        array[j].ellipse_positionID,
                        array[j].ellipseID);
                    key.default_segment_index = array[j].default_segment_index;

                    //Curve_point temp = array[j];
                    array[j].ellipseID = array[step].ellipseID;
                    array[j].ellipse_positionID = array[step].ellipse_positionID;
                    array[j].default_segment_index = array[step].default_segment_index;

                    array[step].ellipseID = key.ellipseID;
                    array[step].ellipse_positionID = key.ellipse_positionID;
                    array[step].default_segment_index = key.default_segment_index;
                    break;


                }
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
            segment.GetCurveGeometry();
            segment.GetUpdatedLinesArray();     
        }

        void ChangePointPosition(Point point, Curve_point curve_point)
        {
            curve_point.ellipse_positionID = point;
            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_point);
            CurvePointMove(this, eventArgs);
        }

        void AddSegment_to_viewport(Segment_curve segment)
        {   //updating segments 
            PathEventArgs eventArgs = new PathEventArgs(segment.GetCurveGeometry());
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

                segment.GetCurveGeometry();
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
                segment_Curve.SetControlPMargin(m_point_margin - 20);

                curve_Point.default_segment_index = m_segmentsArray.Count();

                LineEventArgs eventArgs = new LineEventArgs(segment_Curve.GetUpdatedLinesArray());
                OnLineAdded(this, eventArgs);


                CurvePointsListEventArgs eventArgs1 = new CurvePointsListEventArgs(segment_Curve.GetControlPointArray());
                foreach (Curve_point curvepoint in eventArgs1.curvePointArray)
                {
                    curvepoint.default_segment_index = curve_Point.default_segment_index;
                    m_pointArray.Add(curvepoint);
                    m_control_pointArray.Add(curvepoint);
                }
                OnCurvePointAdded(this, eventArgs1);

                AddSegment_to_viewport(segment_Curve);
            }           
            m_base_pointArray.Add(curve_Point);

            /*m_base_pointArray = SortPoints(m_base_pointArray);
            foreach (Segment_curve ssegment in m_segmentsArray)
            {
                UpdateSegment(ssegment);
            }*/
            
        }
        public void UpdatePointPosition(Ellipse sender, Point e)
        {
            bool executed = false;
            int activeSegmentIndex = 0;
            int index_pointArray = 0;
            //change the position of moved point (if moved base P)
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
                        //No points more, move freely
                        {
                            ChangePointPosition(GetCoordToCanvast(e), curve_Point);
                        }
                    }
                    //last point
                    else if (index_pointArray == (m_base_pointArray.Count() - 1))
                    {
                        if (GetCoordToCanvast(e).X - m_point_margin >= m_base_pointArray[index_pointArray - 1].ellipse_positionID.X)
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
                    //point in the middle
                    else
                    {
                        if (GetCoordToCanvast(e).X - m_point_margin >= 
                            m_base_pointArray[index_pointArray - 1].ellipse_positionID.X
                            && 
                            GetCoordToCanvast(e).X + m_point_margin <= 
                            m_base_pointArray[index_pointArray + 1].ellipse_positionID.X)
                        {
                            //between points - in are
                            ChangePointPosition(GetCoordToCanvast(e), curve_Point);
                        }
                        else
                        {//outside the possible area
                            ChangePointPosition(new Point(
                            (curve_Point.ellipse_positionID.X),
                            GetCoordToCanvast(e).Y),
                            curve_Point);
                        }
                    }
                    executed = true;
                    break;
                }
                index_pointArray++;
            }

            //find and update control point (if moved control P)
            if (!executed)
            {
                for (int i = 0; i < m_control_pointArray.Count(); i++)
                {
                    if (m_control_pointArray[i].ellipseID == sender)
                    {
                        ChangePointPosition(GetCoordToCanvast(e), m_control_pointArray[i]);
                        activeSegmentIndex = m_control_pointArray[i].default_segment_index;
                        break;
                    }
                }
            }

            //Update closest segments (points and curves - lines)
            if (activeSegmentIndex < m_segmentsArray.Count())
            {
                if (activeSegmentIndex > 0)
                {
                    UpdateSegment(m_segmentsArray[activeSegmentIndex - 1]);
                }
                UpdateSegment(m_segmentsArray[activeSegmentIndex]);         
            }
            else if (activeSegmentIndex > 0) 
            {
                UpdateSegment(m_segmentsArray[m_segmentsArray.Count() - 1]);
            }
        }
        public float GetValueAt(float time)
        {
            if (m_segmentsArray.Any())
            {
                if (time >= m_segmentsArray.First().points.First().ellipse_positionID.X &&
                    time <= m_segmentsArray.Last().points.Last().ellipse_positionID.X)
                {
                    foreach (Segment_curve segment_ in m_segmentsArray)
                    {
                        if ( time <= segment_.points.Last().ellipse_positionID.X )
                        {
                            return segment_.GetValueAt(time);
                        }
                    }
                } 
            }
            return 0f;
        }
    }
}
