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



namespace UiDesign
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Point> pointArray = new List<Point>();

        Ellipse activepoint = null;

        public MainWindow()
        {
            InitializeComponent();
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
    }
}
