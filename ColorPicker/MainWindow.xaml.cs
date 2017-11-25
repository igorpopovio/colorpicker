using ColorMine.ColorSpaces;
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
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(ref Point lpPoint);

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

        bool isRunning;
        Color colorUnderCursor;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            isRunning = true;
            new HotKey(Key.C, KeyModifier.Ctrl, CopyToClipboard);

            new Thread(() =>
            {
                while (isRunning)
                {
                    Point p = new Point();
                    GetCursorPos(ref p);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        colorUnderCursor = GetColorAt(p.X, p.Y);
                        var myRgb = new Rgb
                        {
                            R = colorUnderCursor.R,
                            G = colorUnderCursor.G,
                            B = colorUnderCursor.B
                        };
                        var lch = myRgb.To<Lch>();
                        //SelectedColor.Background = new SolidColorBrush(colorUnderCursor);
                        //Hue.Text = $"{lch.H:0.##}";
                        //Chroma.Text = $"{lch.C:0.##}";
                        //Luminance.Text = $"{lch.L:0.##}";
                    }));

                    Thread.Sleep(100);
                }
            }).Start();
        }

        private void CopyToClipboard(HotKey key)
        {
            Clipboard.SetText(colorUnderCursor.ToString());
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            isRunning = false;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            // Begin dragging the window
            this.DragMove();
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
