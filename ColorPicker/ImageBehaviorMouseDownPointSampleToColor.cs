using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker
{
    [Description("Used to sample the color under mouse for the image when the mouse is pressed. ")]
    public class ImageBehaviorMouseDownPointSampleToColor : Behavior<Image>
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(Color),
                                        typeof(ImageBehaviorMouseDownPointSampleToColor),
                                        new UIPropertyMetadata(Colors.White));


        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SamplePixelForColor();
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SamplePixelForColor();
            }
        }

        private void SamplePixelForColor()
        {
            // Retrieve the coordinate of the mouse position in relation to the supplied image.
            Point point = Mouse.GetPosition(AssociatedObject);

            // Use RenderTargetBitmap to get the visual, in case the image has been transformed.
            var renderTargetBitmap = new RenderTargetBitmap((int)AssociatedObject.ActualWidth,
                                                            (int)AssociatedObject.ActualHeight,
                                                            96, 96, PixelFormats.Default);
            renderTargetBitmap.Render(AssociatedObject);

            // Make sure that the point is within the dimensions of the image.
            if ((point.X <= renderTargetBitmap.PixelWidth) && (point.Y <= renderTargetBitmap.PixelHeight))
            {
                // Create a cropped image at the supplied point coordinates.
                var croppedBitmap = new CroppedBitmap(renderTargetBitmap,
                                                      new Int32Rect((int)point.X, (int)point.Y, 1, 1));

                // Copy the sampled pixel to a byte array.
                var pixels = new byte[4];
                croppedBitmap.CopyPixels(pixels, 4, 0);

                // Assign the sampled color to a SolidColorBrush and return as conversion.
                SelectedColor = Color.FromArgb(255, pixels[2], pixels[1], pixels[0]);
            }
        }
    }
}