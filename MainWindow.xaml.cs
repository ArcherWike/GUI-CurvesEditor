using Curves_editor.Core.Class;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace UiDesign
{
    public partial class MainWindow : Window
    {
        Ellipse activepoint = null;
        Ellipse hoverpoint = null;
        Curve curve = null;

        //movable point
        Ellipse circle_point = new Ellipse();
        Point CirclePoint = new Point(190, 120);
        MovableCirclePoint movablePoint;

        public MainWindow()
        {
            InitializeComponent();

            Create_circle_point();
            movablePoint = new MovableCirclePoint(CirclePoint, circle_point, this);
            
        }

        private void Create_circle_point()
        {
            circle_point.Stroke = Brushes.Red;
            circle_point.Fill = System.Windows.Media.Brushes.Red;
            circle_point.StrokeThickness = 10;
            circle_point.Width = 30;
            circle_point.Height = 30;
            SettingsWindow.Children.Add(circle_point);
        }

        public void SetPointPosition(Ellipse myEllipse, Point mousePoint)
        {
            if (curve != null)
            {
                Point delta = new Point((mousePoint.X - 15 - Canvas.GetLeft(myEllipse)) / 200,
                     (Canvas.GetTop(myEllipse) - mousePoint.Y + 15) / 200);

                Point new_pos = GetCanvastToCoord(mousePoint);
                curve.UpdatePointPosition(myEllipse, new_pos);
            }

            Canvas.SetLeft(myEllipse, mousePoint.X - 15);
            Canvas.SetTop(myEllipse, mousePoint.Y - 15);
        }

        //###################### Point function ###########################
        private Ellipse CreatePoint(Point mousePosition)
        {
            Ellipse myEllipse = new Ellipse();

            myEllipse.Width = 30;
            myEllipse.Height = 30;
            myEllipse.Fill = System.Windows.Media.Brushes.White;
            //myEllipse.
            Canvas.SetZIndex(myEllipse,7);
            CordSys.Children.Add(myEllipse);
            SetPointPosition(myEllipse, mousePosition);
            myEllipse.MouseEnter += Ellipse_mouseEnter;
            myEllipse.MouseLeave += Ellipse_mouseLeave;
            myEllipse.MouseLeftButtonDown += MyEllipse_MouseLeftButtonDown;
            myEllipse.MouseLeftButtonUp += MyEllipse_MouseLeftButtonUp;

            return myEllipse;
        }
        private void MypathGeometryLeftButtonUp(object sender, MouseButtonEventArgs e)/////////////////
        {
            (sender as Path).Stroke = Brushes.Black;
        }
        private void MypathGeometry_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as Path).Stroke = Brushes.Yellow;
        }  
        private void CordSys_MouseMove(object sender, MouseEventArgs e)
        {
            if (activepoint != null)
            {
                SetPointPosition(activepoint, e.GetPosition(this.CordSys));
            }
        }
        private void Ellipse_mouseEnter(object sender, MouseEventArgs e)
        {
            hoverpoint = (sender as Ellipse);
            hoverpoint.Fill = System.Windows.Media.Brushes.Red;
        }
        private void Ellipse_mouseLeave(object sender, MouseEventArgs e)
        {
            if ((sender as Ellipse).Width == 20)
            {
                (sender as Ellipse).Fill = System.Windows.Media.Brushes.Blue;
            }
            else
            {
                (sender as Ellipse).Fill = System.Windows.Media.Brushes.White;
            }
            hoverpoint = null;
        }
        private void MyEllipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (activepoint != null)
            {
                activepoint.ReleaseMouseCapture();
                activepoint = null;
                e.Handled = true;
            }
        }
        private void MyEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            activepoint = (sender as Ellipse);
            activepoint.CaptureMouse();
            e.Handled = true;
        }
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (activepoint != null || hoverpoint != null)
            {
                return;
            }
            if (curve == null)
            {
                curve = new Curve();

                curve.PathGeomertyAddToViewport += new Curve.PathHandler(UpdateSegmentViewport);

                curve.OnLineAdded += new Curve.LineHandler(CreateLine);
                curve.UpdateLine += new Curve.LineHandler(UpdateLine);
                curve.DestroyLine += new Curve.LineHandler(DestroyLine);

                curve.OnCurvePointAdded += new Curve.CurvePointHandler(CreateCotrolPoints);
                curve.DestroyCurvePoint += new Curve.CurvePointHandler(DestroyControlPoint);
            }
            curve.AddPoint((
                    e.GetPosition(this.CordSys)),
                    CreatePoint(e.GetPosition(this.CordSys)));
        }

        //############### General static function ##################
        public static Point GetCanvastToCoord(Point mousePosition)
        {
            Point result = new Point((mousePosition.X - 40) / 200, 4 - (mousePosition.Y / 200));

            return result;
        }
        public static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200) + 40, 800 - pointPosition.Y * 200);

            return result;
        }

        


        ///############################## Line - Control Point Function ###############################
        private void CreateLine(object sender, LineEventArgs e)
        {
            foreach (Line line in e.linesArray)
            {
                line.Stroke = System.Windows.Media.Brushes.Black;
                line.StrokeThickness = 1;
                CordSys.Children.Add(line);
            }
        }
        private void UpdateLine(object sender, LineEventArgs e)
        {

        }

        private void DestroyLine(object sender, LineEventArgs e)
        {
            foreach (Line line in e.linesArray)
            {
                CordSys.Children.Remove(line);
            }
        }

        ///############################## Control Point Function ###############################
        private void CreateCotrolPoints(object sender, CurvePointEventArgs e)
        {
            foreach (Curve_point point in e.curvePointArray)
            {
                point.ellipseID.Width = 20;
                point.ellipseID.Height = 20;
                point.ellipseID.Fill = System.Windows.Media.Brushes.Blue;
                Canvas.SetZIndex(point.ellipseID, 10);
                CordSys.Children.Add(point.ellipseID);
                SetControlPointPosition(point.ellipseID, (point.ellipse_positionID));
                point.ellipseID.MouseEnter += Ellipse_mouseEnter;
                point.ellipseID.MouseLeave += Ellipse_mouseLeave;
                point.ellipseID.MouseLeftButtonDown += MyEllipse_MouseLeftButtonDown;
                point.ellipseID.MouseLeftButtonUp += MyEllipse_MouseLeftButtonUp;
            }        
        }
        public void SetControlPointPosition(Ellipse myEllipse, Point mousePoint)
        {
            if (curve != null)
            {
                Point delta = new Point((mousePoint.X - 10 - Canvas.GetLeft(myEllipse)) / 200,
                     (Canvas.GetTop(myEllipse) - mousePoint.Y + 10) / 200);

                Point new_pos = GetCanvastToCoord(mousePoint);
                curve.UpdatePointPosition(myEllipse, new_pos);
            }

            Canvas.SetLeft(myEllipse, mousePoint.X - 15);
            Canvas.SetTop(myEllipse, mousePoint.Y - 15);
        }
        private void DestroyControlPoint(object sender, CurvePointEventArgs e)
        {
            foreach (Curve_point point in e.curvePointArray)
            {
                CordSys.Children.Remove(point.ellipseID);
            }
        }


        ///############################## Segment- Curve Function ###############################
        private void UpdateSegmentViewport(object sender, PathEventArgs e)
        {
            CordSys.Children.Remove(e.pathGeometry);
            /*myPath = new Path();
            myPath.Stroke = Brushes.Black;
            myPath.StrokeThickness = 1;
            myPath.Data = e.pathGeometry;*/

           /* e.pathGeometry.MouseLeftButtonDown += MypathGeometry_MouseLeftButtonDown;
            e.pathGeometry.MouseLeftButtonUp += MypathGeometryLeftButtonUp;*/
           //CordSys.Children.
            CordSys.Children.Add(e.pathGeometry);
        }
        private void LineEventButton(object sender, RoutedEventArgs e)
        {
            if (curve != null)
            {
                curve.ChangeSegmentBezierType(BezierType.Line);
            }
        }

        private void CubicEventButton(object sender, RoutedEventArgs e)
        {
            if (curve != null)
            {
                curve.ChangeSegmentBezierType(BezierType.Cubic);
            }
        }

        private void QuadraticEventButton(object sender, RoutedEventArgs e)
        {
            if (curve != null)
            {
                curve.ChangeSegmentBezierType(BezierType.Quadratic);
            }
        }



        private void Clear_Viewport(object sender, RoutedEventArgs e)
        {
            curve = null;
            CordSys.Children.Clear();
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }
    }
}