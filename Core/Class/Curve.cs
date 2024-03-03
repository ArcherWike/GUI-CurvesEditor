using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using CanvasWindow;
using static System.Security.Cryptography.ECCurve;


namespace Curves_editor.Core.Class
{
    public enum CurveType
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
        public Point ellipse_position { get; set; }
        public Ellipse ellipse { get; set; }
        public int default_segment_index { get; set; }

        public Curve_point(Point position, Ellipse arg_ellipse)
        {
            ellipse_position = position;
            ellipse = arg_ellipse;
        }
    }
    public class Segment_curve
    {
        //-- curve settings
        private const double c_sampleSpan = 5;
        private CurveType m_curveType { get; set; } = CurveType.Quadratic;

        public ColorType m_curveColor { get; set; } = ColorType.Alpha;
        public bool m_visible_mode { get; set; } = true;
        private int m_controlP_margin = 0;

        //-- curve content
        public List<Curve_point> m_points = new List<Curve_point>();
        public List<Line> m_lines { get; private set; } = new List<Line>();
        public Polyline m_curveGeometry { get; private set; } = new Polyline();

        private PointCollection interpolator_points = new PointCollection();

        public Segment_curve(Curve_point start_point, Curve_point end_point,
            CurveType bezierType = CurveType.Quadratic,
            ColorType colorType = ColorType.Alpha)
        {
            m_points.Add(start_point);
            m_points.Add(end_point);
            SetBezierType(bezierType);

            m_curveColor = colorType;
        }
        
        private void CreateLineTypePoints()
        {
            List<Curve_point> newPoints = new List<Curve_point>();

            newPoints.Add(m_points[0]);

            Point controlStartPoint = Canvas_calculations.GetCanvastToCoord(
                new Point(
                    m_points[0].ellipse_position.X,
                    m_points[0].ellipse_position.Y));
            Point controlEndPoint = Canvas_calculations.GetCanvastToCoord(
                new Point(
                    m_points.Last().ellipse_position.X,
                    m_points.Last().ellipse_position.Y));
            Curve_point control_point = new Curve_point(
                Canvas_calculations.GetCoordToCanvast(
                    new Point(
                        Math.Abs(controlStartPoint.X + controlEndPoint.X) / 2,
                        Math.Abs(controlStartPoint.Y + controlEndPoint.Y) / 2)
                ), new Ellipse());
            newPoints.Add(control_point);

            newPoints.Add(m_points.Last());

            m_points = newPoints;
        }


        private void CreateCubicTypePoints()
        {
            List<Curve_point> newPoints2 = new List<Curve_point>();

            newPoints2.Add(m_points[0]);

            Point supportStartPoint1 = Canvas_calculations.GetCanvastToCoord(
                new Point(
                m_points[0].ellipse_position.X,
                m_points[0].ellipse_position.Y));
            Point supportEndPoint1 = Canvas_calculations.GetCanvastToCoord(
                new Point(
                m_points[m_points.Count() - 1].ellipse_position.X,
                m_points[m_points.Count() - 1].ellipse_position.Y));
            Point supportCenterPoint1 = (new Point(
                        Math.Abs(supportStartPoint1.X + supportEndPoint1.X) / 2,
                        Math.Abs(supportStartPoint1.Y + supportEndPoint1.Y) / 2));

            Curve_point control_point_1 = new Curve_point(
                Canvas_calculations.GetCoordToCanvast(new Point(
                    Math.Abs(supportStartPoint1.X + supportCenterPoint1.X) / 2,
                    Math.Abs(supportStartPoint1.Y + supportCenterPoint1.Y) / 2)),
                new Ellipse());

            Curve_point control_point_2 = new Curve_point(
                Canvas_calculations.GetCoordToCanvast(new Point(
                    Math.Abs(supportCenterPoint1.X + supportEndPoint1.X) / 2,
                    Math.Abs(supportCenterPoint1.Y + supportEndPoint1.Y) / 2)),
                new Ellipse());

            newPoints2.Add(control_point_1);
            newPoints2.Add(control_point_2);

            newPoints2.Add(m_points.Last());

            m_points = newPoints2;

            m_lines.Add(CreateLine(
                m_points[0].ellipse_position,
                control_point_1.ellipse_position));
            m_lines.Add(CreateLine(
                m_points.Last().ellipse_position,
                control_point_2.ellipse_position));
        }

        private void CreateQuadraticTypePoints()
        {
            List<Curve_point> newPoints1 = new List<Curve_point>();

            newPoints1.Add(m_points[0]);

            Point controlStartPoint1 = Canvas_calculations.GetCanvastToCoord(
                new Point(
                    m_points[0].ellipse_position.X,
                    m_points[0].ellipse_position.Y));
            Point controlEndPoint1 = Canvas_calculations.GetCanvastToCoord(
                new Point(
                    m_points.Last().ellipse_position.X,
                    m_points.Last().ellipse_position.Y));
            Curve_point control_point1 = new Curve_point(Canvas_calculations.GetCoordToCanvast(
                new Point(
                        Math.Abs(controlStartPoint1.X + controlEndPoint1.X) / 2,
                        Math.Abs(controlStartPoint1.Y + controlEndPoint1.Y) / 2)
                ), new Ellipse());
            newPoints1.Add(control_point1);

            newPoints1.Add(m_points.Last());

            m_points = newPoints1;

            m_lines.Add(CreateLine(
                m_points[0].ellipse_position,
                control_point1.ellipse_position));
            m_lines.Add(CreateLine(
                control_point1.ellipse_position,
                m_points.Last().ellipse_position));
        }
        public void SetBezierType(CurveType bezierType = CurveType.Quadratic)
        {
            m_curveType = bezierType;

            //Setting up value of m_lines and control m_points for Bezier curve
            m_lines.Clear();

            switch (m_curveType)
            {
                case CurveType.Line:
                    CreateLineTypePoints();                    
                    break;

                case CurveType.Cubic:
                    CreateCubicTypePoints();
                    break;

                case CurveType.Quadratic:
                    CreateQuadraticTypePoints();
                    break;
            }
        }

        public Curve_point UpdateControlPointPosition()
        {
            for (int i = 1; i < m_points.Count() - 1; i++)
            {
                if (m_points[i].ellipse_position.X - m_controlP_margin < m_points[0].ellipse_position.X)
                {
                    m_points[i].ellipse_position = new Point(
                        m_points[0].ellipse_position.X + m_controlP_margin,
                        m_points[i].ellipse_position.Y);

                    return m_points[i];

                }
                else if (m_points[i].ellipse_position.X + m_controlP_margin > m_points.Last().ellipse_position.X)
                {
                    m_points[i].ellipse_position = new Point(
                        m_points.Last().ellipse_position.X - m_controlP_margin,
                        m_points[i].ellipse_position.Y);
                    return m_points[i];
                }
            }
            return null;
        }

        private  void  RecalculationLineType()
        {
            PointCollection curvePoints = new PointCollection();
            curvePoints.Add(m_points[0].ellipse_position);
            curvePoints.Add(m_points.Last().ellipse_position);

            m_curveGeometry.Points = curvePoints;
            interpolator_points = curvePoints;
        }
        private void RecalculationCubicType()
        {
            PointCollection curvePoints = new PointCollection();

            int samplesCount = (int)((m_points[3].ellipse_position.X - m_points[0].ellipse_position.X) / c_sampleSpan);
            double localDt = 1.0 / samplesCount, t = 0.0;

            while (true)
            {
                curvePoints.Add(new Point(
                    Interpolator.CubicBezier(m_points[0].ellipse_position.X, m_points[1].ellipse_position.X,
                            m_points[2].ellipse_position.X, m_points[3].ellipse_position.X, t),
                    Interpolator.CubicBezier(m_points[0].ellipse_position.Y, m_points[1].ellipse_position.Y,
                            m_points[2].ellipse_position.Y, m_points[3].ellipse_position.Y, t)));

                if (t >= 1.0)
                {
                    break;
                }

                t += localDt;
                t = Math.Min(t, 1.0);
            }

            m_curveGeometry.Points = curvePoints;
            interpolator_points = curvePoints;
        }
        private void RecalculationQuadraticType()
        {
            PointCollection curvePoints = new PointCollection();

            int samplesCount = (int)((m_points[2].ellipse_position.X - m_points[0].ellipse_position.X) / c_sampleSpan);
            double localDt = 1.0 / samplesCount, t = 0.0;

            while (true)
            {
                curvePoints.Add(new Point(
                    Interpolator.QuadraticBezier(m_points[0].ellipse_position.X,
                            m_points[1].ellipse_position.X, m_points[2].ellipse_position.X, t),
                    Interpolator.QuadraticBezier(m_points[0].ellipse_position.Y,
                            m_points[1].ellipse_position.Y, m_points[2].ellipse_position.Y, t)));

                if (t >= 1.0)
                {
                    break;
                }

                t += localDt;
                t = Math.Min(t, 1.0);
            }

            m_curveGeometry.Points = curvePoints;
            interpolator_points = curvePoints;
        }
        public void UpdateGeometry()
        {
            //GetUpdatedLinesArray();
            SetVisibleMode();

            switch (m_curveType)
            {
                case CurveType.Line:
                    RecalculationLineType();
                    break;

                case CurveType.Cubic:
                    {
                        RecalculationCubicType();
                    }
                    break;

                case CurveType.Quadratic:
                    {
                        RecalculationQuadraticType();
                    }
                    break;
            }
            
        }
        public List<Curve_point> GetControlPointArray()
        {
            List<Curve_point> controlPointArray = new List<Curve_point>();

            if (m_curveType == CurveType.Line)
            {
                return controlPointArray;
            }

            for (int i = 0; i < m_points.Count() - 2; i++)
            {
                controlPointArray.Add(m_points[i + 1]);
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

        void SetLineColor()
        {
            for (int i = 0; i < m_lines.Count(); i++)
            {
                if (m_visible_mode)
                {
                    m_lines[i].Opacity = 0.80;
                    m_lines[i].StrokeThickness = 3;
                    m_lines[i].Stroke = Brushes.Gray;
                }
                else
                {
                    m_lines[i].Opacity = 0;
                }
            }
        }

        public List<Line> GetUpdatedLinesArray()
        {
            switch (m_curveType)
            {
                case CurveType.Line:
                    break;
                case CurveType.Cubic:
                    m_lines[0].X1 = m_points[0].ellipse_position.X;
                    m_lines[0].Y1 = m_points[0].ellipse_position.Y;
                    m_lines[0].X2 = m_points[1].ellipse_position.X;
                    m_lines[0].Y2 = m_points[1].ellipse_position.Y;
                                                
                    m_lines[1].X1 = m_points[2].ellipse_position.X;
                    m_lines[1].Y1 = m_points[2].ellipse_position.Y;
                    m_lines[1].X2 = m_points[3].ellipse_position.X;
                    m_lines[1].Y2 = m_points[3].ellipse_position.Y;
                    break;                      
                case CurveType.Quadratic:       
                    m_lines[0].X1 = m_points[0].ellipse_position.X;
                    m_lines[0].Y1 = m_points[0].ellipse_position.Y;
                    m_lines[0].X2 = m_points[1].ellipse_position.X;
                    m_lines[0].Y2 = m_points[1].ellipse_position.Y;
                                                
                    m_lines[1].X1 = m_points[1].ellipse_position.X;
                    m_lines[1].Y1 = m_points[1].ellipse_position.Y;
                    m_lines[1].X2 = m_points[2].ellipse_position.X;
                    m_lines[1].Y2 = m_points[2].ellipse_position.Y;
                    break;
            }
            return m_lines;
        }

        public void SetControlPMargin(int value = 0)
        {
            m_controlP_margin = value;
        }

        private void SetVisibleMode()
        {
            SetLineColor();
            m_curveGeometry.StrokeThickness = 6;
            if (m_visible_mode)
            {
                //change curve colour depennding on their type
                switch (m_curveColor)
                {
                    case ColorType.Alpha:
                        m_curveGeometry.Stroke = new SolidColorBrush(Color.FromArgb(255, 231, 240, 240));
                        break;
                    case ColorType.Red:
                        m_curveGeometry.Stroke = new SolidColorBrush(Color.FromArgb(255, 200, 88, 68));
                        break;
                    case ColorType.Green:
                        m_curveGeometry.Stroke = Brushes.Green;
                        break;
                    case ColorType.Blue:
                        m_curveGeometry.Stroke = new SolidColorBrush(Color.FromArgb(255, 56, 155, 208));
                        break;
                }
                m_curveGeometry.Opacity = 1;

                foreach (Curve_point control_point in GetControlPointArray())
                {
                    control_point.ellipse.Opacity = 0.75;
                }
            }
            else
            {
                m_curveGeometry.Opacity = 0.75;
                foreach (Curve_point control_point in GetControlPointArray())
                {
                    control_point.ellipse.Opacity = 0;
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
            curvePoint = new Curve_point(curvePoint_.ellipse_position, curvePoint_.ellipse);
        }
    }
    public class Curve
    {
        CurveType globalCuveType = CurveType.Quadratic;
        public ColorType globalCurveColor = ColorType.Alpha;

        //----------------------------------------Arrays keeping m_points
        List<Curve_point> m_pointArray = new List<Curve_point>();
        List<Curve_point> m_base_pointArray = new List<Curve_point>();
        List<Curve_point> m_control_pointArray = new List<Curve_point>();

        //----------------------------------------Arrays keeping segments
        List<Segment_curve> m_segmentsArray = new List<Segment_curve>();

        int m_point_margin = 40; //distance of minimum gap between m_points

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

        void UpdateSegment(Segment_curve segment)
        {
            Curve_point point_to_update = segment.UpdateControlPointPosition();
            if (point_to_update != null)
            {
                CurvePointEventArgs eventArgs = new CurvePointEventArgs(point_to_update);
                CurvePointMove(this, eventArgs);
            }
            segment.UpdateGeometry();
            segment.GetUpdatedLinesArray();
        }

        public void ChangeVisibleCurve(bool visible_option)
        {
            foreach (Segment_curve segment in m_segmentsArray)
            {
                segment.m_visible_mode = visible_option;
                AddSegment_to_viewport(segment);
            }
        }

        void ChangePointPosition(Point point, Curve_point curve_point)
        {
            curve_point.ellipse_position = point;
            CurvePointEventArgs eventArgs = new CurvePointEventArgs(curve_point);
            CurvePointMove(this, eventArgs);
        }
        void AddSegment_to_viewport(Segment_curve segment)
        {
            //updating segments 
            segment.UpdateGeometry();
            PathEventArgs eventArgs = new PathEventArgs(segment.m_curveGeometry);
            PathGeomertyAddToViewport(this, eventArgs);
        }
        public void ChangeSegmentBezierType(CurveType bezierType)
        {
            globalCuveType = bezierType;

            foreach (Segment_curve segment in m_segmentsArray)
            {   //destroy old m_points, m_lines from editor
                LineEventArgs eventArgs = new LineEventArgs(segment.m_lines);
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
                if (sender == curve_Point.ellipse)
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
                if (m_base_pointArray.Last().ellipse_position.X < point.X)
                {
                    m_pointArray.Add(new_curve_point);
                    Segment_curve segment_Curve = new Segment_curve(
                    m_base_pointArray.Last(),
                    new_curve_point, globalCuveType);
                    m_segmentsArray.Add(segment_Curve);
                    segment_Curve.m_curveColor = globalCurveColor;
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
                    //sort the m_points with respect to the new one added in the middle
                    while (i < m_base_pointArray.Count())
                    {
                        if (!curve_points_swap && m_base_pointArray[i].ellipse_position.X > new_curve_point.ellipse_position.X)
                        {
                            curve_points_swap = true;

                            next_temp = new Curve_point(
                                m_base_pointArray[i].ellipse_position,
                                m_base_pointArray[i].ellipse
                                );

                            m_base_pointArray[i].ellipse_position = new_curve_point.ellipse_position;
                            m_base_pointArray[i].ellipse = new_curve_point.ellipse;

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
                                m_base_pointArray[i].ellipse_position,
                                m_base_pointArray[i].ellipse
                                );

                                m_base_pointArray[i].ellipse_position = next_temp.ellipse_position;
                                m_base_pointArray[i].ellipse = next_temp.ellipse;
                            }
                            else
                            {
                                next_temp = new Curve_point(
                                m_base_pointArray[i].ellipse_position,
                                m_base_pointArray[i].ellipse
                                );

                                m_base_pointArray[i].ellipse_position = temp.ellipse_position;
                                m_base_pointArray[i].ellipse = temp.ellipse;
                            }
                            temp_mode = !temp_mode;
                        }
                        i++;
                    }
                    if (!temp_mode)
                    {
                        new_curve_point.ellipse_position = temp.ellipse_position;
                        new_curve_point.ellipse = temp.ellipse;
                    }
                    else
                    {
                        new_curve_point.ellipse_position = next_temp.ellipse_position;
                        new_curve_point.ellipse = next_temp.ellipse;
                    }

                    UpdateSegment(m_segmentsArray.Last());

                    //create a new segment : new added point -> next point after this new one

                    new_curve_point.default_segment_index = m_segmentsArray.Count();

                    Segment_curve segment_Curve = new Segment_curve(
                    m_base_pointArray.Last(),
                    new_curve_point, globalCuveType);
                    m_segmentsArray.Add(segment_Curve);

                    segment_Curve.m_curveColor = globalCurveColor;
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
            if ((Canvas_calculations.GetCoordToCanvast(e).X > 0 && Canvas_calculations.GetCoordToCanvast(e).X < 1240) && (Canvas_calculations.GetCoordToCanvast(e).Y > 0 && Canvas_calculations.GetCoordToCanvast(e).Y < 800))
            {
                bool executed = false;
                int activeSegmentIndex = 0;
                int index_pointArray = 0;
                //change the position of moved point (if moved base P)
                foreach (Curve_point curve_Point in m_base_pointArray)
                {
                    //Find the displaced point
                    if (sender == curve_Point.ellipse)
                    {
                        activeSegmentIndex = curve_Point.default_segment_index;
                        //first point - move
                        if (index_pointArray == 0)
                        {
                            if (m_pointArray.Count >= 2)
                            {
                                if (Canvas_calculations.GetCoordToCanvast(e).X + m_point_margin <=
                                    m_base_pointArray[index_pointArray + 1].ellipse_position.X)
                                {

                                    ChangePointPosition(
                                        new Point(0, Canvas_calculations.GetCoordToCanvast(e).Y), curve_Point);
                                }
                                else
                                {
                                    ChangePointPosition(
                                        new Point(0, Canvas_calculations.GetCoordToCanvast(e).Y), curve_Point);
                                }
                            }
                            else
                            //No m_points more, move freely
                            {
                                ChangePointPosition(
                                        new Point(0, Canvas_calculations.GetCoordToCanvast(e).Y), curve_Point);
                            }
                        }
                        //last point
                        else if (index_pointArray == (m_base_pointArray.Count() - 1))
                        {
                            if (Canvas_calculations.GetCoordToCanvast(e).X - m_point_margin >= m_base_pointArray[index_pointArray - 1].ellipse_position.X)
                            {
                                ChangePointPosition(Canvas_calculations.GetCoordToCanvast(e), curve_Point);
                            }
                            else
                            {
                                ChangePointPosition(new Point(
                                        (curve_Point.ellipse_position.X),
                                        Canvas_calculations.GetCoordToCanvast(e).Y),
                                        curve_Point);
                            }
                        }
                        //point in the middle
                        else
                        {
                            if (Canvas_calculations.GetCoordToCanvast(e).X - m_point_margin >=
                                m_base_pointArray[index_pointArray - 1].ellipse_position.X
                                &&
                                Canvas_calculations.GetCoordToCanvast(e).X + m_point_margin <=
                                m_base_pointArray[index_pointArray + 1].ellipse_position.X)
                            {
                                //between m_points
                                ChangePointPosition(Canvas_calculations.GetCoordToCanvast(e), curve_Point);
                            }
                            else
                            {//outside the possible area
                                ChangePointPosition(new Point(
                                (curve_Point.ellipse_position.X),
                                Canvas_calculations.GetCoordToCanvast(e).Y),
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
                        if (m_pointArray[i].ellipse == sender)
                        {
                            ChangePointPosition(Canvas_calculations.GetCoordToCanvast(e), m_pointArray[i]);
                            activeSegmentIndex = m_pointArray[i].default_segment_index;
                            break;
                        }
                    }
                }

                //Update closest segments (m_points and curves - lines)
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
                if (time >= m_segmentsArray.First().m_points.First().ellipse_position.X &&
                    time <= m_segmentsArray.Last().m_points.Last().ellipse_position.X)
                {
                    foreach (Segment_curve segment_ in m_segmentsArray)
                    {
                        if (time <= segment_.m_points.Last().ellipse_position.X)
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
