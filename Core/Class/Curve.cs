using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using UiDesign;

namespace Curves_editor.Core.Class
{
    public class Curve_point
    {
        public Point ellipse_positionID { get; set; }
        public Ellipse ellipseID { get; set; }
        public Curve_point(Point position, Ellipse ellipse)
        {
            ellipse_positionID = position;
            ellipseID = ellipse;
        }
    }

    internal class Curve
    {
        public List<Curve_point> pointArray = new List<Curve_point>();
        public List<Line> lines = new List<Line>();

        public void AddPoint(Point point, Ellipse ellipse)
        {
            Curve_point curve_Point = new Curve_point(point, ellipse);
            pointArray.Add(curve_Point);
        }

        public void UpdatePointPosition(Ellipse sender, Point e)
        {
            foreach (Curve_point curve_Point in pointArray) 
            {
                if (sender == curve_Point.ellipseID)
                {
                    curve_Point.ellipse_positionID = e;
                    break;
                }
            } 
        }
    }
}
