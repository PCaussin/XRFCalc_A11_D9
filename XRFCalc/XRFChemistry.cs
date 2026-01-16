using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.TextInput;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using XRFCalc.XRF;

namespace XRFCalc;


internal partial class XRFCalcContent
{
    private static readonly Thickness margin = new(0, 0, 1, 1);
    private static readonly Thickness margin2 = new(0, 0, 1, 1);

    internal static Grid ChemGrid = new(); // chemistry grid
    internal static Grid RadGrid = new(); // radiation grid

    private static readonly Label formulaLabel = new()
    {
        Content = "Formula",
        Margin = margin,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static readonly TextBox formulaTextBox = new()
    {
        Watermark = "Enter chemical formula or use ...",
        Margin = margin,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalContentAlignment = HorizontalAlignment.Stretch,

    };

    public static readonly TextBox DensityTextBox = new()
    {
        Watermark = "g/cm³",
        Margin = margin,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    public static readonly TextBox DensityTextBox2 = new()
    {
        Watermark = "g/cm³",
        Margin = margin,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    private static readonly TextBox radiationTextBox = new()
    {
        Watermark = "Radiation (line name, energy or wavelength+A)",
        Margin = margin,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch
    };

    private static readonly Button radiationButton = new()
    {
        Content = "...",
        Margin = margin,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Center,
        HorizontalContentAlignment = HorizontalAlignment.Center
    };

    private static readonly TextBox altRadiationTextBox = new()
    { IsReadOnly = true, Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly TextBox photoAbsorptionTextBox = new()
    { IsReadOnly = true, Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly TextBox rayleighAbsorptionTextBox = new()
    { IsReadOnly = true, Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly TextBox comptonAbsorptionTextBox = new()
    { IsReadOnly = true, Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly TextBox totalAbsorptionTextBox = new()
    { IsReadOnly = true, Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly TextBox pathAt90AbsorptionTextBox = new()
    { IsReadOnly = true, Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label radiationLabel = new()
    { Content = "Radiation", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label altRadiationLabel = new()
    { Content = "Alternate designation", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label photoAbsorptionLabel = new()
    { Content = "Photo absorption", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label rayleighAbsorptionLabel = new()
    { Content = "Rayleigh absorption", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label comptonAbsorptionLabel = new()
    { Content = "Compton absorption", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label totalAbsorptionLabel = new()
    { Content = "Total absorption", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label pathAt90AbsorptionLabel = new()
    { Content = "Path at 90% absorption", Margin = margin, VerticalAlignment = VerticalAlignment.Center };


    private static readonly Dictionary<string, double> densities = new();

    private static readonly Button formulaButton = new()
    {
        Content = "...",
        Margin = margin2,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Center,
        HorizontalContentAlignment = HorizontalAlignment.Center
    };
    private static readonly Button formulaEraser = new()
    {
        Content = "X",
        Margin = margin2,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalContentAlignment = VerticalAlignment.Center,
        HorizontalContentAlignment = HorizontalAlignment.Center
    };

    private static readonly Label densityLabel = new()
    { Content = "Density", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Label densityLabel2 = new()
    { Content = "Density", Margin = margin, VerticalAlignment = VerticalAlignment.Center };

    private static readonly Button oxideButton = new() { Content = "Oxide", IsEnabled = false, Margin = margin };
    private static readonly CheckBox autoCap = new() { Content = "Auto Cap", IsChecked = true, Margin = margin };
    private static readonly ListBox chemGridList = new() { HorizontalAlignment = HorizontalAlignment.Stretch, Margin = margin };

    //


    internal static void InitializeChemistry()
    {
        ChemGrid.Children.Clear();
        ChemGrid.RowDefinitions.Clear();
        ChemGrid.RowDefinitions.Capacity = 4;
        ChemGrid.ColumnDefinitions.Clear();
        ChemGrid.ColumnDefinitions.Capacity = 3;
        ChemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) });
        ChemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.65, GridUnitType.Star) });
        ChemGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.20, GridUnitType.Star) });
        ChemGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.1, GridUnitType.Star) });
        ChemGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.1, GridUnitType.Star) });
        ChemGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.1, GridUnitType.Star) });
        ChemGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.7, GridUnitType.Star) });

        formulaLabel.SetValue(Grid.ColumnProperty, 0);
        formulaTextBox.SetValue(Grid.RowProperty, 0);
        formulaTextBox.SetValue(Grid.ColumnProperty, 1);
        formulaTextBox.TextChanged += (s, e) =>
        {
            if (!e.Handled && autoCap.IsChecked.HasValue && autoCap.IsChecked.Value) Capitalize(s, e);
        };
        TextInputOptions.SetShowSuggestions(formulaTextBox, false);
        formulaButton.SetValue(Grid.ColumnProperty, 0);
        formulaEraser.SetValue(Grid.RowProperty, 0);
        formulaEraser.SetValue(Grid.ColumnProperty, 1);
        formulaEraser.Click += (s, e) =>
        {
            ignoreFormulaOnce = true;
            formulaString = "";
            formulaTextBox.Text = "";
            CalcElements("");
        };
        formulaButton.SetValue(Grid.RowProperty, 0);
        formulaButton.Click += (s, e) => ShowTable(0);
        densityLabel.SetValue(Grid.ColumnProperty, 0);
        densityLabel.SetValue(Grid.RowProperty, 1);
        DensityTextBox.SetValue(Grid.ColumnProperty, 1);
        DensityTextBox.SetValue(Grid.RowProperty, 1);
        DensityTextBox.TextChanged += (s, e) =>
        {
            if (e.Handled) return;
            e.Handled = true;
            if (lockDensityOnce)
            {
                lockDensityOnce = false;
                return;
            }
            lockDensity2Once = string.Compare(DensityTextBox2.Text, DensityTextBox.Text) != 0;
            if (lockDensity2Once)
                DensityTextBox2.Text = DensityTextBox.Text;
            if (double.TryParse(DensityTextBox.Text, out var d) && d > 0 && d < 25 && nbEl > 0)
            {
                densities[saveFormula] = d;
                DisplayPathAt90();
                SaveChemistry();
            }
            densityModified = true;
        };
        TextInputOptions.SetShowSuggestions(DensityTextBox, false);

        oxideButton.SetValue(Grid.ColumnProperty, 0);
        oxideButton.SetValue(Grid.RowProperty, 2);
        oxideButton.Click += (s, e) =>
        {
            if (nbEl == 1)
            {
                var elNum = Chemistry.AtomicNumber(formulaString);
                if (elNum > 0)
                {
                    Chemistry.NormalOxide(elNum, out string oxide, out float _);
                    if (!string.IsNullOrEmpty(oxide))
                    {
                        ignoreFormulaOnce = true;
                        formulaTextBox.Text = oxide;
                        CalcElements(oxide);
                        SaveChemistry();
                    }
                }
            }
        };
        autoCap.SetValue(Grid.ColumnProperty, 1);
        autoCap.SetValue(Grid.RowProperty, 2);
        chemGridList.SetValue(Grid.ColumnProperty, 0);
        chemGridList.SetValue(Grid.ColumnSpanProperty, 3);
        chemGridList.SetValue(Grid.RowProperty, 3);
        ChemGrid.Children.Add(formulaLabel);
        ChemGrid.Children.Add(formulaTextBox);
        var dualgrid = new Grid();
        dualgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
        dualgrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
        dualgrid.Children.Add(formulaEraser);
        dualgrid.Children.Add(formulaButton);
        dualgrid.SetValue(Grid.ColumnProperty, 2);
        dualgrid.SetValue(Grid.RowProperty, 0);
        ChemGrid.Children.Add(dualgrid);
        ChemGrid.Children.Add(densityLabel);
        ChemGrid.Children.Add(DensityTextBox);
        ChemGrid.Children.Add(oxideButton);
        ChemGrid.Children.Add(autoCap);
        ChemGrid.Children.Add(chemGridList);
    }

    internal static void InitializeRadiation()
    {
        RadGrid.Children.Clear();
        RadGrid.RowDefinitions.Clear();
        RadGrid.RowDefinitions.Capacity = 10;
        RadGrid.ColumnDefinitions.Clear();
        RadGrid.ColumnDefinitions.Capacity = 4;
        RadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) });
        RadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.15, GridUnitType.Star) });
        RadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.6, GridUnitType.Star) });
        RadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.1, GridUnitType.Star) });
        for (int i = 0; i < 10; i++) RadGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.1, GridUnitType.Star) });

        radiationLabel.SetValue(Grid.ColumnProperty, 0);
        radiationLabel.SetValue(Grid.RowProperty, 0);
        radiationTextBox.SetValue(Grid.ColumnProperty, 1);
        radiationTextBox.SetValue(Grid.RowProperty, 0);
        radiationTextBox.SetValue(Grid.ColumnSpanProperty, 2);
        radiationTextBox.TextChanged += (s, e) =>
        {
            if (e.Handled) return;
            e.Handled = true;
            string t = radiationTextBox.Text ?? "";
            if (AbsorptionCalc.FindEnergy(ref t, out int edge, out string alt) > 0)
            {
                if (t != radiationTextBox.Text)
                {
                    radiationTextBox.Text = t;
                    radiationTextBox.CaretIndex = t.Length;
                }
            }
            ComputeAbsorption();
            SaveChemistry();
        };
        TextInputOptions.SetShowSuggestions(radiationTextBox, false);
        radiationButton.SetValue(Grid.ColumnProperty, 3);
        radiationButton.SetValue(Grid.RowProperty, 0);
        radiationButton.Click += (s, e) => ShowTable(1);
        altRadiationLabel.SetValue(Grid.ColumnProperty, 0);
        altRadiationLabel.SetValue(Grid.ColumnSpanProperty, 2);
        altRadiationLabel.SetValue(Grid.RowProperty, 1);
        altRadiationTextBox.SetValue(Grid.ColumnProperty, 2);
        altRadiationTextBox.SetValue(Grid.ColumnSpanProperty, 2);
        altRadiationTextBox.SetValue(Grid.RowProperty, 1);
        photoAbsorptionLabel.SetValue(Grid.ColumnProperty, 0);
        photoAbsorptionLabel.SetValue(Grid.ColumnSpanProperty, 2);
        photoAbsorptionLabel.SetValue(Grid.RowProperty, 2);
        photoAbsorptionTextBox.SetValue(Grid.ColumnProperty, 2);
        photoAbsorptionTextBox.SetValue(Grid.ColumnSpanProperty, 2);
        photoAbsorptionTextBox.SetValue(Grid.RowProperty, 2);
        rayleighAbsorptionLabel.SetValue(Grid.ColumnProperty, 0);
        rayleighAbsorptionLabel.SetValue(Grid.ColumnSpanProperty, 2);
        rayleighAbsorptionLabel.SetValue(Grid.RowProperty, 3);
        rayleighAbsorptionTextBox.SetValue(Grid.ColumnProperty, 2);
        rayleighAbsorptionTextBox.SetValue(Grid.ColumnSpanProperty, 2);
        rayleighAbsorptionTextBox.SetValue(Grid.RowProperty, 3);
        comptonAbsorptionLabel.SetValue(Grid.ColumnProperty, 0);
        comptonAbsorptionLabel.SetValue(Grid.ColumnSpanProperty, 2);
        comptonAbsorptionLabel.SetValue(Grid.RowProperty, 4);
        comptonAbsorptionTextBox.SetValue(Grid.ColumnProperty, 2);
        comptonAbsorptionTextBox.SetValue(Grid.ColumnSpanProperty, 2);
        comptonAbsorptionTextBox.SetValue(Grid.RowProperty, 4);
        totalAbsorptionLabel.SetValue(Grid.ColumnProperty, 0);
        totalAbsorptionLabel.SetValue(Grid.ColumnSpanProperty, 2);
        totalAbsorptionLabel.SetValue(Grid.RowProperty, 5);
        totalAbsorptionTextBox.SetValue(Grid.ColumnProperty, 2);
        totalAbsorptionTextBox.SetValue(Grid.ColumnSpanProperty, 2);
        totalAbsorptionTextBox.SetValue(Grid.RowProperty, 5);
        densityLabel2.SetValue(Grid.ColumnProperty, 0);
        densityLabel2.SetValue(Grid.ColumnSpanProperty, 2);
        densityLabel2.SetValue(Grid.RowProperty, 6);
        DensityTextBox2.SetValue(Grid.ColumnProperty, 2);
        DensityTextBox2.SetValue(Grid.ColumnSpanProperty, 2);
        DensityTextBox2.SetValue(Grid.RowProperty, 6);
        DensityTextBox2.TextChanged += (s, e) =>
        {
            if (e.Handled) return;
            e.Handled = true;
            if (lockDensity2Once)
            {
                lockDensity2Once = false;
                return;
            }
            lockDensityOnce = string.Compare(DensityTextBox.Text, DensityTextBox2.Text) != 0;
            if (lockDensityOnce) DensityTextBox.Text = DensityTextBox2.Text;
            if (double.TryParse(DensityTextBox2.Text, out var d) && d > 0 && d < 25 && nbEl > 0)
            {
                densities[saveFormula] = d;
                DisplayPathAt90();
                SaveChemistry();
            }
            densityModified = true;
        };
        TextInputOptions.SetShowSuggestions(DensityTextBox2, false);

        pathAt90AbsorptionLabel.SetValue(Grid.ColumnProperty, 0);
        pathAt90AbsorptionLabel.SetValue(Grid.ColumnSpanProperty, 2);
        pathAt90AbsorptionLabel.SetValue(Grid.RowProperty, 7);
        pathAt90AbsorptionTextBox.SetValue(Grid.ColumnProperty, 2);
        pathAt90AbsorptionTextBox.SetValue(Grid.ColumnSpanProperty, 2);
        pathAt90AbsorptionTextBox.SetValue(Grid.RowProperty, 7);

        RadGrid.Children.Add(radiationLabel);
        RadGrid.Children.Add(radiationTextBox);
        RadGrid.Children.Add(radiationButton);
        RadGrid.Children.Add(altRadiationLabel);
        RadGrid.Children.Add(altRadiationTextBox);
        RadGrid.Children.Add(photoAbsorptionLabel);
        RadGrid.Children.Add(photoAbsorptionTextBox);
        RadGrid.Children.Add(rayleighAbsorptionLabel);
        RadGrid.Children.Add(rayleighAbsorptionTextBox);
        RadGrid.Children.Add(comptonAbsorptionLabel);
        RadGrid.Children.Add(comptonAbsorptionTextBox);
        RadGrid.Children.Add(totalAbsorptionLabel);
        RadGrid.Children.Add(totalAbsorptionTextBox);
        RadGrid.Children.Add(densityLabel2);
        RadGrid.Children.Add(DensityTextBox2);
        RadGrid.Children.Add(pathAt90AbsorptionLabel);
        RadGrid.Children.Add(pathAt90AbsorptionTextBox);

    }


    private static bool ignoreFormulaOnce = false;
    private static bool densityModified = false;
    private static bool readingInput = false;
    private static bool capNext = false;
    private static string formulaString = "";
    private static string saveFormula = "";
    private static int nbEl = 0;

    private static void ShowTable(int mode)
    {
        if (XRFCalcUIDefinition.MainTab != null && XRFCalcUIDefinition.MainTab.Items[mode] is TabItem t)
        {
            InitializeMendeleevTable();
            t.Content = TableView;
        }
    }

    private static void Capitalize(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            e.Handled = true;
            if (ignoreFormulaOnce)
            {
                ignoreFormulaOnce = false;
                return;
            }

            densityModified = false;
            var f1 = formulaTextBox.Text;
            if (f1 == null)
                return;
            if (f1.Length > 0 && f1.Length == saveFormula.Length + 1 &&
                string.Compare(f1[..^1], saveFormula, StringComparison.CurrentCultureIgnoreCase) ==
                0) // 1 char added at end
                formulaString = saveFormula + f1[^1];
            else
                formulaString = f1;

            var nn = formulaString.IndexOf(';') + 1;
            if (!readingInput)
            {
                var len = formulaString.Length;
                if (len <= 1) capNext = true;
                else if (nn > 0 && len > nn) capNext = false;
                if ((nn > 0 && len == nn + 1) ||
                    ((len == 1 || (len == saveFormula.Length + 1 && formulaString[..^1] == saveFormula)) &&
                     (capNext || (len > 1 && Chemistry.AtomicNumber(formulaString.Substring(len - 2, 2)) == 0 &&
                                  Chemistry.AtomicNumber(formulaString.Substring(len - 2, 1)) > 0))))
                {
                    var t = formulaString[len - 1];
                    if (t >= 'a' && t <= 'z')
                    {
                        formulaString = formulaString[..(len - 1)] + char.ToUpper(t);
                        ignoreFormulaOnce = true;
                        formulaTextBox.Text = formulaString;
                    }
                }

                var tu = len >= 1 ? formulaString[len - 1] : '\0';
                capNext = tu < 'A' || tu > 'Z';
            }

            CalcElements(formulaString);
            if (!readingInput && nbEl > 0)
                SaveChemistry();
        }
    }

    private static bool lockDensityOnce = false;
    private static bool lockDensity2Once = false;


    private static void CalcElements(string formulaString)
    {
        var pnZ = new int[Chemistry.MaxElementsD];
        var pnAtom = new float[Chemistry.MaxElementsD];
        var pfFraction = new float[Chemistry.MaxElementsD];

        nbEl = Chemistry.DecodeFormula(formulaString, pnZ, pnAtom, pfFraction, out float weight);
        saveFormula = formulaString;

        string s = "";
        if (nbEl > 0)
        {
            if (densities.ContainsKey(saveFormula) && !densityModified)
                s = densities[saveFormula].ToString("G5");
            else if (nbEl == 1)
                s = AbsorptionCalc.ElementDensity(pnZ[0]).ToString("G5");
        }

        lockDensityOnce = string.Compare(DensityTextBox.Text, s) != 0;
        lockDensity2Once = string.Compare(DensityTextBox2.Text, s) != 0;
        if (lockDensity2Once)
        {
            DensityTextBox2.Text = s;
            densityModified = false;
        }
        if (lockDensityOnce)
        {
            DensityTextBox.Text = s;
            densityModified = false;
        }


        oxideButton.IsEnabled = nbEl == 1 && !Chemistry.HasNoOxidation(pnZ[0]);

        chemGridList.Items.Clear();
        if (nbEl > 0) chemGridList.Items.Add($"Molecular mass: {weight:G5}");
        bool hasZeros = false;
        for (int i = 0; i < nbEl; i++)
            if (pnAtom[i] <= 0)
            { hasZeros = true; break; }
        for (int i = 0; i < nbEl; i++)
        {
            var elemName = Chemistry.gacElements[pnZ[i]];
            var atomNum = pnAtom[i];
            var frac = pfFraction[i];
            int z = pnZ[i];
            switch (z)
            {
                case 103:
                case 105:
                    z = 1;
                    break;
                case 104:
                    z = 65;
                    break;
            }
            object[] dsp = new object[hasZeros ? 4 : 5];
            dsp[0] = elemName;
            if (!hasZeros) dsp[4] = atomNum;
            dsp[3] = z;
            dsp[1] = Chemistry.gafAtomWeight[pnZ[i]];
            dsp[2] = frac;
            string fmt = hasZeros ? "{0,2} | Z = {3,2} | A = {1,6:F2} | P = {2,5:P2}" : "{0,2} ({4,6:G4}) | Z = {3,2} | A = {1,6:F2} | P = {2,5:P2}";
            chemGridList.Items.Add(String.Format(fmt, dsp));
        }
        ComputeAbsorption();

    }

    private const char Equ = '=';

    public static void InitializeData()
    {
        readingInput = true;
        var datas = Persistence.ReadIniFile();
        if (datas == null) return;
        var inDensities = false;
        foreach (var s in datas)
        {
            var split = s.Split(Equ);
            if (split.Length != 2) continue;
            var key = split[0].Trim();
            var value = split[1].Trim();
            if (!inDensities)
            {
                if (string.Compare(key, "autocap", StringComparison.InvariantCultureIgnoreCase) == 0)
                    autoCap.IsChecked =
                        string.Compare(value, "true", StringComparison.InvariantCultureIgnoreCase) == 0;
                else if (string.Compare(key, "formula", StringComparison.InvariantCultureIgnoreCase) == 0)
                    formulaTextBox.Text = value;
                else if (string.Compare(key, "radiation", StringComparison.InvariantCultureIgnoreCase) == 0)
                    radiationTextBox.Text = value;
                else if (string.Compare(key, "fontfactor", StringComparison.InvariantCultureIgnoreCase) == 0
                    && double.TryParse(value, out double x))
                    Persistence.SavedFactor = x;
                else if (string.Compare(key, "density", StringComparison.InvariantCultureIgnoreCase) == 0)
                    inDensities = true;
            }
            else if (double.TryParse(value, out var density) && density > 0)
            {
                densities[key] = density;
            }
        }
        ComputeAbsorption();

        readingInput = false;
    }

    private static void SaveChemistry()
    {
        List<string> datas =
        [
            $"Autocap = {(autoCap.IsChecked.HasValue && autoCap.IsChecked.Value
                 ? "true"
                 : "false")}",
             $"Formula = {formulaTextBox.Text}",
             $"Radiation = {radiationTextBox.Text}",
             $"FontFactor = {Persistence.SavedFactor:G6}",
             "Density = ..."
        ];
        foreach (var s in densities)
            datas.Add($"{s.Key} = {s.Value:G5}");
        Persistence.SaveIniFile(datas);
    }

    private static float totalAbsorp = 0;
    private static float totalAbsorp2 = 0;

    private static void ComputeAbsorption()
    {
        var radInput = radiationTextBox.Text;
        if (string.IsNullOrWhiteSpace(radInput) || nbEl == 0)
        {
            altRadiationTextBox.Text = "";
            photoAbsorptionTextBox.Text = "";
            rayleighAbsorptionTextBox.Text = "";
            comptonAbsorptionTextBox.Text = "";
            totalAbsorptionTextBox.Text = "";
            pathAt90AbsorptionTextBox.Text = "";
            return;
        }
        var pnZ = new int[Chemistry.MaxElementsD];
        var pnAtom = new float[Chemistry.MaxElementsD];
        var pfFraction = new float[Chemistry.MaxElementsD];
        nbEl = Chemistry.DecodeFormula(formulaString, pnZ, pnAtom, pfFraction, out float weight);
        if (nbEl == 0)
        {
            altRadiationTextBox.Text = "";
            photoAbsorptionTextBox.Text = "";
            rayleighAbsorptionTextBox.Text = "";
            comptonAbsorptionTextBox.Text = "";
            totalAbsorptionTextBox.Text = "";
            pathAt90AbsorptionTextBox.Text = "";
            return;
        }
        string alt = "";
        int edge = 0;
        float energy = 0;
        bool isWavelengthAngstrom = radInput.EndsWith("a", StringComparison.InvariantCultureIgnoreCase);
        if (isWavelengthAngstrom)
        {
            radInput = radInput[..^1].TrimEnd();
            if (float.TryParse(radInput, out float wavelength) && wavelength > 0)
            {
                energy = AbsorptionCalc.HC / wavelength;
            }
        }
        else if (!float.TryParse(radiationTextBox.Text, out energy) || energy <= 0)
        {
            energy = AbsorptionCalc.FindEnergy(ref radInput, out edge, out alt);
        }
        if (energy <= 0)
        {
            altRadiationTextBox.Text = "Invalid radiation";
            photoAbsorptionTextBox.Text = "";
            rayleighAbsorptionTextBox.Text = "";
            comptonAbsorptionTextBox.Text = "";
            totalAbsorptionTextBox.Text = "";
            pathAt90AbsorptionTextBox.Text = "";
            return;
        }
        altRadiationTextBox.Text = string.IsNullOrWhiteSpace(alt)
            ? $"{energy:g5}keV; {AbsorptionCalc.HC / energy:g5}Å"
            : $"{alt}; {energy:g5}keV; {AbsorptionCalc.HC / energy:g5}Å";
        float photoAbsorp = 0;
        float rayleighAbsorp = 0;
        float comptonAbsorp = 0;
        float photoAbsorp2 = 0;

        float en1m = 0;
        float en1p = 0;

        bool nearEdge = false;
        if (edge > 0)
        {
            for (int i = 0; i < nbEl; i++)
            {
                if (pnZ[i] == edge)
                {
                    en1m = energy - 0.002f;
                    en1p = energy + 0.002f;
                    nearEdge = true;
                    break;
                }
            }
        }

        for (int i = 0; i < nbEl; i++)
        {
            var z = pnZ[i];
            var frac = pfFraction[i];
            if (nearEdge)
            {
                photoAbsorp += AbsorptionCalc.Absorption(z, en1m) * frac;
                photoAbsorp2 += AbsorptionCalc.Absorption(z, en1p) * frac;
            }
            else
            {
                photoAbsorp += AbsorptionCalc.Absorption(z, energy) * frac;
            }
            rayleighAbsorp += AbsorptionCalc.Rayleigh(z, energy) * frac;
            comptonAbsorp += AbsorptionCalc.Compton(z, energy) * frac;
        }
        totalAbsorp = photoAbsorp + rayleighAbsorp + comptonAbsorp;
        if (nearEdge)
            totalAbsorp2 = photoAbsorp2 + rayleighAbsorp + comptonAbsorp;
        else
            totalAbsorp2 = 0;
        photoAbsorptionTextBox.Text = nearEdge ? $"{photoAbsorp:G3}cm²/g  ->  {photoAbsorp2:G3}cm²/g" : $"{photoAbsorp:G3}cm²/g";
        rayleighAbsorptionTextBox.Text = $"{rayleighAbsorp:G3}cm²/g";
        comptonAbsorptionTextBox.Text = $"{comptonAbsorp:G3}cm²/g";
        totalAbsorptionTextBox.Text = nearEdge ? $"{totalAbsorp:G3}cm²/g  ->  {totalAbsorp2:G3}cm²/g" : $"{totalAbsorp:G3}cm²/g";
        DisplayPathAt90();
    }

    private static void DisplayPathAt90()
    {
        if (nbEl == 0)
        {
            pathAt90AbsorptionTextBox.Text = "";
            return;
        }
        if (!double.TryParse(DensityTextBox2.Text, out var density) || density <= 0)
        {
            pathAt90AbsorptionTextBox.Text = "Density required";
            return;
        }
        float path = 0;
        float pathp = 0;
        if (totalAbsorp > 0)
            path = (float)(2.302585092994046 / (totalAbsorp * density)); // ln(10) = 2.302585092994046
        if (totalAbsorp2 > 0)
            pathp = (float)(2.302585092994046 / (totalAbsorp2 * density));
        string unit = "";
        if (totalAbsorp > 0)
        {
            if (path >= 100.0f)
            {
                path /= 100.0f;
                pathp /= 100.0f;
                unit = "m";
            }
            else if (path >= 1)
                unit = "cm";
            else if (path >= 0.1f)
            {
                path *= 10.0f;
                pathp *= 10.0f;
                unit = "mm";
            }
            else if (path >= 0.0001f)
            {
                path *= 1.0e4f;
                pathp *= 1.0e4f;
                unit = "µm";
            }
            else
            {
                path *= 1.0e7f;
                pathp *= 1.0e7f;
                unit = "nm";
            }
        }
        if (pathp == 0)
            pathAt90AbsorptionTextBox.Text = $"{path:G3} {unit}";
        else
            pathAt90AbsorptionTextBox.Text = $"{path:G3} {unit}  ->  {pathp:G3} {unit}";
    }
}

