using Avalonia.Controls;

namespace XRFCalc.Views;

public partial class MainView : UserControl
{

    public enum XMode
    {
        Chemistry,
        XRays
    };
    
    public static XMode Mode
        {
            get;
            set;
        }  = XMode.Chemistry;
    public MainView()
    {
        InitializeComponent();
    }
}