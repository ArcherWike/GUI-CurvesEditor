using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Curves_editor.Core.Class
{
    internal class RectangleRGB
    {
        float time = 0.0f;

        UiDesign.MainWindow mainWindow_m;

        private Rectangle m_rectangle_rgb_shape = null;
        private Ellipse m_chart_marker = null;

        bool button_pause = false;
        DispatcherTimer timer_val = null;

        public RectangleRGB(Rectangle rectangle_rgb_shape, Ellipse chart_marker_shape, UiDesign.MainWindow mainWindow)
        {
            mainWindow_m = mainWindow;
            m_rectangle_rgb_shape = rectangle_rgb_shape;
            m_chart_marker = chart_marker_shape;
        }

        public void DisconectedTimer()
        {
            CompositionTarget.Rendering -= (timer_Tick);
        }

        public void ConnectTimer()
        {
            CompositionTarget.Rendering += (timer_Tick);
        }

        public class CompositionTargetEx
        {
            private static TimeSpan _last = TimeSpan.Zero;
            public static event EventHandler<RenderingEventArgs> _FrameUpdating;
            public static event EventHandler<RenderingEventArgs> FrameUpdating
            {
                add
                {
                    if (_FrameUpdating == null)
                    {
                        CompositionTarget.Rendering += CompositionTarget_Rendering;
                        _FrameUpdating += value;
                    }
                }
                remove
                {
                    _FrameUpdating -= value;

                    if (_FrameUpdating == null)
                        CompositionTarget.Rendering -= CompositionTarget_Rendering;
                }
            }
            static void CompositionTarget_Rendering(object sender, EventArgs e)
            {
                RenderingEventArgs args = (RenderingEventArgs)e;
                if (args.RenderingTime == _last)
                    return;
                _last = args.RenderingTime;
                _FrameUpdating(sender, args);
            }
        }

        public void SetPauseMode(bool pauseMode)
        {
            if (!pauseMode && button_pause)
            {
                ConnectTimer();
            }
            if (pauseMode)
            {
                DisconectedTimer();
            }
        }

        public bool SetPause()
        {
            if (button_pause)
            {
                button_pause = false;
                DisconectedTimer();
            }
            else
            {
                button_pause = true;
                ConnectTimer();
            }
            return button_pause;
        }

        float velocity_alpha;
        float velocity_red;
        float velocity_green;
        float velocity_blue;

        float color_max_value = 255.0f;

        float y;

        void timer_Tick(object sender, EventArgs e)
        {
            velocity_alpha = 255.0f;
            velocity_red = 0.0f;
            velocity_green = 0.0f;
            velocity_blue = 0.0f;

            float multiply = 0.313225490196078f;
            //float multiply = 255 / 800;

            if (mainWindow_m.active_curve != null)
            {
                time += 10;

                if (time > 6000)
                {
                    time = 0;
                }
                Point temp_point = new Point(time / 1000, 0);
                Point time_in_canvas_cord = mainWindow_m.GetCoordToCanvast(temp_point);
                Canvas.SetLeft(m_chart_marker, time_in_canvas_cord.X);

                foreach (Curve active_curve in mainWindow_m.curves)
                {
                    y = active_curve.GetValueAt((float)(time_in_canvas_cord.X)) * multiply;
                    switch (active_curve.globalCurveColor)
                    {
                        case Class.ColorType.Alpha:
                            velocity_alpha = color_max_value - y;
                            break;
                        case Class.ColorType.Red:
                            if (y > 0)
                            {
                                velocity_red = color_max_value - y;
                            }
                            break;
                        case Class.ColorType.Green:
                            if (y > 0)
                            {
                                velocity_green = color_max_value - y;
                            }
                            break;
                        case Class.ColorType.Blue:
                            if (y > 0)
                            {
                                velocity_blue = color_max_value - y;
                            }
                            break;
                    }
                }
            }
            //Console.WriteLine(velocity_red + " " + velocity_green + " " + velocity_blue + " " + velocity_alpha);

            m_rectangle_rgb_shape.Fill = new SolidColorBrush(Color.FromArgb(
                (byte)velocity_alpha,
                (byte)velocity_red,
                (byte)velocity_green,
                (byte)velocity_blue));
        }
    }
}
