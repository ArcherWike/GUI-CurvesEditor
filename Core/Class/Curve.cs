using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

namespace Curves_editor.Core.Class
{
    class Curve_point
    {
        public Point Position;
        public Ellipse Ellipse_m { get; private set; }
        public Curve_point(Point position, Ellipse ellipse)
        {
            Position = position;
            Ellipse_m = ellipse;
        }
    }
    public class AddPointEventArgs : EventArgs
    {
        public Point Position_change { get; private set; }
        public Ellipse Ellipse_m { get; private set; }

        public AddPointEventArgs(Point position, Ellipse ellipse)
        {
            Position_change = position;
            Ellipse_m = ellipse;
        }
    }

    internal class Curve
    {
        List<Curve_point> pointArray = new List<Curve_point>();
        //public delegate void AddPointHandler(object sender, AddPointEventArgs e);
        //public event AddPointHandler OnPointAdded;

        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point curve_Point = new Curve_point(point, ellipse);
            pointArray.Add(curve_Point);
            /*if (OnPointAdded != null)
            {
                AddPointEventArgs eventArgs = new AddPointEventArgs(point);
                OnPointAdded(this, eventArgs);
            }*/
        }

        public void UpdatePointPosition(Ellipse sender, Point e)
        {
            foreach (Curve_point curve_Point in pointArray) 
            { 
                if (sender == curve_Point.Ellipse_m)
                {
                    curve_Point.Position.X += e.X;
                    curve_Point.Position.Y += e.Y;
                    break;
                }
            } 
        }
    }
}
