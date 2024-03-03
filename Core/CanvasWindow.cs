using System.Windows;
namespace CanvasWindow
{
    public struct Canvas_calculations
    { 
        public static Point GetCanvastToCoord(Point mousePosition)
        {
            Point result = new Point((mousePosition.X) / 200, 4 - (mousePosition.Y / 200));

            return result;
        }

        public static Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X * 200), 800 - pointPosition.Y * 200);

            return result;
        }
    }
}