using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
    public static class ResourceLoader
    {
        public static void CopyTo(Stream source, Stream destination, int bufferSize = 2048, bool resetPosition = true)
        {
            long oldPosition1 = -1;
            if (source.CanSeek)
            {
                oldPosition1 = source.Position;
            }
            long oldPosition2 = -1;
            if (destination.CanSeek)
            {
                oldPosition2 = destination.Position;
            }
            byte[] buffer = new byte[bufferSize];
            int read = 0;
            do
            {
                read = source.Read(buffer, 0, buffer.GetLength(0));
                if (read > 0)
                {
                    destination.Write(buffer, 0, read);
                }
            } while (read != 0);
            if (resetPosition)
            {
                if (oldPosition1 != -1)
                {
                    source.Position = oldPosition1;
                }
                if (oldPosition2 != -1)
                {
                    destination.Position = oldPosition2;
                }
            }
        }

        public static Assembly LoadAssembly(string resourcePath,Assembly assembly = null)
        {
            if (!HasResource(resourcePath,assembly))
            {
                return null;
            }
            using (Stream stream = Retrieve(resourcePath, assembly))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return Assembly.Load(reader.ReadBytes((int)stream.Length));
                }
            }
        }

        public static bool HasResource(string resourcePath,Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = typeof(ResourceLoader).Assembly;
            }
            foreach (var path in assembly.GetManifestResourceNames())
            {
                if (path == resourcePath)
                {
                    return true;
                }
            }
            return false;
        }

        public static Stream Retrieve(string resourcePath,Assembly assembly = null)
        {
            if (assembly == null)
            {
                assembly = typeof(ResourceLoader).Assembly;
            }

            if (!HasResource(resourcePath))
            {
                return null;
            }

            bool compressed = false;
            if (HasResource(resourcePath + "_meta"))
            {
                using (var metaStream = assembly.GetManifestResourceStream(resourcePath + "_meta"))
                {
                    int compressedByte = metaStream.ReadByte();
                    compressed = compressedByte == 1;
                }
            }

            if (!compressed)
            {
                return assembly.GetManifestResourceStream(resourcePath);
            }
            else
            {
                MemoryStream finalStream = new MemoryStream();
                using (Stream compressedStream = assembly.GetManifestResourceStream(resourcePath))
                {
                    using (var decompressionStream = new GZipStream(compressedStream,CompressionMode.Decompress))
                    {
                        CopyTo(decompressionStream, finalStream);
                    }
                }
                return finalStream;
            }
        }
    }
}
