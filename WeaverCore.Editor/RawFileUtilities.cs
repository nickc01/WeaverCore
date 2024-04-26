using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System.Runtime.InteropServices;


namespace WeaverCore.Editor
{
	public static unsafe class RawFileUtilities
	{
        [DllImport("libc", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr creat(string pathname, int mode);

        [DllImport("libc", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr open(string pathname, int flags, int mode);

        [DllImport("libc", ExactSpelling = true, SetLastError = true)]
        static extern int read(IntPtr fd, IntPtr data, int length);

        [DllImport("libc", ExactSpelling = true, SetLastError = true)]
        static extern int write(IntPtr fd, IntPtr data, int length);

        [DllImport("libc", ExactSpelling = true, SetLastError = true)]
        static extern int close(IntPtr fd);
        
        [DllImport("libc", ExactSpelling = true)]
        static extern string strerror(int errnum);

        public static IntPtr CreateFile(string pathname)
        {
            const int mode = 0000004;

            return creat(pathname, mode);
        }

        public static IntPtr OpenFile(string pathname, int flags = 577, int mode = 644)
        {
            return open(pathname, flags, mode);
        }

        public static string ReadFromFile(IntPtr fd) {
            const int BUFFER_SIZE = 1024;

            var result = new System.Text.StringBuilder();

            IntPtr buffer = default;
            try
            {
                buffer = UnmanagedUtilities.Malloc<byte>(BUFFER_SIZE, Allocator.Temp);

                while (true)
                {
                    var readAmount = read(fd, buffer, BUFFER_SIZE);
                    if (readAmount > 0) {
                        result.Append(System.Text.Encoding.UTF8.GetString((byte*)buffer, readAmount));
                    }

                    if (readAmount < BUFFER_SIZE) {
                        break;
                    }
                }

                return result.ToString();
            }
            finally
            {
                UnmanagedUtilities.Free(buffer, Allocator.Temp);
            }
        }

        public static void WriteToFile(IntPtr fd, string input) {
            var data = System.Text.Encoding.UTF8.GetBytes(input);

            fixed (byte* dataPtr = data) {
                int writtenBytes = write(fd, (IntPtr)dataPtr, data.Length);

                if (writtenBytes < 0) {
                    var errno = Marshal.GetLastWin32Error();
                    UnityEngine.Debug.Log("Error writing to file: " + errno);
                }
            }
        }

        public static int CloseFile(IntPtr fd)
        {
            return close(fd);
        }
    }
}