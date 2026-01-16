using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Avalonia;
using Avalonia.Android;
using System;

namespace XRFCalc.Android;

[Activity(
    Label = "XRFCalc.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }


    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Your Avalonia initialization code here...
        // After the native window/content is available schedule the disabling:
        //Window?.DecorView?.Post(() => DisableSuggestionsForAllEditTexts(Window.DecorView));
    }

    //private void DisableSuggestionsForAllEditTexts(View root)
    //{
    //    try
    //    {
    //        if (root is ViewGroup vg)
    //        {
    //            for (int i = 0; i < vg.ChildCount; i++)
    //            {
    //                DisableSuggestionsForAllEditTexts(vg.GetChildAt(i));
    //            }
    //        }

    //        if (root is EditText editText)
    //        {
    //            // Turn off suggestions / spell-check for this EditText
    //            editText.InputType = InputTypes.ClassText | InputTypes.TextFlagNoSuggestions;

    //            // Optional: also disable full-screen extract UI when in landscape (depends on behavior you want)
    //            // editText.ImeOptions = Android.Views.InputMethods.ImeAction.Done;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        //Android.Util.Log.Warn("XRFCalc", "DisableSuggestionsForAllEditTexts failed: " + ex);
    //    }
    //}
}
