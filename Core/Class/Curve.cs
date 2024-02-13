using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Curves_editor.Core.Class
{
    public enum BezierType
    {
        Line,
        Cubic,
        Quadratic,
    }

    public enum ColorType
    {
        Alpha,
        Red,
        Green,
        Blue,
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
        //-- curve settings
        private const double c_sampleSpan = 5;
        BezierType curveType = BezierType.Quadratic;
        ColorType curveColor = ColorType.Alpha;
        public bool visible_mode = true;
        private int m_controlP_margin = 0;

        //-- curve content
        public List<Curve_point> points = new List<Curve_point>();
        List<Line> m_lines = new List<Line>();
        private Polyline curveGeometry = new Polyline();

        PointCollection interpolator_points = new PointCollection();

        ///???????????????????????????????????????????????????????????????????????funkcje CordToCanvas
        static Point GetCanvastToCoord(Point mousePosition)
        {
            Point result = new Point((mousePosition.X) / 200, 4 - (mousePosition.Y / 200));

            return result;
        }   
        static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200), 800 - pointPosition.Y * 200);

            return result;
        }

        public Segment_curve(Curve_point start_point, Curve_point end_point,
            BezierType bezierType = BezierType.Quadratic,
            ColorType colorType = ColorType.Alpha)
        {
            points.Add(start_point);
            points.Add(end_point);
            SetBezierType(bezierType);
        }
        public void SetColorType(ColorType colorType)
        {
            curveColor = colorType;
        }

        public void SetBezierType(BezierType bezierType = BezierType.Quadratic)
        {
            //Setting up value of m_lines and control points for Bezier curve
            curveType = bezierType;
            m_lines.Clear();

            switch (curveType)
            {
                case BezierType.Line:

                    List<Curve_point> newPoints = new List<Curve_point>();

                    newPoints.Add(points[0]);

                    Point controlStartPoint = GetCanvastToCoord(new Point(points[0].ellipse_positionID.X, points[0].ellipse_positionID.Y));
                    Point controlEndPoint = GetCanvastToCoord(new Point(points.Last().ellipse_positionID.X, points.Last().ellipse_positionID.Y));
                    Curve_point control_point = new Curve_point(GetCoordToCanvast(new Point(
                                Math.Abs(controlStartPoint.X + controlEndPoint.X) / 2,
                                Math.Abs(controlStartPoint.Y + controlEndPoint.Y) / 2)
                        ), new Ellipse());
                    newPoints.Add(control_point);

                    newPoints.Add(points.Last());

                    points = newPoints;

                    break;

                case BezierType.Cubic:

                    List<Curve_point> newPoints2 = new List<Curve_point>();

                    newPoints2.Add(points[0]);

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

                    newPoints2.Add(control_point_1);
                    newPoints2.Add(control_point_2);

                    newPoints2.Add(points.Last());

                    points = newPoints2;

                    m_lines.Add(CreateLine(
                        points[0].ellipse_positionID,
                        control_point_1.ellipse_positionID));
                    m_lines.Add(CreateLine(
                        points.Last().ellipse_positionID,
                        control_point_2.ellipse_positionID));

                    break;

                case BezierType.Quadratic:

                    List<Curve_point> newPoints1 = new List<Curve_point>();

                    newPoints1.Add(points[0]);

                    Point controlStartPoint1 = GetCanvastToCoord(new Point(points[0].ellipse_positionID.X, points[0].ellipse_positionID.Y));
                    Point controlEndPoint1 = GetCanvastToCoord(new Point(points.Last().ellipse_positionID.X, points.Last().ellipse_positionID.Y));
                    Curve_point control_point1 = new Curve_point(GetCoordToCanvast(new Point(
                                Math.Abs(controlStartPoint1.X + controlEndPoint1.X) / 2,
                                Math.Abs(controlStartPoint1.Y + controlEndPoint1.Y) / 2)
                        ), new Ellipse());
                    newPoints1.Add(control_point1);

                    newPoints1.Add(points.Last());
                                        
                    points = newPoints1;

                    m_lines.Add(CreateLine(
                        points[0].ellipse_positionID,
                        control_point1.ellipse_positionID));
                    m_lines.Add(CreateLine(
                        control_point1.ellipse_positionID,
                        points.Last().ellipse_positionID));

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
                else if (points[i].ellipse_positionID.X + m_controlP_margin > points.Last().ellipse_positionID.X)
                {
                    points[i].ellipse_positionID = new Point(
                        points.Last().ellipse_positionID.X - m_controlP_margin,
                        points[i].ellipse_positionID.Y);
                    return points[i];
                }
            }
            return null;
        }

        public Shape GetCurveGeometry()
        {
            if (visible_mode)
            {
                switch (curveColor)
                {
                    case ColorType.Alpha:
                        curveGeometry.Stroke = new SolidColorBrush(Color.FromArgb(255, 231, 240, 240));
                        break;
                    case ColorType.Red:
                        curveGeometry.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 88, 68));
                        break;
                    case ColorType.Green:
                        curveGeometry.Stroke = Brushes.Green;
                        break;
                    case ColorType.Blue:
                        curveGeometry.Stroke = new SolidColorBrush(Color.FromArgb(255, 56, 155, 208));
                        break;
                }
                curveGeometry.Opacity = 1;
            }
            else
            {
                curveGeometry.Opacity = 0.75;
            }
            Set_control_point_visible_mode(visible_mode);
            curveGeometry.StrokeThickness = 6;

            PointCollection curvePoints = new PointCollection();
            switch (curveType)
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

            if (curveType == BezierType.Line)
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

            line.X1 = start.X;
            line.Y1 = start.Y;
            line.X2 = end.X;
            line.Y2 = end.Y;

            return line;
        }

        Line SetLineColor(Line line_)
        {
            if (visible_mode)
            {
                line_.Opacity = 0.80;
                line_.StrokeThickness = 3;
                line_.Stroke = Brushes.Gray;
            }
            else
            {
                line_.Opacity = 0;
            }
            return line_;
        }

        public void ChangeLinesColor()
        {
            for (int i = 0; i < m_lines.Count(); i++)
            {
                m_lines[i] = SetLineColor(m_lines[i]);
            }
        }

        public List<Line> GetUpdatedLinesArray()
        {
            switch (curveType)
            {
                case BezierType.Line:
                    break;
                case BezierType.Cubic:
                    m_lines[0].X1 = points[0].ellipse_positionID.X;
                    m_lines[0].Y1 = points[0].ellipse_positionID.Y;
                    m_lines[0].X2 = points[1].ellipse_positionID.X;
                    m_lines[0].Y2 = points[1].ellipse_positionID.Y;

                    m_lines[1].X1 = points[2].ellipse_positionID.X;
                    m_lines[1].Y1 = points[2].ellipse_positionID.Y;
                    m_lines[1].X2 = points[3].ellipse_positionID.X;
                    m_lines[1].Y2 = points[3].ellipse_positionID.Y;
                    break;
                case BezierType.Quadratic:
                    m_lines[0].X1 = points[0].ellipse_positionID.X;
                    m_lines[0].Y1 = points[0].ellipse_positionID.Y;
                    m_lines[0].X2 = points[1].ellipse_positionID.X;
                    m_lines[0].Y2 = points[1].ellipse_positionID.Y;

                    m_lines[1].X1 = points[1].ellipse_positionID.X;
                    m_lines[1].Y1 = points[1].ellipse_positionID.Y;
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

        private void Set_control_point_visible_mode(bool visible = true)
        {
            if (visible)
            {
                foreach (Curve_point control_point in GetControlPointArray())
                {
                    control_point.ellipseID.Opacity = 0.75;
                }
            }
            else
            {
                foreach (Curve_point control_point in GetControlPointArray())
                {
                    control_point.ellipseID.Opacity = 0;
                }
            }
        }

        //value at the chart point
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
        public ColorType globalCurveColor = ColorType.Alpha;

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

        public void ChangeVisibleCurve(bool visible_option)
        {
            foreach (Segment_curve segment in m_segmentsArray)
            {
                segment.visible_mode = visible_option;
                AddSegment_to_viewport(segment);
            }
        }

        void ChangePointPosition(Point point, Curve_point curve_point)
        {
            curve_point.ellipse_positionID = point;
            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_point);
            CurvePointMove(this, eventArgs);
        }
        void AddSegment_to_viewport(Segment_curve segment)
        {
            //updating segments 
            PathEventArgs eventArgs = new PathEventArgs(segment.GetCurveGeometry());
            segment.ChangeLinesColor();
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

                AddSegment_to_viewport(segment);
            }
        }
        public bool isPoint(Ellipse sender)
        {
            foreach (Curve_point curve_Point in m_pointArray)
            {
                //Find point
                if (sender == curve_Point.ellipseID)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point new_curve_point = new Curve_point(point, ellipse);

            if (m_base_pointArray.Count() > 0)
            {
                //point added at the end
                if (m_base_pointArray.Last().ellipse_positionID.X < point.X)
                {
                    m_pointArray.Add(new_curve_point);
                    Segment_curve segment_Curve = new Segment_curve(
                    m_base_pointArray.Last(),
                    new_curve_point, globalCuveType);
                    m_segmentsArray.Add(segment_Curve);
                    segment_Curve.SetColorType(globalCurveColor);
                    segment_Curve.SetControlPMargin(m_point_margin - 20);

                    new_curve_point.default_segment_index = m_segmentsArray.Count();

                    LineEventArgs eventArgs = new LineEventArgs(segment_Curve.GetUpdatedLinesArray());
                    OnLineAdded(this, eventArgs);

                    CurvePointsListEventArgs eventArgs1 = new CurvePointsListEventArgs(segment_Curve.GetControlPointArray());
                    foreach (Curve_point curvepoint in eventArgs1.curvePointArray)
                    {
                        curvepoint.default_segment_index = new_curve_point.default_segment_index;
                        m_pointArray.Add(curvepoint);
                        m_control_pointArray.Add(curvepoint);
                    }
                    OnCurvePointAdded(this, eventArgs1);

                    AddSegment_to_viewport(segment_Curve);
                }
                //point added at the centre of the curve
                else
                {
                    Curve_point temp = null;
                    Curve_point next_temp = null;

                    bool curve_points_swap = false;
                    int i = 0;
                    bool temp_mode = true;
                    //sort the points with respect to the new one added in the middle
                    while (i < m_base_pointArray.Count())
                    {
                        if (!curve_points_swap && m_base_pointArray[i].ellipse_positionID.X > new_curve_point.ellipse_positionID.X)
                        {
                            curve_points_swap = true;

                            next_temp = new Curve_point(
                                m_base_pointArray[i].ellipse_positionID,
                                m_base_pointArray[i].ellipseID
                                );

                            m_base_pointArray[i].ellipse_positionID = new_curve_point.ellipse_positionID;
                            m_base_pointArray[i].ellipseID = new_curve_point.ellipseID;

                            i++;

                            continue;
                        }
                        else if (curve_points_swap)
                        {
                            if (i > m_base_pointArray.Count() - 1)
                            {
                                break;
                            }

                            if (temp_mode)
                            {
                                temp = new Curve_point(
                                m_base_pointArray[i].ellipse_positionID,
                                m_base_pointArray[i].ellipseID
                                );

                                m_base_pointArray[i].ellipse_positionID = next_temp.ellipse_positionID;
                                m_base_pointArray[i].ellipseID = next_temp.ellipseID;
                            }
                            else
                            {
                                next_temp = new Curve_point(
                                m_base_pointArray[i].ellipse_positionID,
                                m_base_pointArray[i].ellipseID
                                );

                                m_base_pointArray[i].ellipse_positionID = temp.ellipse_positionID;
                                m_base_pointArray[i].ellipseID = temp.ellipseID;
                            }
                            temp_mode = !temp_mode;
                        }
                        i++;
                    }
                    if (!temp_mode)
                    {
                        new_curve_point.ellipse_positionID = temp.ellipse_positionID;
                        new_curve_point.ellipseID = temp.ellipseID;
                    }
                    else
                    {
                        new_curve_point.ellipse_positionID = next_temp.ellipse_positionID;
                        new_curve_point.ellipseID = next_temp.ellipseID;
                    }

                    UpdateSegment(m_segmentsArray.Last());

                    //create a new segment : new added point -> next point after this new one

                    new_curve_point.default_segment_index = m_segmentsArray.Count();

                    Segment_curve segment_Curve = new Segment_curve(
                    m_base_pointArray.Last(),
                    new_curve_point, globalCuveType);
                    m_segmentsArray.Add(segment_Curve);

                    segment_Curve.SetColorType(globalCurveColor);
                    segment_Curve.SetControlPMargin(m_point_margin - 20);

                    m_pointArray.Add(new_curve_point);
                    new_curve_point.default_segment_index = m_segmentsArray.Count();

                    LineEventArgs eventArgs = new LineEventArgs(segment_Curve.GetUpdatedLinesArray());
                    OnLineAdded(this, eventArgs);

                    CurvePointsListEventArgs eventArgs1 = new CurvePointsListEventArgs(segment_Curve.GetControlPointArray());
                    foreach (Curve_point curvepoint in eventArgs1.curvePointArray)
                    {
                        curvepoint.default_segment_index = new_curve_point.default_segment_index;
                        m_pointArray.Add(curvepoint);
                        m_control_pointArray.Add(curvepoint);
                    }
                    OnCurvePointAdded(this, eventArgs1);

                    AddSegment_to_viewport(segment_Curve);

                    foreach (Segment_curve segment_i in m_segmentsArray)
                    {
                        UpdateSegment(segment_i);
                    }
                }
            }
            else
            {
                m_pointArray.Add(new_curve_point);
                ChangePointPosition(new Point(0, point.Y), m_pointArray.Last());
            }
            m_base_pointArray.Add(new_curve_point);
        }
        public void UpdatePointPosition(Ellipse sender, Point e)
        {
            if ((GetCoordToCanvast(e).X > 0 && GetCoordToCanvast(e).X < 1240) && (GetCoordToCanvast(e).Y > 0 && GetCoordToCanvast(e).Y < 800))
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

                                    ChangePointPosition(
                                        new Point(0, GetCoordToCanvast(e).Y), curve_Point);
                                }
                                else
                                {
                                    ChangePointPosition(
                                        new Point(0, GetCoordToCanvast(e).Y), curve_Point);
                                }
                            }
                            else
                            //No points more, move freely
                            {
                                ChangePointPosition(
                                        new Point(0, GetCoordToCanvast(e).Y), curve_Point);
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
                                //between points
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
                    for (int i = 0; i < m_pointArray.Count(); i++)
                    {
                        if (m_pointArray[i].ellipseID == sender)
                        {
                            ChangePointPosition(GetCoordToCanvast(e), m_pointArray[i]);
                            activeSegmentIndex = m_pointArray[i].default_segment_index;
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
                    UpdateSegment(m_segmentsArray.Last());
                }
            }

            foreach (Segment_curve segment_ in m_segmentsArray)
            {
                UpdateSegment(segment_);
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
                        if (time <= segment_.points.Last().ellipse_positionID.X)
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
