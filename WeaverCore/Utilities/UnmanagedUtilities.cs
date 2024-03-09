using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using WeaverCore.Interfaces;
using System.Reflection;
using WeaverCore.Attributes;
using System.Text;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;

namespace WeaverCore.Utilities
{
    public static unsafe class UnmanagedUtilities
    {
        public sealed class UnmanagedStreamWrapper : IDisposable
        {
            public readonly Stream SourceStream;
            public readonly IntPtr DataPtr;
            public readonly long DataLength;

            public UnmanagedStreamWrapper(Stream source) : this(source, out var _, out var _) { }

            public UnmanagedStreamWrapper(Stream source, out IntPtr dataPtr, out long dataLength)
            {
                SourceStream = source;

                if (source is UnmanagedMemoryStream ums)
                {
                    DataPtr = (IntPtr)ums.PositionPointer;
                    DataLength = ums.Length;
                }
                else
                {
                    DataPtr = Marshal.AllocHGlobal((int)source.Length);
                    DataLength = source.Length;

                    using (var ustream = new UnmanagedMemoryStream((byte*)DataPtr, DataLength,DataLength, FileAccess.ReadWrite))
                    {
                        source.CopyTo(ustream);
                    }
                }

                dataPtr = DataPtr;
                dataLength = DataLength;
            }

            public void Dispose()
            {
                if (!(SourceStream is UnmanagedMemoryStream))
                {
                    Marshal.FreeHGlobal(DataPtr);
                }
            }
        }

        static Func<Assembly, IntPtr> InternalGetReferencedAssemblies;

        static bool initialized = false;

        static Type monoAssemblyType;
        static Type RuntimeGPtrArrayHandleType;
        static Type safeGPtrArrayHandleType;
        static Type runtimeMarshalType;

        static PropertyInfo LengthGetter;
        static PropertyInfo PtrIndexer;

        [StructLayout(LayoutKind.Sequential)]
        internal struct MonoAssemblyName
        {
            public IntPtr name;
            public IntPtr culture;
            public IntPtr hash_value;
            public IntPtr public_key;
            public byte public_key_token1;
            public byte public_key_token2;
            public byte public_key_token3;
            public byte public_key_token4;
            public byte public_key_token5;
            public byte public_key_token6;
            public byte public_key_token7;
            public byte public_key_token8;
            public byte public_key_token9;
            public byte public_key_token10;
            public byte public_key_token11;
            public byte public_key_token12;
            public byte public_key_token13;
            public byte public_key_token14;
            public byte public_key_token15;
            public byte public_key_token16;
            public byte public_key_token17;
            public uint hash_alg;
            public uint hash_len;
            public uint flags;
            public ushort major;
            public ushort minor;
            public ushort build;
            public ushort revision;
            public ushort arch;
        }

        static void Init()
        {
            if (!initialized)
            {
                initialized = true;
                InternalGetReferencedAssemblies = ReflectionUtilities.MethodToDelegate<Func<Assembly, IntPtr>>(typeof(Assembly).GetMethod("InternalGetReferencedAssemblies", BindingFlags.NonPublic | BindingFlags.Static));

                monoAssemblyType = typeof(Assembly).Assembly.GetType();

                safeGPtrArrayHandleType = monoAssemblyType.Assembly.GetType("Mono.SafeGPtrArrayHandle");

                LengthGetter = safeGPtrArrayHandleType.GetProperty("Length", BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var prop in safeGPtrArrayHandleType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (prop.GetIndexParameters().Length > 0)
                    {
                        PtrIndexer = prop;
                        break;
                    }
                }
            }
        }

        internal unsafe static string PtrToUtf8String(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return string.Empty;
            }
            byte* ptr2 = (byte*)((void*)ptr);
            int num = 0;
            try
            {
                while (*(ptr2++) != 0)
                {
                    num++;
                }
            }
            catch (NullReferenceException)
            {
                throw new ArgumentOutOfRangeException("ptr", "Value does not refer to a valid string.");
            }
            return new string((sbyte*)((void*)ptr), 0, num, Encoding.UTF8);
        }

        public static string[] GetAssemblyReferencesQuick(Assembly assembly)
        {
            Init();
            assembly.GetReferencedAssemblies();
            var handle = Activator.CreateInstance(safeGPtrArrayHandleType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance, null, new object[] { InternalGetReferencedAssemblies(assembly) }, null, null);
            using ((IDisposable)handle)
            {
                var length = (int)LengthGetter.GetValue(handle);

                string[] assemblyNames = new string[length];

                var indexContainer = new object[] { 0 };

                for (int i = 0; i < length; i++)
                {
                    indexContainer[0] = i;
                    var ptr = (IntPtr)PtrIndexer.GetValue(handle, indexContainer);
                    MonoAssemblyName* native = (MonoAssemblyName*)(void*)(ptr);

                    assemblyNames[i] = PtrToUtf8String(native->name);
                }
                return assemblyNames;
            }
        }

        public static unsafe IntPtr Malloc(long size, int alignment, Allocator allocator)
        {
            return (IntPtr)UnsafeUtility.Malloc(size, alignment, allocator);
        }

        public static unsafe IntPtr Malloc<T>(long size, Allocator allocator) where T : struct
        {
            return Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
        }

        public static unsafe void Free(IntPtr ptr, Allocator allocator)
        {
            UnsafeUtility.Free((void*)ptr, allocator);
        }

        public static unsafe byte IndexPtr(IntPtr ptr, int index)
        {
            return ((byte*)ptr)[index];
        }
    }
}