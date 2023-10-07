using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Curves_editor.Core.Class
{
    internal class MovableCirclePoint
    {
        float angle = 0.0f;
        float rad = 60f;
        float time = 0.0f;

        UiDesign.MainWindow mainWindow_m;

        private Ellipse circle_point = null;

        private Point CirclePoint;

        public MovableCirclePoint(Point circlePoint, Ellipse circle_point_shape, UiDesign.MainWindow mainWindow)
        {
            mainWindow_m = mainWindow;
            CirclePoint = circlePoint;
            circle_point = circle_point_shape;
            createTimer();          
        }
        private Point GetCoordToCanvastMovablePoint(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 100) + 40, 200 - pointPosition.Y * 100);

            return result;
        }


        void createTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.001);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            time += 10;
            if (time > 6000)
            { 
                time = 0; 
            }

            if (mainWindow_m.curve != null)
            {
                Point temp_point = new Point(time/1000, 0);
                Point time_in_canvas_cord = mainWindow_m.GetCoordToCanvast(temp_point);

                Canvas.SetLeft(circle_point, time_in_canvas_cord.X);
                Canvas.SetTop(circle_point, mainWindow_m.curve.GetValueAt((float)(time_in_canvas_cord.X)));
            }
        }

        public Point GetCirclePoint(float radius, float angleInDegrees, Point origin)
        {
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F) + origin.X);
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F) + origin.Y);

            return new Point(x, y);
        }
    }  
}
