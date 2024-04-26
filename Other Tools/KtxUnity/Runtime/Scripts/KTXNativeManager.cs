// Copyright (c) 2019-2022 Andreas Atteneder, All Rights Reserved.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

//    http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Runtime.InteropServices;
using IntPtr = System.IntPtr;
using System.IO;
using WeaverCore.Utilities;
using WeaverCore;
using WeaverCore.Attributes;

namespace KtxUnity
{
    public static class KTXNativeManager
    {
        /*[DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandleA(string moduleName);

        [DllImport("libdl", ExactSpelling = true)]
        static extern IntPtr dlopen(string filename, int flags);*/

        static IntPtr ktx_unity;
        public static IntPtr Ktx_unity
        {
            get
            {
                if (ktx_unity == default)
                {
                    LoadKTXUnity();
                }
                return ktx_unity;
            }
        }
        public static T GetProcInKTXUnity<T>(string symbol) where T : Delegate => GetProc<T>(Ktx_unity, symbol);

        static T GetProc<T>(IntPtr handle, string symbol) where T : Delegate
        {
            var method_handle = NativeLibraryLoader.GetSymbol(handle, symbol);

            if (method_handle == default)
            {
                throw new Exception($"Couldn't find symbol {symbol}");
            }

            return (T)Marshal.GetDelegateForFunctionPointer(
            method_handle,
            typeof(T));
        }

        [OnRuntimeInit]
        static void LoadKTXUnity()
        {
#if UNITY_EDITOR
            ktx_unity = NativeLibraryLoader.GetLoadedHandle("ktx_unity");
#else
        string ktxUnityDest = NativeLibraryLoader.ExportDLL("ktx_unity", typeof(WeaverCore.WeaverAudio).Assembly);

        ktx_unity = NativeLibraryLoader.Load(ktxUnityDest);
#endif
        }

        /*static string ExportDLL(string resourceName)
        {
            string fileName = resourceName;
            string ext = "";
            switch (NativeLibraryLoader.GetCurrentOS())
            {
                case NativeLibraryLoader.OS.Windows:
                    resourceName = resourceName + ".windows";
                    ext = ".dll";
                    break;
                case NativeLibraryLoader.OS.Mac:
                    resourceName = resourceName + ".mac";
                    ext = ".dylib";
                    break;
                case NativeLibraryLoader.OS.Linux:
                    resourceName = resourceName + ".linux";
                    ext = ".so";
                    break;
                default:
                    break;
            }

            var tempDirectory = PathUtilities.AddSlash(new DirectoryInfo(System.IO.Path.GetTempPath()).FullName);

            var fileDest = tempDirectory + fileName + ext;

            if (File.Exists(fileDest))
            {
                File.Delete(fileDest);
            }

            using (var fileStream = File.Create(fileDest))
            {
                if (!ResourceUtilities.Retrieve(resourceName, fileStream, typeof(WeaverCore.WeaverAudio).Assembly))
                {
                    return null;
                }
            }

            return fileDest;
        }*/
    }
} 
