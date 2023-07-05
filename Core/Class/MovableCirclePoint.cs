using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Curves_editor.Core.Class
{
    internal class MovableCirclePoint
    {
        float angle = 0.0f;
        float rad = 60f;
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

        void createTimer()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.01);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            Point transformed_point = GetCirclePoint(rad, angle, CirclePoint);
            angle += 5;
            mainWindow_m.SetPointPosition(circle_point, transformed_point);
        }

        public Point GetCirclePoint(float radius, float angleInDegrees, Point origin)
        {
            float x = (float)(radius * Math.Cos(angleInDegrees * Math.PI / 180F) + origin.X);
            float y = (float)(radius * Math.Sin(angleInDegrees * Math.PI / 180F) + origin.Y);

            return new Point(x, y);
        }
    }
    
}
