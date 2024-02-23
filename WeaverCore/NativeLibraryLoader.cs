using System.Runtime.InteropServices;
using System;
using System.IO;

namespace WeaverCore
{
    public unsafe static class NativeLibraryLoader
	{
        public enum OS
        {
            Windows,
            Mac,
            Linux
        }


        #region WINDOWS_FUNCTIONS
        [DllImport("kernel32", EntryPoint = "LoadLibraryA", SetLastError = true)]
        static extern IntPtr LoadLibraryA(string dllToLoad);

        [DllImport("kernel32")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32")]
        static extern bool FreeLibrary(IntPtr hModule);
        #endregion

        #region LINUX_MAC_FUNCTIONS
        [DllImport("libdl", ExactSpelling = true)]
        public static extern IntPtr dlopen(string filename, int flags);

        [DllImport("libdl", ExactSpelling = true)]
        public static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl", ExactSpelling = true)]
        public static extern int dlclose(IntPtr handle);

        [DllImport("libdl")]
        public static extern string dlerror();
        #endregion


        static bool osSet = false;
        static OS os;

        public static OS GetCurrentOS()
        {
            if (!osSet)
            {
                osSet = true;
                string windir = Environment.GetEnvironmentVariable("windir");
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
                }
            }

            return os;
        }

        public static IntPtr Load(string dllToLoad)
        {
            IntPtr result;
            switch (GetCurrentOS())
            {
                case OS.Windows:
                    result = LoadLibraryA(dllToLoad);

                    if (result == default)
                    {
                        throw new Exception($"Failed to load dll {dllToLoad} {Marshal.GetLastWin32Error()}");
                    }

                    break;
                default:
                    result = dlopen(dllToLoad, 0x002);
                    if (result == default)
                    {
                        throw new Exception($"Failed to load so {dllToLoad} {dlerror()}");
                    }
                    break;
            }

            return result;
        }

        public static IntPtr GetSymbol(IntPtr handle, string symbol)
        {
            switch (GetCurrentOS())
            {
                case OS.Windows:
                    return GetProcAddress(handle, symbol);
                default:
                    return dlsym(handle, symbol);
            }
        }

        public static bool Free(IntPtr handle)
        {
            switch (GetCurrentOS())
            {
                case OS.Windows:
                    return FreeLibrary(handle);
                default:
                    return dlclose(handle) == 0;
            }
        }
    }
}
