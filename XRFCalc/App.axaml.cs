using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XRFCalc.ViewModels;
using XRFCalc.Views;

namespace XRFCalc;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    
    public override void OnFrameworkInitializationCompleted()
    {
        TabControl? mainTab = null;
        IStorageProvider? storageProvider = null;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
            storageProvider = desktop.MainWindow.StorageProvider;
            IEnumerable<ILogical> test = desktop.MainWindow.GetLogicalChildren();
            foreach (var item in test)
            {
                if (item is MainView mv)
                {
                    mainTab = mv.FindControl<TabControl>("MainTabControl");
                    break;
                }
            }
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
            mainTab = singleViewPlatform.MainView.FindControl<TabControl>("MainTabControl");
            var top = TopLevel.GetTopLevel(singleViewPlatform.MainView);
            storageProvider = top?.StorageProvider;

        }
        Debug.Assert(mainTab != null);
        if (mainTab != null)
            XRFCalcUIDefinition.InitializeApplication(mainTab, storageProvider);

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}