using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Curves_editor.Core.Class
{
    internal class MovableCirclePoint
    {
        double angle = 0.0f;
        //float rad = 180f;
        float time = 0.0f;
        byte alpha = 0;

        UiDesign.MainWindow mainWindow_m;

        private Ellipse m_circle_point = null;
        private Ellipse m_chart_marker = null;

        private Point CirclePoint;

        public MovableCirclePoint(Point circlePoint, Ellipse circle_point_shape, Ellipse chart_marker_shape, UiDesign.MainWindow mainWindow)
        {
            mainWindow_m = mainWindow;
            CirclePoint = circlePoint;
            m_circle_point = circle_point_shape;
            m_chart_marker = chart_marker_shape;

            createTimer();          
        }
        enum ColorType
        {
            Alpha,
            Red,
            Green,
            Blue,
        }
        private Point GetCoordToCanvastMovablePoint(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 100) + 40, 200 - pointPosition.Y * 100);

            return result;
        }


        void createTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.01);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
           

            float velocity_alpha = 255.0f;
            float velocity_red = 0.0f;
            float velocity_green = 0.0f;
            float velocity_blue = 0.0f;

            float multiply = 0.313225490196078f;

            if (mainWindow_m.active_curve != null)
            {
                time += 1;
                if (time > 600.0f)
                {
                    time = 0.0f;
                }
                Point temp_point = new Point(time, 0);
                Point time_in_canvas_cord = temp_point;//mainWindow_m.GetCoordToCanvast(temp_point);
                Point time_in_cord = mainWindow_m.GetCanvastToCoord(temp_point);
                Canvas.SetLeft(m_chart_marker, time_in_canvas_cord.X);

                foreach (Curve active_curve in mainWindow_m.curves)
                {
                    switch (active_curve.globalCurveColor)
                    {
                        case Class.ColorType.Alpha:
                            velocity_alpha -= active_curve.GetValueAt((float)(time_in_canvas_cord.X));
                            break;
                        case Class.ColorType.Red:
                            velocity_red = (active_curve.GetValueAt((float)(time_in_canvas_cord.X))*multiply);
                            break;
                        case Class.ColorType.Green:
                            velocity_green = active_curve.GetValueAt((float)(time_in_canvas_cord.X)) * multiply;
                            break;
                        case Class.ColorType.Blue:
                            velocity_blue = active_curve.GetValueAt((float)(time_in_canvas_cord.X)) * multiply;
                            break;
                    }

                    Console.WriteLine(velocity_red + " " + velocity_green + " " + velocity_blue + " time: " + time_in_canvas_cord.X);
                }
                //Canvas.SetTop(m_chart_marker, mainWindow_m.curve.GetValueAt((float)(time_in_canvas_cord.X)) - 15);
            }
            

            //
            /*float multiply = 0.0f;

            velocity_alpha += multiply;
            velocity_red += multiply;
            velocity_green += multiply;
            velocity_blue += multiply;*/

            //angle = velocity_alpha* ;
            //Console.WriteLine(angle+" "+alpha);
            //alpha = (byte)angle;
            //CirclePoint = GetCirclePoint(rad, angle, new Point(400, 300));
            m_circle_point.Fill = new SolidColorBrush(Color.FromArgb(
                (byte)velocity_alpha, 
                (byte)velocity_red, 
                (byte)velocity_green, 
                (byte)velocity_blue));
            //Canvas.SetTop(m_circle_point, CirclePoint.Y - 30);
            //Canvas.SetLeft(m_circle_point, CirclePoint.X);
        }

        public Point GetCirclePoint(float radius, float angleInDegrees, Point origin)
        {
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F) + origin.X);
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F) + origin.Y);

            return new Point(x, y);
        }
    }  
}
