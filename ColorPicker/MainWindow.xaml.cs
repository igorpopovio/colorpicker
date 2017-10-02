using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Drawing.Point;

namespace ColorPicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(ref Point lpPoint);


        public MainWindow()
        {
            InitializeComponent();
            
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        public static Color GetColorAt(int x, int y)
        {
            var desktop = GetDesktopWindow();
            var desktopContext = GetWindowDC(desktop);
            int pixel = (int)GetPixel(desktopContext, x, y);
            ReleaseDC(desktop, desktopContext);

            return Color.FromArgb(255,
                GetColorByte(ColorOffset.RED, pixel),
                GetColorByte(ColorOffset.GREEN, pixel),
                GetColorByte(ColorOffset.BLUE, pixel));
        }

        private static byte GetColorByte(ColorOffset offset, int pixel)
        {
            return Convert.ToByte((pixel >> (int)offset) & 0xff);
        }

        enum ColorOffset
        {
            RED = 0,
            GREEN = 8,
            BLUE = 16,
        }

        //private void Window_MouseMove(object sender, MouseEventArgs e)
        //{
        //    var p = PointToScreen(Mouse.GetPosition(this));
        //    var color = GetColorAt((int)p.X, (int)p.Y);
        //    border.Background = new SolidColorBrush(color);
        //    e.Handled = false;
        //}

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //CaptureMouse();
            new Thread(() =>
            {
                while (true)
                {
                    Point p = new Point();
                    GetCursorPos(ref p);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var color = GetColorAt(p.X, p.Y);
                        border.Background = new SolidColorBrush(color);
                    }));

                    Thread.Sleep(100);
                }
            }).Start();
        }

        //private void Window_Closing(object sender, CancelEventArgs e)
        //{
        //    base.OnClosing(e);
        //    ReleaseMouseCapture();
        //}
    }
}
