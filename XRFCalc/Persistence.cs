using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace XRFCalc;

public static class Persistence
{
    private const string FileName = "XRFCalc.ini";
    private static readonly string[] Empty = ["AutoCap = true", "Formula = ", "Radiation = ", "Density = ..."];
    private static IStorageFolder? documentsFolder;
    private static string InitFilePath = "";
    public static bool Initialized { get; private set; }
    public static double SavedFactor { get; set; } = 1.0;
    public static void Initialize(IStorageProvider? storageProvider)
    {
        if (storageProvider == null) return;
        Task<IStorageFolder?> folder = storageProvider.TryGetWellKnownFolderAsync(WellKnownFolder.Documents);
        documentsFolder = folder.Result;
        try
        {
            if (documentsFolder == null) return;
            {
                string dir = documentsFolder.Path.LocalPath;
                InitFilePath = dir + "/";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                if (!File.Exists(InitFilePath + FileName))
                {
                    File.WriteAllLines(InitFilePath + FileName, Empty);
                }
                Initialized = true;
            }
        }
        catch (Exception)
        {
            return;     // ignore for now
        }
    }

    public static string[]? ReadIniFile()
    {
        try
        {
            if (!Initialized) return null;
            return File.ReadAllLines(InitFilePath + FileName);
        }
        catch (Exception)
        {       // ignore for now
        }
        return null;
    }

    public static void SaveIniFile(IEnumerable<string> data)
    {
        if (!Initialized) return;
        try
        {
            File.WriteAllLines(InitFilePath + FileName, data);
        }
        catch (Exception)
        {       // ignore for now
        }
    }
}