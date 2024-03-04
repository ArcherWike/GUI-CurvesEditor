using CanvasWindow;
using Curves_editor.Core.Class;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UiDesign
{
    public partial class MainWindow : Window
    {
        Ellipse activepoint = null;
        Ellipse hoverpoint = null;

        double window_height = 0;
        double window_width = 0;

        public Curve active_curve = null;
        public List<Curve> curves = new List<Curve>();
        ColorType active_color = ColorType.Alpha;

        RectangleRGB rectangleRGB;
        Rectangle rectangle_rgb = new Rectangle();

        //chart marker
        Ellipse chart_marker = new Ellipse();
        Point ChartMarkerPoint = new Point(190, 120);

        //button start-pause time
        private StackPanel stackPnl = new StackPanel();
        Image img = new Image();

        public MainWindow()
        {
            InitializeComponent();

            CreateRectangleRGB();
            Create_chart_marker_point();
            rectangleRGB = new RectangleRGB(rectangle_rgb, chart_marker, this);

            //set up button - play (start time)
            img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Curves_editor;component//Icon/Play.png"));
            stackPnl.Orientation = Orientation.Horizontal;
            stackPnl.Margin = new Thickness(10);
            StartEvent_Button.Content = stackPnl;
            stackPnl.Children.Add(img);
        }
        private void CreateRectangleRGB()
        {
            rectangle_rgb.Stroke = Brushes.Black;
            rectangle_rgb.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            rectangle_rgb.StrokeThickness = 2;
            rectangle_rgb.Width = 300;
            rectangle_rgb.Height = 300;
            Colour_square.Children.Add(rectangle_rgb);
        }

        private void Create_chart_marker_point()
        {
            chart_marker.Stroke = Brushes.Red;
            chart_marker.Fill = System.Windows.Media.Brushes.Red;
            chart_marker.StrokeThickness = 10;
            chart_marker.Width = 3;
            chart_marker.Height = 800;
            CordSys.Children.Add(chart_marker);
            Canvas.SetZIndex(chart_marker, -1);
        }

        public void SetPointPosition(Ellipse myEllipse, Point mousePoint)
        {
            if (active_curve != null)
            {
                Point delta = new Point((mousePoint.X - 15 - Canvas.GetLeft(myEllipse)) / 200,
                     (Canvas.GetTop(myEllipse) - mousePoint.Y + 15) / 200);

                Point new_pos = Canvas_calculations.GetCanvastToCoord(mousePoint);
                active_curve.UpdatePointPosition(myEllipse, new_pos);
            }
        }

        //###################### Point function ###########################
        private Ellipse CreatePoint(Point mousePosition)
        {

            Ellipse myEllipse = new Ellipse();

            myEllipse.Width = 30;
            myEllipse.Height = 30;
            myEllipse.Fill = new SolidColorBrush(Color.FromArgb(148, 98, 98, 0));
            Canvas.SetZIndex(myEllipse, 2);
            CordSys.Children.Add(myEllipse);
            SetPointPosition(myEllipse, mousePosition);

            Canvas.SetLeft(myEllipse, mousePosition.X - 15);
            Canvas.SetTop(myEllipse, mousePosition.Y - 15);

            myEllipse.MouseEnter += Ellipse_mouseEnter;
            myEllipse.MouseLeave += Ellipse_mouseLeave;
            myEllipse.MouseLeftButtonDown += MyEllipse_MouseLeftButtonDown;
            myEllipse.MouseLeftButtonUp += MyEllipse_MouseLeftButtonUp;

            return myEllipse;
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
            rectangleRGB.SetPauseMode(true);
            hoverpoint = (sender as Ellipse);

            if (active_curve.isPoint(sender as Ellipse))
            {
                hoverpoint.Opacity = 1;
                hoverpoint.Fill = System.Windows.Media.Brushes.Red;
            }
        }
        private void Ellipse_mouseLeave(object sender, MouseEventArgs e)
        {
            if (active_curve.isPoint(sender as Ellipse))
            {
                if ((sender as Ellipse).Width == 20)
                {
                    (sender as Ellipse).Fill = System.Windows.Media.Brushes.SlateGray;
                    (sender as Ellipse).Opacity = 0.65;
                }
                else
                {
                    (sender as Ellipse).Fill = System.Windows.Media.Brushes.White;
                    (sender as Ellipse).Opacity = 0.75;
                }
            }
            hoverpoint = null;
            rectangleRGB.SetPauseMode(false);
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

        private void CreateNewCurve()
        {
            active_curve = new Curve();
            active_curve.globalCurveColor = active_color;
            curves.Add(active_curve);

            active_curve.PathGeomertyAddToViewport += new Curve.PathHandler(UpdateSegmentViewport);

            active_curve.OnLineAdded += new Curve.LineHandler(CreateLine);
            active_curve.DestroyLine += new Curve.LineHandler(DestroyLine);

            active_curve.OnCurvePointAdded += new Curve.CurvePointsListHandler(CreateCotrolPoints);
            active_curve.DestroyCurvePoint += new Curve.CurvePointsListHandler(DestroyControlPoint);

            active_curve.CurvePointMove += new Curve.CurvePointHandler(UpdatePointPosition);
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (activepoint != null || hoverpoint != null)
            {
                return;
            }
            if (active_curve == null)
            {
                CreateNewCurve();
            }
            active_curve.AddPoint((
                    e.GetPosition(this.CordSys)),
                    CreatePoint(e.GetPosition(this.CordSys)));
        }

        private void UpdatePointPosition(object sender, CurvePointEventArgs e)
        {
            Canvas.SetLeft(e.curvePoint.ellipse, e.curvePoint.ellipse_position.X - 15);
            Canvas.SetTop(e.curvePoint.ellipse, e.curvePoint.ellipse_position.Y - 15);

        }

        ///############################## Line - Control Point Function ###############################
        private void CreateLine(object sender, LineEventArgs e)
        {
            foreach (Line line in e.linesArray)
            {
                CordSys.Children.Add(line);
                Canvas.SetZIndex(line, 2);
            }
        }

        private void DestroyLine(object sender, LineEventArgs e)
        {
            foreach (Line line in e.linesArray)
            {
                CordSys.Children.Remove(line);
            }
        }

        ///############################## Control Point Function ###############################
        private void CreateCotrolPoints(object sender, CurvePointsListEventArgs e)
        {
            foreach (Curve_point point in e.curvePointArray)
            {
                point.ellipse.Width = 20;
                point.ellipse.Height = 20;
                point.ellipse.Fill = System.Windows.Media.Brushes.SlateGray;
                point.ellipse.Opacity = 0.65;
                Canvas.SetZIndex(point.ellipse, 3);
                CordSys.Children.Add(point.ellipse);
                SetControlPointPosition(point.ellipse, (point.ellipse_position));
                point.ellipse.MouseEnter += Ellipse_mouseEnter;
                point.ellipse.MouseLeave += Ellipse_mouseLeave;
                point.ellipse.MouseLeftButtonDown += MyEllipse_MouseLeftButtonDown;
                point.ellipse.MouseLeftButtonUp += MyEllipse_MouseLeftButtonUp;
            }
        }
        public void SetControlPointPosition(Ellipse myEllipse, Point mousePoint)
        {
            //rectangleRGB.DisconectedTimer();
            if (active_curve != null)
            {
                Point delta = new Point((mousePoint.X - Canvas.GetLeft(myEllipse)) / 200,
                     (Canvas.GetTop(myEllipse) - mousePoint.Y) / 200);

                Point new_pos = Canvas_calculations.GetCanvastToCoord(mousePoint);

                active_curve.UpdatePointPosition(myEllipse, new_pos);
            }

            Canvas.SetLeft(myEllipse, mousePoint.X - 15);
            Canvas.SetTop(myEllipse, mousePoint.Y - 15);

        }
        private void DestroyControlPoint(object sender, CurvePointsListEventArgs e)
        {
            foreach (Curve_point point in e.curvePointArray)
            {
                CordSys.Children.Remove(point.ellipse);
            }
        }

        ///############################## Segment- Curve Function ###############################
        private void UpdateSegmentViewport(object sender, PathEventArgs e)
        {
            if (!CordSys.Children.Contains(e.pathGeometry))
            {
                CordSys.Children.Add(e.pathGeometry);
                Canvas.SetZIndex(e.pathGeometry, 2);
            }
        }

        private void PickByColor(ColorType new_color)
        {
            if (active_curve != null)
            {
                if (active_curve.globalCurveColor == new_color)
                {
                    return;
                }
                active_curve.ChangeVisibleCurve(false);
                active_curve = null;
            }
            active_color = new_color;

            foreach (Curve curve in curves)
            {
                if (curve.globalCurveColor == active_color)
                {
                    curve.ChangeVisibleCurve(true);
                    active_curve = curve;
                    return;
                }
            }
            CreateNewCurve();
        }

        //############################# Window buttons ##############################
        private void LineEventButton(object sender, RoutedEventArgs e)
        {
            if (active_curve != null)
            {
                active_curve.ChangeSegmentBezierType(CurveType.Line);
            }
        }

        private void CubicEventButton(object sender, RoutedEventArgs e)
        {
            if (active_curve != null)
            {
                active_curve.ChangeSegmentBezierType(CurveType.Cubic);
            }
        }

        private void QuadraticEventButton(object sender, RoutedEventArgs e)
        {
            if (active_curve != null)
            {
                active_curve.ChangeSegmentBezierType(CurveType.Quadratic);
            }
        }

        private void Clear_Viewport(object sender, RoutedEventArgs e)
        {
            active_curve = null;
            curves.Clear();
            CordSys.Children.Clear();
            Create_chart_marker_point();

        }

        private void btnMinimalize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnMaximalize_Click(object sender, RoutedEventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    this.WindowState = WindowState.Maximized; break;
                case WindowState.Maximized:
                    this.WindowState = WindowState.Normal; break;
                default: break;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AlphaEvent_Button_Click(object sender, RoutedEventArgs e)
        {
            PickByColor(ColorType.Alpha);
        }

        private void RedEvent_Button_Click(object sender, RoutedEventArgs e)
        {
            PickByColor(ColorType.Red);
        }

        private void GreenEvent_Button_Click(object sender, RoutedEventArgs e)
        {
            PickByColor(ColorType.Green);
        }

        private void BlueEvent_Button_Click(object sender, RoutedEventArgs e)
        {
            PickByColor(ColorType.Blue);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            window_height = CordSys.ActualHeight;
            window_width = CordSys.ActualWidth;
        }

        private void StartEvent_Click(object sender, RoutedEventArgs e)
        {
            stackPnl.Children.Clear();

            if (rectangleRGB.SetPause())
            {
                img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Curves_editor;component//Icon/pause.png"));
                stackPnl.Children.Add(img);
            }
            else
            {
                img.Source = new BitmapImage(new Uri(@"pack://application:,,,/Curves_editor;component//Icon/Play.png"));
                stackPnl.Children.Add(img);
            }
        }

        private void bar_settings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}