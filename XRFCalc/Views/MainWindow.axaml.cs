using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace XRFCalc.Views;

public partial class MainWindow : Window
{
    //private static PixelRect? ScreenBounds;
    //public static Rect Screen => ScreenBounds.HasValue
    //    ? new Rect(ScreenBounds.Value.X, ScreenBounds.Value.Y, ScreenBounds.Value.Width, ScreenBounds.Value.Height)
    //    : new Rect(0, 0, 0, 0);
    public MainWindow()
    {
        InitializeComponent();
    }
}