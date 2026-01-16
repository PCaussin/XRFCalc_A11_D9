using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Styling;
using XRFCalc.Views;
using XRFCalc.XRF;
namespace XRFCalc;

internal partial class XRFCalcContent
{
    // Chemical formula
    private string Formula {get; set;}
    // Density g/cm³
    private double Density {get; set;}
    private double Energy {get; set;}
}