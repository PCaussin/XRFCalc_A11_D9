using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.IO;
using Avalonia.Platform;
using XRFCalc.XRF;
using Avalonia.Platform.Storage;

namespace XRFCalc;

public class XRFCalcUIDefinition
{

    public static TabControl? MainTab;
    public static bool? LandscapeOrientation
    {
        get
        {
            TopLevel? top = TopLevel.GetTopLevel(MainTab);
            return top == null ? null : top.Bounds.Width >= top.Bounds.Height;
        }
    }

    public static double FontSizeFactor
    {
        get => LandscapeOrientation.HasValue && LandscapeOrientation.Value ? 1 : 2;
    }


    static string help = "coucou";

    private static TabItem helpPage;

    public static void InitializeApplication(TabControl tab, IStorageProvider? storageProvider)
    {
        MainTab = tab;
        var item1 = new TabItem() { Header = "Chemistry", Background = new SolidColorBrush(Colors.Blue), Foreground = new SolidColorBrush(Colors.White) };
        var item2 = new TabItem() { Header = "Radiation", Background = new SolidColorBrush(Colors.Navy), Foreground = new SolidColorBrush(Colors.White) };
        helpPage = new TabItem() { Header = "Help", Background = new SolidColorBrush(Colors.Gray), Foreground = new SolidColorBrush(Colors.Black) };
        tab.Items.Add(item1);
        tab.Items.Add(item2);
        tab.Items.Add(helpPage);
        ;

        Persistence.Initialize(storageProvider);
        if (!Persistence.Initialized)
        {
            item1.Content = "Error initializing Persistence";
            tab.SelectedIndex = 0;
            return;
        }

        if (!InitElamTable())
        {
            item1.Content = "Error: failed to load Elam table";
            tab.SelectedIndex = 0;
            return;
        }
        else
        {
            XRFCalcContent.InitializeMendeleevTable();
            XRFCalcContent.InitializeChemistry();
            XRFCalcContent.InitializeRadiation();
            XRFCalcContent.InitializeData();
            //
            item1.Content = XRFCalcContent.ChemGrid;
            item2.Content = XRFCalcContent.RadGrid;
            MainTab.SelectionChanged += (s, e) =>
            {
                if (TabIndex == 2)
                {
                    UpdateHelp();
                }
            };
            Stream hlps = AssetLoader.Open(new Uri("Assets/helpFile.htm", UriKind.Relative), new Uri("avares://XRFCalc"));
            help = new StreamReader(hlps).ReadToEnd();
            hlps.Dispose();
            //UpdateHelp();
        }
    }

    private static void UpdateHelp()
    {
        try
        {
            NativeWebView webView = new();
            //webView.NavigateToString(ChangeFontSize(help, FontSizeFactor));
            webView.Navigate(new Uri("https://sites.google.com/view/xrfcalc/help"));
            helpPage.Content = webView;
        }
        catch (Exception ex)
        {
            helpPage.Content = $"Error loading help: {ex.Message}";
        }
    }


    public static int TabIndex => MainTab == null ? -1 : MainTab.SelectedIndex;

    private static string ChangeFontSize(string s, double factor)
    {
        if (factor == 1.0) return s;
        int ix = 0;
        while (ix + 14 < s.Length)
        {
            int ip = s.IndexOf("font-size:", ix, StringComparison.InvariantCultureIgnoreCase);
            if (ip < 0) break;
            ip += 10;
            int iq = s.IndexOf("pt", ip, StringComparison.InvariantCultureIgnoreCase);
            if (iq < 0)
            {
                ix = ip;
                continue;
            }
            if (double.TryParse(s.AsSpan(ip, iq - ip),
                    System.Globalization.CultureInfo.InvariantCulture, out double fontSize))
            {
                string ns = $"{factor * fontSize:f1}";
                s = string.Concat(s.AsSpan(0, ip), ns, s.AsSpan(iq));
            }
            ix = iq + 2;
        }
        return s;
    }


    private const char Lf = '\n';
    private static bool InitElamTable()
    {
        try
        {
            using Stream fs = AssetLoader.Open(new Uri("Assets/ElamDB12.txt", UriKind.Relative), new Uri("avares://XRFCalc"));
            using StreamReader sr = new(fs);
            string all = sr.ReadToEnd();
            return AbsorptionCalc.Initialize(all.Split(Lf));

        }
        catch (Exception e)
        {
            string s = e.Message;
            return false;
        }

    }


}

