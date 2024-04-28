using System.Runtime.InteropServices;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using WeaverCore.Utilities;
using System.Reflection;
using System.IO.Pipes;
using pdj.tiny7z.Archive;
using System.Linq;

namespace WeaverCore
{
    public unsafe static class NativeLibraryLoader
	{
        const int RTLD_NOW = 0x00002;
        const int RTLD_NOLOAD = 0x00004;

        public enum OS
        {
            Windows,
            Mac,
            Linux
        }


        #region WINDOWS_FUNCTIONS
        [DllImport("kernel32", EntryPoint = "LoadLibraryW", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibraryW(string dllToLoad);

        //[DllImport("kernel32", EntryPoint = "LoadLibraryA", SetLastError = true, CharSet = CharSet.Ansi)]
        //static extern IntPtr LoadLibraryA(string dllToLoad);

        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32")]
        static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr GetModuleHandleW(string moduleName);
        #endregion

        #region LINUX_MAC_FUNCTIONS
        [DllImport("libdl", ExactSpelling = true)]
        static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl", ExactSpelling = true)]
        static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl", ExactSpelling = true)]
        static extern int dlclose(IntPtr handle);

        [DllImport("libdl", ExactSpelling = true)]
        public static extern string dlerror();

        [DllImport("libdl.so.2", ExactSpelling = true, EntryPoint = "dlopen")]
        public static extern IntPtr dlopenV2(string filename, int flags);

        [DllImport("libdl.so.2", ExactSpelling = true, EntryPoint = "dlsym")]
        public static extern IntPtr dlsymV2(IntPtr handle, string symbol);

        [DllImport("libdl.so.2", ExactSpelling = true, EntryPoint = "dlclose")]
        public static extern int dlcloseV2(IntPtr handle);

        [DllImport("libdl.so.2", ExactSpelling = true, EntryPoint = "dlerror")]
        public static extern string dlerrorV2();

        #endregion


        static bool osSet = false;
        static OS os;

        static int linuxLoadMode = -1;

        public static OS GetCurrentOS()
        {
            if (!osSet)
            {
                osSet = true;

                if (SystemInfo.operatingSystem.ToLower().Contains("windows"))
                {
                    os = OS.Windows;
                    //extension = ".bundle.win";
                }
                else if (SystemInfo.operatingSystem.ToLower().Contains("mac") || SystemInfo.operatingSystem.ToLower().Contains("apple"))
                {
                    os = OS.Mac;
                    //extension = ".bundle.mac";
                }
                else// if (SystemInfo.operatingSystem.ToLower().Contains("Linux"))
                {
                    os = OS.Linux;
                    //extension = ".bundle.unix";
                }

                /*string windir = Environment.GetEnvironmentVariable("windir");
                if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
                {
                    os = OS.Windows;
                }
                else if (File.Exists(@"/proc/sys/kernel/ostype"))
                {
                    string osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                    if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                    {
                        // Note: Android gets here too
                        os = OS.Linux;
                    }
                    else
                    {
                        throw new Exception(osType);
                    }
                }
                else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
                {
                    // Note: iOS gets here too
                    os = OS.Mac;
                }
                else
                {
                    throw new Exception();
                }*/
            }

            return os;
        }

        public static IntPtr Load(string dllToLoad)
        {
            //UnityEngine.Debug.Log("Attempting to load DLL = " + dllToLoad);
            //UnityEngine.Debug.Log("Current OS = " + GetCurrentOS());
            IntPtr result;
            switch (GetCurrentOS())
            {
                case OS.Windows:
                    result = LoadLibraryW(dllToLoad);

                    if (result == default)
                    {
                        throw new Exception($"Failed to load dll {dllToLoad} {Marshal.GetLastWin32Error()}");
                    }

                    break;
                default:
                    if (linuxLoadMode == 1)
                    {
                        result = dlopen(dllToLoad, RTLD_NOW);
                        if (result == default)
                        {
                            throw new Exception($"Failed to load so {dllToLoad} {dlerror()}");
                        }

                    }
                    else if (linuxLoadMode == 2)
                    {
                        result = dlopenV2(dllToLoad, RTLD_NOW);
                        if (result == default)
                        {
                            throw new Exception($"Failed to load so {dllToLoad} {dlerror()}");
                        }
                    }
                    else
                    {
                        try
                        {
                            result = dlopen(dllToLoad, RTLD_NOW);
                            linuxLoadMode = 1;
                            if (result == default)
                            {
                                throw new Exception($"Failed to load so {dllToLoad} {dlerror()}");
                            }
                        }
                        catch (DllNotFoundException)
                        {
                            result = dlopenV2(dllToLoad, RTLD_NOW);
                            linuxLoadMode = 2;
                            if (result == default)
                            {
                                throw new Exception($"Failed to load so {dllToLoad} {dlerror()}");
                            }
                        }
                    }

                    break;
            }

            //UnityEngine.Debug.Log($"Loaded DLL {dllToLoad} with Handle = {result}");

            return result;
        }

        public static IntPtr GetSymbol(IntPtr handle, string symbol)
        {
            //UnityEngine.Debug.Log($"Loaded symbol {symbol} from handle = {handle}");
            switch (GetCurrentOS())
            {
                case OS.Windows:
                    return GetProcAddress(handle, symbol);
                default:
                    if (linuxLoadMode == 1)
                    {
                        return dlsym(handle, symbol);
                    }
                    else if (linuxLoadMode == 2)
                    {
                        return dlsymV2(handle, symbol);
                    }
                    else
                    {
                        try
                        {
                            var result = dlsym(handle, symbol);
                            linuxLoadMode = 1;
                            return result;
                        }
                        catch (DllNotFoundException)
                        {
                            var result = dlsymV2(handle, symbol);
                            linuxLoadMode = 2;
                            return result;
                        }
                    }
                    //return dlsym(handle, symbol);
            }
        }

        public static bool Free(IntPtr handle)
        {
            switch (GetCurrentOS())
            {
                case OS.Windows:
                    return FreeLibrary(handle);
                default:
                    if (linuxLoadMode == 1)
                    {
                        return dlclose(handle) == 0;
                    }
                    else if (linuxLoadMode == 2)
                    {
                        return dlcloseV2(handle) == 0;
                    }
                    else
                    {
                        try
                        {
                            var result = dlclose(handle) == 0;
                            linuxLoadMode = 1;
                            return result;
                        }
                        catch (DllNotFoundException)
                        {
                            var result = dlcloseV2(handle) == 0;
                            linuxLoadMode = 2;
                            return result;
                        }
                    }
                    //return dlclose(handle) == 0;
            }
        }

        public static string ExportDLL(string resourceName, Assembly assemblyToLoadFrom)
        {
            return ExportDLL(resourceName, assemblyToLoadFrom, NativeLibraryLoader.GetCurrentOS());
        }

        public static string ExportDLL(string resourceName, Assembly assemblyToLoadFrom, OS os)
        {
            try
            {
                //UnityEngine.Debug.Log("Beginning Export of resource = " + resourceName);
                //UnityEngine.Debug.Log("OS = " + os);
                //Debug.Log("Assembly To Load From = " + assemblyToLoadFrom.GetName().Name);
                string fileName = resourceName;
                List<string> exts = new List<string>();
                switch (os)
                {
                    case NativeLibraryLoader.OS.Windows:
                        resourceName = resourceName + ".windows";
                        exts.Add(".dll");
                        break;
                    case NativeLibraryLoader.OS.Mac:
                        resourceName = resourceName + ".mac";
                        exts.Add(".dylib");
                        exts.Add(".bundle.7z");
                        break;
                    case NativeLibraryLoader.OS.Linux:
                        resourceName = resourceName + ".linux";
                        exts.Add(".so");
                        break;
                    default:
                        break;
                }

                foreach (var ext in exts)
                {
                    //Debug.Log("Searching for resource name = " + resourceName + ext);
                    if (!ResourceUtilities.HasResource(resourceName + ext, assemblyToLoadFrom))
                    {
                        //Debug.Log("Not found. Continuing");
                        continue;
                    }

                    //Debug.Log($"{resourceName + ext} found!");

                    if (ext == ".bundle.7z")
                    {
                        //Debug.Log("bundle.7z found");
                        var tempDirectory = new DirectoryInfo(System.IO.Path.GetTempPath()).CreateSubdirectory(resourceName);
                        //Debug.Log("Temp Dir = " + tempDirectory.FullName);
                        if (Directory.Exists(tempDirectory.FullName))
                        {
                            //Debug.Log("Deleting and rebuilding temp dir");
                            tempDirectory.Delete(true);
                            tempDirectory.Create();
                        }

                        using (var zipStream = ResourceUtilities.Retrieve(resourceName + ext, assemblyToLoadFrom))
                        {
                            //pdj.tiny7z.Archive.SevenZipArchive test = new pdj.tiny7z.Archive.SevenZipArchive()
                            using (var sevenZipStream = new SevenZipArchive(zipStream, FileAccess.Read))
                            {
                                using (var extractor = sevenZipStream.Extractor())
                                {
                                    extractor.ExtractArchive(tempDirectory.FullName);
                                }
                            }
                        }

                        //Debug.Log("Extracted Bundle Contents To Directory = " + tempDirectory.FullName);

                        var subDir = tempDirectory.EnumerateDirectories().FirstOrDefault();
                        tempDirectory = subDir;
                        /*foreach (var folder in )
                        {
                            Debug.Log("SUB DIR = " + folder.Name);

                            if (folder.Name == resourceName + ext)
                            {
                                Debug.Log("Found Final Bundle Dir = " + folder.FullName);
                                tempDirectory = folder;
                            }
                        }*/

                        /*foreach (var folder in tempDirectory.EnumerateDirectories())
                        {
                            Debug.Log("Folder in Final = " + folder.Name);
                        }

                        foreach (var file in tempDirectory.EnumerateFiles())
                        {
                            Debug.Log("File in Final = " + file.Name);
                        }*/

                        return tempDirectory.FullName;
                    }
                    else
                    {
                        //Debug.Log("bundle.7z not found");
                        var tempDirectory = PathUtilities.AddSlash(new DirectoryInfo(System.IO.Path.GetTempPath()).FullName);
                        //Debug.Log("Temp Dir = " + tempDirectory);
                        var fileDest = tempDirectory + fileName + ext;
                        //Debug.Log("File dest = " + fileDest);
                        if (File.Exists(fileDest))
                        {
                            UnityEngine.Debug.Log("Deleting already existing file = " + fileDest);
                            File.Delete(fileDest);
                        }

                        using (var fileStream = File.Create(fileDest))
                        {
                            if (!ResourceUtilities.Retrieve(resourceName + ext, fileStream, assemblyToLoadFrom))
                            {
                                UnityEngine.Debug.LogError("Error: Failed to retrieve resource and export it to file");
                                return null;
                            }
                        }

                        UnityEngine.Debug.Log("Finished Exporting = " + fileDest);

                        return fileDest;
                    }

                    /*UnityEngine.Debug.Log("Beginning Exporting Resource = " + resourceName + ext);

                    var tempDirectory = PathUtilities.AddSlash(new DirectoryInfo(System.IO.Path.GetTempPath()).FullName);

                    UnityEngine.Debug.Log("Temp Dir = " + tempDirectory);

                    var fileDest = tempDirectory + fileName + ext;

                    UnityEngine.Debug.Log("Export File Dest = " + fileDest);

                    if (ext != ".bundle.zip" && File.Exists(fileDest))
                    {
                        UnityEngine.Debug.Log("Deleting already existing file = " + fileDest);
                        File.Delete(fileDest);
                    }

                    if (ext == ".bundle.zip" && Directory.Exists(fileDest))
                    {

                    }*/

                    /*using (var fileStream = File.Create(fileDest))
                    {
                        if (!ResourceUtilities.Retrieve(resourceName + ext, fileStream, assemblyToLoadFrom))
                        {
                            UnityEngine.Debug.LogError("Error: Failed to retrieve resource and export it to file");
                            return null;
                        }
                    }

                    UnityEngine.Debug.Log("Finished Exporting = " + fileDest);

                    return fileDest;*/
                }

                UnityEngine.Debug.Log("1: Some Error occured. Returning null");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                UnityEngine.Debug.Log("2: Some Error occured. Returning null");
                return null;
            }
        }

        public static IntPtr GetLoadedHandle(string dll){
            switch(GetCurrentOS())
            {
                case OS.Windows:
                    return GetModuleHandleW(dll.Replace('/', '\\'));
                default:
                    if (linuxLoadMode == 1)
                    {
                        return dlopen(dll, RTLD_NOLOAD);
                    }
                    else if (linuxLoadMode == 2)
                    {
                        return dlopenV2(dll, RTLD_NOLOAD);
                    }
                    else
                    {
                        try
                        {
                            var result = dlopen(dll, RTLD_NOLOAD);
                            linuxLoadMode = 1;
                            return result;
                        }
                        catch (DllNotFoundException)
                        {
                            var result = dlopenV2(dll, RTLD_NOLOAD);
                            linuxLoadMode = 2;
                            return result;
                        }
                    }
                    break;
            }
        }
    }
}
