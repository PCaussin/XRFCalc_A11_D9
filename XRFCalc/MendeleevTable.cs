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
using System.Runtime.CompilerServices;
namespace XRFCalc;

internal partial class XRFCalcContent
{
    private static int firstColumn;
    private static int lastColumn;
    internal static Grid TableView;
    private static Grid buttons;
    private static Grid MendeleevTable;
    private static Button leftA;
    private static Button left;
    private static Button rightA;
    private static Button right;
    private static Button cancelButton;

    static int[,] Znum = new int[8, 18] {   { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 },
                                            { 3, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 6, 7, 8, 9,10 },
                                            {11,12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,13,14,15,16,17,18 },
                                            {19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36 },
                                            {37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54 },
                                            {55,56,57,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86 },
                                            {87,88,89,90,91,92,93,94,95,96,97,98, 0, 0, 0, 0, 0, 0 },
                                            { 0, 0, 0,58,59,60,61,62,63,64,65,66,67,68,69,70,71, 0 },    };

    private static Button[] elementButons = new Button[Chemistry.MaxElementsD + 1];

    internal static void InitializeMendeleevTable()
    {
        if (!XRFCalcUIDefinition.LandscapeOrientation.HasValue) return;

        InitializeButtons();

        bool orientationChanged =
            !previousOrientationKnown || previousLandscapeOrientation != XRFCalcUIDefinition.LandscapeOrientation;
        previousLandscapeOrientation = XRFCalcUIDefinition.LandscapeOrientation.Value;
        previousOrientationKnown = true;
        if (orientationChanged)
        {
            if (previousLandscapeOrientation)
            {
                firstColumn = 0;
                lastColumn = 17;
            }
            else
            {
                firstColumn = 0;
                lastColumn = 8;
            }
        }
        int numColumns = lastColumn - firstColumn + 1;

        MendeleevTable = new Grid();
        MendeleevTable.ColumnDefinitions.Capacity = numColumns;
        MendeleevTable.RowDefinitions.Capacity = 8;
        for (int i = 0; i < numColumns; i++)
            MendeleevTable.ColumnDefinitions.Add((new ColumnDefinition() { Width = new GridLength(1.0 / numColumns, GridUnitType.Star) }));
        for (int i = 0; i < 8; i++)
            MendeleevTable.RowDefinitions.Add((new RowDefinition() { Height = new GridLength(1.0 / 8, GridUnitType.Star) }));

        buttons = new Grid();
        buttons.ColumnDefinitions.Capacity = 10;
        buttons.RowDefinitions.Capacity = 2;
        for (int i = 0; i < 10; i++)
            buttons.ColumnDefinitions.Add((new ColumnDefinition() { Width = new GridLength(1.0 / 10, GridUnitType.Star) }));
        for (int i = 0; i < 2; i++)
            buttons.RowDefinitions.Add((new RowDefinition() { Height = new GridLength(1.0 / 2, GridUnitType.Star) }));

        leftA.SetValue(Grid.ColumnProperty, 0);
        leftA.SetValue(Grid.RowProperty, 0);
        left.SetValue(Grid.ColumnProperty, 1);
        left.SetValue(Grid.RowProperty, 0);
        right.SetValue(Grid.ColumnProperty, 8);
        right.SetValue(Grid.RowProperty, 0);
        right.HorizontalAlignment = HorizontalAlignment.Right;
        rightA.SetValue(Grid.ColumnProperty, 9);
        rightA.SetValue(Grid.RowProperty, 0);
        rightA.HorizontalAlignment = HorizontalAlignment.Right;

        if (!XRFCalcUIDefinition.LandscapeOrientation.Value)
        {
            leftA.SetValue(Grid.ColumnProperty, 0);
            leftA.SetValue(Grid.RowProperty, 0);
            left.SetValue(Grid.ColumnProperty, 1);
            left.SetValue(Grid.RowProperty, 0);
            right.SetValue(Grid.ColumnProperty, 8);
            right.SetValue(Grid.RowProperty, 0);
            rightA.SetValue(Grid.ColumnProperty, 9);
            rightA.SetValue(Grid.RowProperty, 0);
            buttons.Children.Add(leftA);
            buttons.Children.Add(left);
            buttons.Children.Add(right);
            buttons.Children.Add(rightA);
        }
        cancelButton.SetValue(Grid.ColumnProperty, XRFCalcUIDefinition.LandscapeOrientation.Value ? 8 : 3);
        cancelButton.SetValue(Grid.ColumnSpanProperty, 2);
        cancelButton.SetValue(Grid.RowProperty, 0);
        buttons.Children.Add(cancelButton);

        buttons.SetValue(Grid.RowProperty, 0);
        MendeleevTable.SetValue(Grid.RowProperty, 1);

        TableView = new Grid();
        TableView.RowDefinitions.Capacity = 2;
        TableView.ColumnDefinitions.Capacity = 1;
        TableView.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.25, GridUnitType.Star) });
        TableView.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.75, GridUnitType.Star) });
        TableView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        TableView.Children.Add(buttons);
        TableView.Children.Add(MendeleevTable);

        left.Click += Left_Click;
        leftA.Click += LeftA_Click;
        right.Click += Right_Click;
        rightA.Click += RightA_Click;
        cancelButton.Click += (s, e) =>
        {
            int idx = XRFCalcUIDefinition.TabIndex;
            if (idx >= 0 && idx <= 1 && XRFCalcUIDefinition.MainTab != null && XRFCalcUIDefinition.MainTab.Items[idx] is TabItem t)
                t.Content = idx == 0 ? ChemGrid : RadGrid;
        };
        for (int z = 1; z <= Chemistry.MaxElementsD; z++)
        {
            elementButons[z] = new Button()
            {
                Content = Chemistry.gacElements[z],
                Background = new SolidColorBrush(Chemistry.elementsColor[z]),
                Foreground = new SolidColorBrush(Chemistry.elementsForeColor[z]),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                //Margin = new Thickness(2),
            };
            elementButons[z].Click += XRFCalcContent_Click;
            elementButons[z].Tag = z;
        }

        FillTable();
    }

    private static void InitializeButtons()
    {
        TableView = new();
        buttons = new();
        MendeleevTable = new Grid();
        leftA = new() { Content = "<<" };
        rightA = new() { Content = ">>" };
        left = new() { Content = "<" };
        right = new() { Content = ">" };
        cancelButton = new()
        {
            Content = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            //Margin = new Thickness(2),
        };
    }


    private static bool previousOrientationKnown = false;
    private static bool previousLandscapeOrientation = false;
    private static int prevFirstColumn = -1;

    private static void FillTable()
    {
        if (!XRFCalcUIDefinition.LandscapeOrientation.HasValue) return;
        bool orientationChanged = !previousOrientationKnown || previousLandscapeOrientation != XRFCalcUIDefinition.LandscapeOrientation;
        previousLandscapeOrientation = XRFCalcUIDefinition.LandscapeOrientation.Value;
        previousOrientationKnown = true;
        if (orientationChanged)
        {
            if (previousLandscapeOrientation)
            {
                firstColumn = 0;
                lastColumn = 17;
            }
            else
            {
                firstColumn = 0;
                lastColumn = 8;
            }
        }

        prevFirstColumn = firstColumn;
        MendeleevTable.Children.Clear();
        for (int i = firstColumn; i <= lastColumn; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                int z = Znum[j, i];
                if (z > 0)
                {
                    elementButons[z].SetValue(Grid.ColumnProperty, i - firstColumn);
                    elementButons[z].SetValue(Grid.RowProperty, j);
                    MendeleevTable.Children.Add(elementButons[z]);
                }
            }
        }
    }

    //private static bool xraydialogshown = false;

    private static void XRFCalcContent_Click(object? sender, EventArgs e)
    {
        if (sender is Button bt && bt.Tag is int z)
        {
            if (XRFCalcUIDefinition.TabIndex == 0)
            {
                ignoreFormulaOnce = true;
                int semicolonIndex = formulaString.IndexOf(';');
                if (semicolonIndex >= 0)
                    formulaString = formulaString.Substring(0, semicolonIndex) + Chemistry.gacElements[z];
                else
                    formulaString += Chemistry.gacElements[z];
                formulaTextBox.Text = formulaString;
                formulaTextBox.CaretIndex = formulaTextBox.Text.Length;
                CalcElements(formulaString);
                SaveChemistry();
                if (XRFCalcUIDefinition.MainTab != null && XRFCalcUIDefinition.MainTab.Items[0] is TabItem t)
                    t.Content = ChemGrid;
            }
            else if (XRFCalcUIDefinition.TabIndex == 1)
            {
                DisplayElementLines(z);
            }
        }
    }

    private static void RightA_Click(object? sender, EventArgs e)
    {
        if (lastColumn >= 17) return;
        firstColumn = 9;
        lastColumn = 17;
        FillTable();
    }

    private static void Right_Click(object? sender, EventArgs e)
    {
        if (lastColumn >= 17) return;
        firstColumn++;
        lastColumn++;
        FillTable();
    }

    private static void LeftA_Click(object? sender, EventArgs e)
    {
        if (firstColumn == 0) return;
        firstColumn = 0;
        lastColumn = 8;
        FillTable();
    }

    private static void Left_Click(object? sender, EventArgs e)
    {
        if (firstColumn <= 0) return;
        firstColumn--;
        lastColumn--;
        FillTable();
    }


    private static void DisplayElementLines(int z)
    {
        if (!XRFCalcUIDefinition.LandscapeOrientation.HasValue) return;
        Grid marginGrid = new Grid();

        Grid lineGrid = new Grid();
        int numColumns = XRFCalcUIDefinition.LandscapeOrientation.Value ? 6 : 4;
        int numRows = (int)Math.Ceiling((double)(AbsorptionCalc.AllLines(z, false).Count) / numColumns) + 1;
        for (int i = 0; i < numColumns; i++)
            lineGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        for (int i = 0; i < numRows; i++)
        {
            lineGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
        }
        marginGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.5, GridUnitType.Star) });
        marginGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(50 * numRows, GridUnitType.Pixel) });
        marginGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0.5, GridUnitType.Star) });
        marginGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
        marginGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60 * numColumns, GridUnitType.Pixel) });
        marginGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0.5, GridUnitType.Star) });
        lineGrid.SetValue(Grid.RowProperty, 1);
        lineGrid.SetValue(Grid.ColumnProperty, 1);
        marginGrid.Children.Add(lineGrid);
        TextBlock elementLabel = new()
        {
            Text = Chemistry.gacElements[z],
            FontSize = 32,
            FontWeight = FontWeight.Bold,
            //Margin = new Thickness(8, 1),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        elementLabel.SetValue(Grid.RowProperty, 0);
        elementLabel.SetValue(Grid.ColumnProperty, 0);
        lineGrid.Children.Add(elementLabel);

        Button btCancel = new()
        {
            Content = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            //Margin = new Thickness(8, 1),
        };
        btCancel.SetValue(Grid.RowProperty, 0);
        btCancel.SetValue(Grid.ColumnProperty, numColumns - 2);
        btCancel.SetValue(Grid.ColumnSpanProperty, 2);
        btCancel.Click += (s, e) =>
        {
            if (XRFCalcUIDefinition.MainTab != null && XRFCalcUIDefinition.MainTab.Items[1] is TabItem t)
            {
                InitializeMendeleevTable();
                t.Content = TableView;
            }
        };
        lineGrid.Children.Add(btCancel);

        int row = 1;
        int col = 0;
        foreach (var line in AbsorptionCalc.AllLines(z, false))
        {
            Button button = new Button()
            {
                Content = line.Name,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                //Margin = new Thickness(8, 1),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Tag = line,
            };
            button.SetValue(Grid.RowProperty, row);
            button.SetValue(Grid.ColumnProperty, col);
            button.Click += (sender, e) =>
            {
                if (sender is Button b && b.Tag is AbsorptionCalc.LineInfo li)
                {
                    string lineOrEdge = $"{Chemistry.gacElements[z]} {line.Name}";
                    float energy = AbsorptionCalc.FindEnergy(ref lineOrEdge, out int isEdge, out string alt);
                    radiationTextBox.Text = lineOrEdge;
                    altRadiationTextBox.Text = alt;
                    if (XRFCalcUIDefinition.MainTab != null && XRFCalcUIDefinition.MainTab.Items[1] is TabItem t)
                        t.Content = RadGrid;
                }
            };
            lineGrid.Children.Add(button);
            if (line.IsEdge)
            {
                button.Background = new SolidColorBrush(Colors.LightBlue);
                button.Foreground = new SolidColorBrush(Colors.Black);
            }
            col++;
            if (col >= numColumns)
            {
                col = 0;
                row++;
            }
        }
        if (XRFCalcUIDefinition.MainTab != null && XRFCalcUIDefinition.MainTab.Items[1] is TabItem t)
            t.Content = marginGrid;

    }
}
