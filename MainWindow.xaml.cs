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

namespace UiDesign
{
    public partial class MainWindow : Window
    {
        

        Ellipse activepoint = null;

        Ellipse circle_point = new Ellipse();
        
        Point CirclePoint = new Point(190, 120);

        MovableCirclePoint movablePoint;

        Curve curve = null;

        public delegate void EllypseMoveHandler(object sender, AddPointEventArgs e);
        public event EllypseMoveHandler OnEllypseMove;

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
        void UpdatePoint(object sender, AddPointEventArgs e)
        {
            if (curve != null)
            {
                //curve.UpdatePointPosition();
            }
        }
        public void SetPointPosition(Ellipse myEllipse, Point mousePoint) 
        {

            if (curve != null)
            {
                Point delta = new Point(mousePoint.X - 15 - Canvas.GetLeft(myEllipse), 
                    Canvas.GetTop(myEllipse) - mousePoint.Y + 15);

                curve.UpdatePointPosition(myEllipse, delta);
                /* AddPointEventArgs eventArgs = new AddPointEventArgs(delta);
                 OnEllypseMove(myEllipse, eventArgs);*/
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

        private Point GetCanvastToCoord(Point mousePosition)
        {
            Point result = new Point((mousePosition.X - 40)/200, 4 - (mousePosition.Y/200));

            return result;
        }
        private Point GetCoordToCanvast(Point pointPosition)
        {
            Point result = new Point((pointPosition.X *200) + 40, 800 - pointPosition.Y * 200);

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
                //OnEllypseMove += new EllypseMoveHandler(Curve.)
                //curve.OnPointAdded += new Curve.AddPointHandler(DrawPoint);
            }
            curve.AddPoint(GetCanvastToCoord(e.GetPosition(this.CordSys)), CreatePoint(e.GetPosition(this.CordSys)));
        }

        private void CordSys_MouseMove(object sender, MouseEventArgs e)
        {
            if (activepoint != null)
            {
                SetPointPosition(activepoint, e.GetPosition(this.CordSys));
                
            }
        }
    }
}
