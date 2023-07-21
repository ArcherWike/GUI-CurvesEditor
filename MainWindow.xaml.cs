using Curves_editor.Core.Class;
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
using static System.Net.Mime.MediaTypeNames;



namespace UiDesign
{
    public partial class MainWindow : Window
    {
        Ellipse activepoint = null;

        Ellipse circle_point = new Ellipse();
        Point CirclePoint = new Point(190, 120);
        MovableCirclePoint movablePoint;

        private List<Line> linesArray = new List<Line>();

        Curve curve = null;

        public MainWindow()
        {
            InitializeComponent();

            Create_circle_point();
            movablePoint = new MovableCirclePoint(CirclePoint, circle_point, this);
        }

        private void OnLineAdded(object sender, AddLineEventArgs e)
        {
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.LightSteelBlue;
            /*myLine.X1 = e.line.X1;
            myLine.Y1 = e.line.Y1;
            myLine.X2 = e.line.X2;
            myLine.Y2 = e.line.Y2;*/
            CordSys.Children.Add(myLine);
            linesArray.Add(myLine);
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

        private Ellipse CreatePoint(Point mousePosition)
        {
            Ellipse myEllipse = new Ellipse();

            myEllipse.Width = 30;
            myEllipse.Height = 30;
            myEllipse.Fill = System.Windows.Media.Brushes.White;
            CordSys.Children.Add(myEllipse);
            SetPointPosition(myEllipse, mousePosition);
            myEllipse.MouseEnter += Ellipse_mouseEnter;
            myEllipse.MouseLeave += Ellipse_mouseLeave;
            myEllipse.MouseLeftButtonDown += MyEllipse_MouseLeftButtonDown;
            myEllipse.MouseLeftButtonUp += MyEllipse_MouseLeftButtonUp;

            return myEllipse;
        }

        public void UpdateLinePosition(object sender, AddLineEventArgs e)
        {
            Line activeLine = linesArray[curve.activeIndex];

            if (sender == null)
            {
                activeLine.X1 = GetCoordToCanvast(new Point
                    (e.line.X1, e.line.Y1)).X;
                activeLine.Y1 = GetCoordToCanvast(new Point
                    (e.line.X1, e.line.Y1)).Y;
            }
            else
            {
                Point newLinePoint = GetCoordToCanvast(new Point
                    (e.line.X2, e.line.Y2));
                activeLine.X2 = newLinePoint.X;
                activeLine.Y2 = newLinePoint.Y;
            }
        }
        public void UpdateLines(object sender, AddLineEventArgs e)
        {
            int point_index = 0;
            foreach (Line active_line in linesArray)
            {
                Point startLine = GetCoordToCanvast(new Point(
                    curve.pointArray[point_index].ellipse_positionID.X,
                    curve.pointArray[point_index].ellipse_positionID.Y));

                active_line.X1 = startLine.X;
                active_line.Y1 = startLine.Y;

                point_index++;

                Point endLine = GetCoordToCanvast(new Point(
                    curve.pointArray[point_index].ellipse_positionID.X,
                    curve.pointArray[point_index].ellipse_positionID.Y));

                active_line.X2 = endLine.X;
                active_line.Y2 = endLine.Y;
            }
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
        private void MyEllipse_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            activepoint = null;
            e.Handled = true;
        }

        private void MyEllipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            activepoint = (sender as Ellipse);
        }

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
            if (curve == null)
            {
                curve = new Curve();
                dataGrid.ItemsSource = curve.pointArray;

                curve.OnLineAdded += new Curve.AddLineHandler(OnLineAdded);
                curve.UpdateLinePosition += new Curve.AddLineHandler(UpdateLinePosition);
                curve.UpdateLines += new Curve.AddLineHandler(UpdateLines);
            }
            //if (activepoint == null) ///////////////##
            //{
            curve.AddPoint(GetCanvastToCoord(
                e.GetPosition(this.CordSys)),
                CreatePoint(e.GetPosition(this.CordSys)));
            //}
        }

        private void CordSys_MouseMove(object sender, MouseEventArgs e)
        {
            if (activepoint != null)
            {
                SetPointPosition(activepoint, e.GetPosition(this.CordSys));
                dataGrid.Items.Refresh();
            }
        }



        private void test(object sender, MouseButtonEventArgs e)
        {

        }


    }
}