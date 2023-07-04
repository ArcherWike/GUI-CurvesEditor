using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UiDesign
{
    public partial class MainWindow : Window
    {
        List<Point> pointArray = new List<Point>();

        Ellipse activepoint = null;

        Ellipse circle_point = new Ellipse();
        int rotator_deg = 90;
        float angle = 0.0f;
        Point CirclePoint = new Point(550, 400);
        float rad = 150f;




        public MainWindow()
        {
            InitializeComponent();


            circle_point.Stroke = Brushes.Red;
            circle_point.Fill = System.Windows.Media.Brushes.Red;
            circle_point.StrokeThickness = 10;
            Canvas.SetLeft(circle_point, 200);
            Canvas.SetTop(circle_point, 100);
            circle_point.Width = 30;
            circle_point.Height = 30;
            SetPointPosition(circle_point, CirclePoint);
            CordSys.Children.Add(circle_point);

            //timer
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void AddPoint(Point mousePoint)
        {
            Ellipse myEllipse = new Ellipse();

            myEllipse.Width = 30;
            myEllipse.Height = 30;
            myEllipse.Fill = System.Windows.Media.Brushes.White;
            CordSys.Children.Add(myEllipse);
            SetPointPosition(myEllipse, mousePoint);
            myEllipse.MouseEnter += Ellipse_mouseEnter;
            myEllipse.MouseLeave += Ellipse_mouseLeave;
            myEllipse.MouseLeftButtonDown += MyEllipse_MouseLeftButtonDown;
            myEllipse.MouseLeftButtonUp += MyEllipse_MouseLeftButtonUp;

        }

        private void SetPointPosition(Ellipse myEllipse, Point mousePoint) 
        {

            Canvas.SetLeft(myEllipse, mousePoint.X - 15);
            Canvas.SetTop(myEllipse, mousePoint.Y - 15);
        }
        private void MyEllipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            activepoint = null;
            e.Handled = true;
        }

        private void MyEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            activepoint = (sender as Ellipse);
        }

        private Point GetCanvastToCoord(Point mousePosition)
        {
            Point result = new Point((mousePosition.X - 40)/200, 4 - (mousePosition.Y/200));

            return result;
        }

        private void Ellipse_mouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Ellipse).Fill = System.Windows.Media.Brushes.Red;

        }
        private void Ellipse_mouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Ellipse).Fill = System.Windows.Media.Brushes.White;

        }


        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
            pointArray.Add(GetCanvastToCoord(e.GetPosition(this.CordSys)));
            AddPoint(e.GetPosition(this.CordSys));

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void CordSys_MouseMove(object sender, MouseEventArgs e)
        {
            if (activepoint != null)
            {
                SetPointPosition(activepoint, e.GetPosition(this.CordSys));

            }
        }

        //circle point
        void timer_Tick(object sender, EventArgs e)
        {
            Point transformed_point = GetCirclePoint(rad, angle, CirclePoint);
            angle += 5;
            SetPointPosition(circle_point, transformed_point);
        }

        public Point GetCirclePoint(float radius, float angleInDegrees, Point origin)
        {
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F) + origin.X);
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F) + origin.Y);

            return new Point(x, y);
        }
    }
}
