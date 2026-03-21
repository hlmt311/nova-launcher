using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NovaGamingOptimizer
{
    public partial class CrosshairWindow : Window
    {
        private CrosshairOverlay? _overlay;
        private Color _color = Colors.LimeGreen;

        public CrosshairWindow() { InitializeComponent(); DrawPreview(); }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) DragMove(); }
        private void Close_Click(object sender, RoutedEventArgs e) { _overlay?.Close(); Close(); }

        private void Slider_Changed(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (SizeLabel != null) SizeLabel.Text = $"{(int)SizeSlider.Value}px";
            if (ThickLabel != null) ThickLabel.Text = $"{(int)ThickSlider.Value}px";
            if (GapLabel != null) GapLabel.Text = $"{(int)GapSlider.Value}px";
            DrawPreview();
            _overlay?.Draw(_color, SizeSlider.Value, ThickSlider.Value, GapSlider.Value);
        }

        private void HexInput_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                string hex = HexInput.Text.Trim();
                if (!hex.StartsWith("#")) hex = "#" + hex;
                if (hex.Length == 7)
                {
                    _color = (Color)ColorConverter.ConvertFromString(hex);
                    ColorPreview.Background = new SolidColorBrush(_color);
                    DrawPreview();
                    _overlay?.Draw(_color, SizeSlider.Value, ThickSlider.Value, GapSlider.Value);
                }
            }
            catch { }
        }

        private void DrawPreview()
        {
            try
            {
                if (Preview == null) return;
                Preview.Children.Clear();
                double cx = 95, cy = 95;
                double s = SizeSlider?.Value ?? 10;
                double t = ThickSlider?.Value ?? 2;
                double g = GapSlider?.Value ?? 4;
                var b = new SolidColorBrush(_color);
                Preview.Children.Add(new Line { X1=cx-s-g, Y1=cy, X2=cx-g, Y2=cy, Stroke=b, StrokeThickness=t });
                Preview.Children.Add(new Line { X1=cx+g, Y1=cy, X2=cx+s+g, Y2=cy, Stroke=b, StrokeThickness=t });
                Preview.Children.Add(new Line { X1=cx, Y1=cy-s-g, X2=cx, Y2=cy-g, Stroke=b, StrokeThickness=t });
                Preview.Children.Add(new Line { X1=cx, Y1=cy+g, X2=cx, Y2=cy+s+g, Stroke=b, StrokeThickness=t });
            }
            catch { }
        }

        private void ShowOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (_overlay == null || !_overlay.IsVisible)
            { _overlay = new CrosshairOverlay(); _overlay.Draw(_color, SizeSlider.Value, ThickSlider.Value, GapSlider.Value); _overlay.Show(); }
        }
        private void HideOverlay_Click(object sender, RoutedEventArgs e) => _overlay?.Hide();
    }

    public class CrosshairOverlay : Window
    {
        [DllImport("user32.dll")] static extern int GetWindowLong(IntPtr h, int i);
        [DllImport("user32.dll")] static extern int SetWindowLong(IntPtr h, int i, int v);
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_LAYERED = 0x80000;
        private System.Windows.Controls.Canvas _c = new() { Background = System.Windows.Media.Brushes.Transparent };

        public CrosshairOverlay()
        {
            WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = System.Windows.Media.Brushes.Transparent;
            ShowInTaskbar = false; Topmost = true;
            Width = SystemParameters.PrimaryScreenWidth; Height = SystemParameters.PrimaryScreenHeight; Left = 0; Top = 0;
            Content = _c;
            Loaded += (s, e) =>
            {
                var h = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                SetWindowLong(h, GWL_EXSTYLE, GetWindowLong(h, GWL_EXSTYLE) | WS_EX_TRANSPARENT | WS_EX_LAYERED);
            };
        }

        public void Draw(Color color, double size, double thick, double gap)
        {
            _c.Children.Clear();
            double cx = Width / 2, cy = Height / 2;
            var b = new SolidColorBrush(color);
            _c.Children.Add(new Line { X1=cx-size-gap, Y1=cy, X2=cx-gap, Y2=cy, Stroke=b, StrokeThickness=thick });
            _c.Children.Add(new Line { X1=cx+gap, Y1=cy, X2=cx+size+gap, Y2=cy, Stroke=b, StrokeThickness=thick });
            _c.Children.Add(new Line { X1=cx, Y1=cy-size-gap, X2=cx, Y2=cy-gap, Stroke=b, StrokeThickness=thick });
            _c.Children.Add(new Line { X1=cx, Y1=cy+gap, X2=cx, Y2=cy+size+gap, Stroke=b, StrokeThickness=thick });
        }
    }
}
